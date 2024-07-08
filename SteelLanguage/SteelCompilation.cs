using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using SteelLanguage.Exceptions;
using SteelLanguage.Extensions;
using SteelLanguage.Library;
using SteelLanguage.Reflection;
using SteelLanguage.Token;
using SteelLanguage.Token.Fields;
using SteelLanguage.Token.Functions;
using SteelLanguage.Token.Functions.Modifier;
using SteelLanguage.Token.Interaction;
using SteelLanguage.Token.KeyWords;
using SteelLanguage.Token.Terms;
using SteelLanguage.Utils;

namespace SteelLanguage;

public sealed class SteelCompiler
{
    private ILibrary[] _libraries;
    private SteelScript _currentScript;

    public static readonly SteelLibrary Library = new();

    public SteelScript Compile(string tokens)
    {
        return Compile(new StringReader(tokens));
    }

    /// <summary>
    /// Compiles a <see cref="SteelScript"/>
    /// NOTICE: This does not work in multi threaded or async situations. 
    /// </summary>
    /// <param name="reader"></param>
    /// <returns>A compiled <see cref="SteelScript"/> ready for execution</returns>
    public SteelScript Compile(TextReader reader)
    {
        _reader = reader;
        _currentScript = new SteelScript();
        
        // TODO: Should the system library always be imported?
        _currentScript.GetLibraryManager().ImportLibrary(Library);

        using (_reader)
        {
            try
            {
                string line = ReadCleanLine();
                while (line != null)
                {
                    if (string.IsNullOrEmpty(line))
                    {
                        line = ReadCleanLine();
                        continue;
                    }

                    ParseToken(line, _currentScript);
                    line = ReadCleanLine();
                }
            }
#if !DEBUG
            catch (ActionException e)
            {
                if (e.LineNumber == 0)
                    e.LineNumber = CurrentLine;
                throw;
            }
            catch (AggregateException e)
            {
                if (e.InnerException != null && e.InnerExceptions.Count == 1)
                    throw new InvalidCompilationException(CurrentLine, e.InnerException.Message);

                string errors = "\n";
                foreach (Exception innerException in e.InnerExceptions)
                {
                    errors += innerException.Message;
                }

                throw new InvalidCompilationException(CurrentLine, errors);
            }
            catch (Exception e)
            {
                throw new InvalidCompilationException(CurrentLine, e.Message);
            }
#endif
            finally
            {
                CurrentLine = 0;
            }
        }

        _currentScript.PostCompilation();
        return _currentScript;
    }

    #region Tokens

    public void ParseToken(string token, ITokenHolder holder)
    {
        _currentScript.TotalTokens++;
        
        string keywordName = token.Split(new[] { ' ' }, 2, StringSplitOptions.RemoveEmptyEntries)[0].Trim();
        if (_currentScript.GetLibraryManager().HasKeyword(keywordName))
        {
            IKeyword keyword = _currentScript.GetLibraryManager().GetKeyword(keywordName);
            keyword.CompileKeyword(token, this, _currentScript, holder);
            _currentScript.KeyTokens++;
        }
        else
        {
            string[] split = token.SanitizedSplit('=', 2, StringSplitOptions.RemoveEmptyEntries);
            if (split.Length == 1)
            {
                TokenCall call = ParseFunctionCall(token, holder);
                holder.AddCall(call);
            }
            else if (split.Length == 2)
            {
                ParseTerm(token, holder);
            }
            else // TODO: More in depth error logging for this. Should tell them if it was an issue with a function call or type not being found
            {
                throw new InvalidCompilationException(CurrentLine, $"Could not determine token at line {CurrentLine}");
            }
        }
    }

    #endregion

    #region Terms

    internal void ParseTerm(string token, ITokenHolder holder)
    {
        AssignmentKind kind = CompileUtils.DetermineAssignment(token, holder);
        string[] values = CompileUtils.GetTermValues(token);

        LibraryManager manager = _currentScript.GetLibraryManager();
        switch (kind)
        {
            case AssignmentKind.Constant:
            {
                TermType type = manager.GetTermType(values[0]);
                BaseTerm term = type.Construct(values[1], CurrentLine, manager);
                if (!term.Parse(values[2]))
                    throw new InvalidAssignmentException(CurrentLine, term);

                _currentScript.TermTokens++;
                holder.AddTerm(term);
            } break;
            case AssignmentKind.Term:
            {
                TermType type = manager.GetTermType(values[0]);
                BaseTerm from = holder.GetTerm(values[2]);
                if (!from.CanImplicitCastToType(type.Name) && !from.GetTermType().IsSubclassOf(type.Name))
                    throw new InvalidAssignmentException(CurrentLine);

                BaseTerm term;
                if (from.GetTermType().IsSubclassOf(type.Name))
                {
                    TermType constructType = from.GetTermType();
                    term = constructType.Construct(values[1], CurrentLine, manager);
                }
                else
                {
                    term = type.Construct(values[1], CurrentLine, manager);
                }
                Input input = new Input(from);

                AssignmentCall assignmentCall = new AssignmentCall(term, input, holder, CurrentLine);
                _currentScript.TermTokens++;
                holder.AddTerm(term);
                holder.AddCall(assignmentCall);
            } break;
            case AssignmentKind.Function:
            {
                TermType type = manager.GetTermType(values[0]);
                BaseTerm term = type.Construct(values[1], CurrentLine, manager);
                Input input = CompileUtils.HandleToken(values[2], type.Name, holder, this);
                AssignmentCall call = new AssignmentCall(term, input, holder, CurrentLine);
                holder.AddTerm(term);
                holder.AddCall(call);
                _currentScript.TermTokens++;
            } break;
            case AssignmentKind.Assignment:
            {
                BaseTerm term = holder.GetTerm(values[0]);
                string type = term.ValueType;
                Input input = CompileUtils.HandleToken(values[1], type, holder, this);
                AssignmentCall call = new AssignmentCall(term, input, holder, CurrentLine);
                holder.AddCall(call);
                _currentScript.CallTokens++;
            } break;
            case AssignmentKind.Field:
            {
                BaseTerm term = holder.GetTerm(values[0]);
                TermField field = term.GetField(values[1]);
                if (!field.Set)
                    throw new FieldReadOnlyException(CurrentLine, field.Name);
                
                Input input = CompileUtils.HandleToken(values[2], field.Value.Type, holder, this);
                AssignmentCall call = new AssignmentCall(term, field.Name, input, holder, CurrentLine);
                holder.AddCall(call);
                _currentScript.CallTokens++;
            } break;
            default:
                throw new ArgumentOutOfRangeException(nameof(kind), kind, $"Assignment of {token} was invalid");
        }
    }

    #endregion

    #region Function Calls

    public TokenCall ParseFunctionCall(string token, ITokenHolder holder)
    {
        _currentScript.CallTokens++;
        
        // Is this a global or local function?
        TokenKind kind = CompileUtils.GetTokenKind(token, holder);
        if (kind == TokenKind.Function)
        {
            return ParseGlobalCall(token, holder);
        }
        else
        {
            return ParseLocalCall(token, holder);
        }
    }

    private FunctionCall ParseGlobalCall(string token, ITokenHolder holder)
    {
        string[] split = token.SanitizedSplit('(', 2, StringSplitOptions.RemoveEmptyEntries);
        string name = split[0].Trim();
        string prms = split[1].Trim();
        prms = prms.Remove(prms.Length - 1);

        IFunction func = holder.GetFunction(name);
        List<Input> inputTokens = ParseInputTokens(prms, func, holder);

        return new FunctionCall(holder, func, inputTokens, CurrentLine);
    }

    private LocalCall ParseLocalCall(string token, ITokenHolder holder)
    {
        string noPrms = token.SanitizeParenthesis();

        string[] split = token.SplitAt(noPrms.LastIndexOf('.'), 2, StringSplitOptions.RemoveEmptyEntries, ScanDirection.RightToLeft);
        string termToken = split[0].Trim();
        string[] funcToken = split[1].Trim().Remove(0,1).SanitizedSplit('(', 2, StringSplitOptions.RemoveEmptyEntries);

        TermType type = CompileUtils.GetTypeFromToken(termToken, holder, CompileUtils.GetTokenKind(termToken, holder));
        Input term = CompileUtils.HandleToken(termToken, type.Name, holder, this);

        string name = funcToken[0].Trim();
        string prms = funcToken[1].Trim();
        prms = prms.Remove(prms.Length - 1);

        IFunction func = type.GetFunction(name);
        List<Input> inputTokens = new List<Input>();
        
        inputTokens.AddRange(ParseInputTokens(prms, func, holder));

        return new LocalCall(holder, func.Name, inputTokens, CurrentLine, term);
    }

    public List<Input> ParseInputTokens(string prms, IFunction func, ITokenHolder holder)
    {
        List<string> inputs =  ParseCallInputs(prms);

        // Determine tokens
        List<Input> inputTokens = new List<Input>();
        bool hasParams = false;
        
        for (var i = 0; i < inputs.Count; i++)
        {
            var input = inputs[i];
            if (i >= func.InputTypes.Length)
                throw new InvalidParametersException(CurrentLine, inputs.ToArray());
            
            if (func.InputTypes[i].StartsWith("params"))
            {
                hasParams = true;
                break;
            }
            
            inputTokens.Add(CompileUtils.HandleToken(input, func.InputTypes[i], holder, this));
        }

        if (hasParams)
        {
            string type = func.InputTypes[func.InputTypes.Length - 1].Split(' ')[1].Trim();

            Input[] grouping = new Input[inputs.Count - inputTokens.Count];
            
            int j = 0;
            for (int i = inputTokens.Count; i < inputs.Count; i++, j++)
            {
                string input = inputs[i];
                grouping.SetValue(CompileUtils.HandleToken(input, type, holder, this), j);
            }
            
            inputTokens.Add(new Input(holder, new ParamsCall(holder, CurrentLine, type, grouping)));
        }

        return inputTokens;
    }
    
    #endregion

    #region Inputs

    public List<string> ParseCallInputs(string prms)
    {
        List<string> inputs = new List<string>();

        string current = "";
        bool isStr = false;
        int methodLevel = 0;
        if (prms.Length > 1)
        {
            for (int i = 1; i < prms.Length; i++)
            {
                char p = prms[i - 1];
                char c = prms[i];

                if (p == '"')
                {
                    isStr = !isStr;
                }

                if (c == '(' && !isStr) // TODO: 
                {
                    methodLevel++;
                }

                if (c == ')' && !isStr)
                {
                    methodLevel--;
                }

                current += p;
                
                if ((isStr || methodLevel != 0) && i + 1 < prms.Length)
                    continue;

                if (c == ',' || i + 1 >= prms.Length)
                {
                    current += c;

                    inputs.Add(current.Trim(' ', '\t', ','));
                    current = "";
                }
            }
        }
        else if (prms.Length != 0) // Lazy way to account for single character params lol
        {
            current = prms;
            inputs.Add(current.Trim(' ', '\t'));
        }

        return inputs;
    }

    #endregion

    #region Text reading

    public int CurrentLine;
    private TextReader _reader;

    public string ReadCleanLine()
    {
        CurrentLine++;
        return CleanLine(_reader.ReadLine());
    }

    private string CleanLine(string line)
    {
        if (line == null) return null;
        
        line = line.Trim();

        int commentPosition = line.IndexOf("//", StringComparison.Ordinal); //Remove comments
        if (commentPosition != -1)
        {
            // Remove comment
            // We need to account for strings and such potentially containing // as well
            bool inStr = false;
            bool foundComment = false;

            for (var i = 1; i < line.Length; i++)
            {
                if (foundComment)
                    break;
                        
                char c = line[i];
                char previous = line[i - 1];
                        
                switch (c)
                {
                    case '/':
                    {
                        if (!inStr && previous == '/')
                        {
                            commentPosition = i - 1;
                            foundComment = true;
                        }
                    } break;
                    case '"':
                    {
                        // Don't end the string if its escaped
                        if (inStr && previous != '\\')
                        {
                            inStr = false;
                        }
                        else
                        {
                            inStr = true;
                        }
                    } break;
                }
            }

            if (foundComment)
            {
                line = line.Remove(commentPosition).Trim();
            }
        }

        return line;
    }

    #endregion

    #region Libraries

    public bool HasLibrary(string name) => _libraries.Any(l => l.Name == name);

    public ILibrary GetLibrary(string name)
    {
        return _libraries.First(l => name == l.Name);
    }

    #endregion

    public SteelCompiler(params ILibrary[] libraries)
    {
        // needs to be longer by one to account for the system library(always imported)
        _libraries = new ILibrary[libraries.Length + 1];
        _libraries.SetValue(new SteelLibrary(), 0);
        for (int i = 0; i < libraries.Length; i++)
        {
            _libraries.SetValue(libraries[i], i + 1);
        }
    }
}