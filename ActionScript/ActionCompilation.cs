using System;
using System.Collections.Generic;
using System.IO;
using ActionScript.Exceptions;
using ActionScript.Extensions;
using ActionScript.Library;
using ActionScript.Token;
using ActionScript.Token.Functions;
using ActionScript.Token.Interaction;
using ActionScript.Token.KeyWords;
using ActionScript.Token.Terms;
using ActionScript.Utils;

namespace ActionScript;

public class ActionCompiler : ITokenHolder
{
    private static readonly ActionLibrary Library = new();

    private string _scriptPath;
    private ILibrary[] _libraries;
    private ActionScript _script;

    public ActionScript CompileScript()
    {
        _reader = new StringReader(_scriptPath);
        _script = new ActionScript();
        ImportLibrary(Library);
        foreach (ILibrary library in _libraries)
        {
            ImportLibrary(library);
        }

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
                
                    ParseToken(line, _script);
                    line = ReadCleanLine();
                }
            }
#if !DEBUG
            catch (ActionException e)
            {
                if (e.LineNumber == 0)
                    e.LineNumber = _currentLine;
                throw;
            }
#else
            finally{}
#endif
        }

        return _script;
    }

    #region Types

    public bool TermTypeExists(string name)
    {
        foreach (TypeLibrary library in _script.TypeLibraries)
        {
            if (library == null)
                continue;
            
            if (library.HasTermType(name))
                return true;
        }
        return false;
    }

    public TermType GetTermType(string name)
    {
        foreach (TypeLibrary library in _script.TypeLibraries)
        {
            if (library == null)
                continue;
            
            if (!library.HasTermType(name))
                continue;

            return library.GetTermType(name, CurrentLine);
        }
            
        throw new TypeNotExistException(CurrentLine, name);
    }

    

    #endregion
    
    #region Tokens

    public void ParseToken(string token, ITokenHolder holder)
    {
        string keywordName = token.Split(new[] { ' ' }, 2, StringSplitOptions.RemoveEmptyEntries)[0].Trim();
        if (HasKeyword(keywordName))
        {
            IKeyword keyword = GetKeyword(keywordName);
            keyword.CompileKeyword(token, this, _script, holder);
        }
        else
        {
            string[] split = token.SanitizedSplit('=', 2, StringSplitOptions.RemoveEmptyEntries);
            if (split.Length == 1)
            {
                FunctionCall call = ParseFunctionCall(token, holder);
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

    public void ParseTerm(string token, ITokenHolder holder)
    {
        AssignmentKind kind = CompileUtils.DetermineAssignment(token, holder);
        string[] values = CompileUtils.GetTermValues(token);
        
        switch (kind)
        {
            case AssignmentKind.Constant:
            {
                TermType type = GetTermType(values[0]);
                BaseTerm term = type.Construct(values[1], CurrentLine);
                if (!term.Parse(values[2]))
                    throw new InvalidAssignmentException(CurrentLine, term);
                
                holder.AddTerm(term);
            } break;
            case AssignmentKind.Term:
            {
                TermType type = GetTermType(values[0]);
                BaseTerm from = holder.GetTerm(values[2]);
                BaseTerm term = type.Construct(values[1], CurrentLine);
                Input input = new Input(from);

                AssignmentCall assignmentCall = new AssignmentCall(term, input, holder, CurrentLine);
                holder.AddTerm(term);
                holder.AddCall(assignmentCall);
            } break;
            case AssignmentKind.Function:
            {
                TermType type = GetTermType(values[0]);
                BaseTerm term = type.Construct(values[1], CurrentLine);
                Input input = CompileUtils.HandleToken(values[2], type.Name, holder, this);
                AssignmentCall call = new AssignmentCall(term, input, holder, CurrentLine);
                holder.AddTerm(term);
                holder.AddCall(call);
            } break;
            case AssignmentKind.Assignment:
            {
                BaseTerm term = holder.GetTerm(values[0]);
                string type = term.ValueType;
                Input input = CompileUtils.HandleToken(values[1], type, holder, this);
                AssignmentCall call = new AssignmentCall(term, input, holder, CurrentLine);
                holder.AddCall(call);
            } break;
        }
    }

    #endregion

    #region Function Calls

    public FunctionCall ParseFunctionCall(string token, ITokenHolder holder)
    {
        // Is this a global or local function?
        if (holder.HasFunction(token.Split('(')[0]))
        {
            return ParseGlobalCall(token, holder);
        }
        else
        {
            string[] dets = token.SanitizedSplit('.', 2, StringSplitOptions.RemoveEmptyEntries);
            if (dets.Length > 1 && holder.HasTerm(dets[0].Trim()))
            {
                return ParseLocalCall(token, holder);
            }
            else
            {
                throw new FunctionNotExistException(CurrentLine, token);
            }
        }
    }

    private FunctionCall ParseGlobalCall(string token, ITokenHolder holder)
    {
        string[] split = token.SanitizedSplit('(', 2, StringSplitOptions.RemoveEmptyEntries);
        string name = split[0].Trim();
        string prms = split[1].Trim(' ', '\t');
        prms = prms.Remove(prms.Length - 1);

        IFunction func = holder.GetFunction(name);
        List<Input> inputTokens = ParseInputTokens(prms, func, holder);

        return new FunctionCall(holder, func, inputTokens, CurrentLine);
    }

    private TermCall ParseLocalCall(string token, ITokenHolder holder)
    {
        string[] dets = token.SanitizedSplit('.', 2, StringSplitOptions.RemoveEmptyEntries);
        BaseTerm term = holder.GetTerm(dets[0].Trim());

        string funcToken = dets[1].Trim(' ', '.');
        string[] split = funcToken.SanitizedSplit('(', 2, StringSplitOptions.RemoveEmptyEntries);
        string name = split[0].Trim();
        string prms = split[1].Trim();
        prms = prms.Remove(prms.Length - 1);
        
        IFunction func = term.GetFunction(name);
        List<Input> inputs = ParseInputTokens(prms, func, holder);

        return new TermCall(holder, CurrentLine, func, term.Name, inputs);
    }

    private List<Input> ParseInputTokens(string prms, IFunction func, ITokenHolder holder)
    {
        List<string> inputs =  ParseCallInputs(prms);
        
        if (inputs.Count != func.InputTypes.Length && !func.AnyCount)
            throw new InvalidParametersException(CurrentLine, func.InputTypes);

        // Determine tokens
        List<Input> inputTokens = new List<Input>();
        for (var i = 0; i < inputs.Count; i++)
        {
            var input = inputs[i];
            inputTokens.Add(CompileUtils.HandleToken(input, func.InputTypes[i], holder, this));
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

    #region Reading
    
    public int CurrentLine;
    private TextReader _reader;

    public string ReadCleanLine()
    {
        CurrentLine++;
        return CleanLine(_reader.ReadLine());
    }
    
    public string CleanLine(string line)
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

    public void ImportLibrary(ILibrary library)
    {
        if (library.GlobalFunctions != null)
        {
            foreach (IFunction globalFunction in library.GlobalFunctions)
            {
                if (_script.HasFunction(globalFunction.Name))
                    continue;
                
                _script.Functions.Add(globalFunction.Name, globalFunction);
            }
        }

        if (library.GlobalTerms != null)
        {
            foreach (BaseTerm term in library.GlobalTerms)
            {
                if (_script.Functions.ContainsKey(term.Name))
                    continue;
                
                _script.Terms.Add(term.Name, term);
            }
        }

        if (library.Keywords != null)
        {
            foreach (IKeyword keyword in library.Keywords)
            {
                _script.Keywords.Add(keyword.Name, keyword);
            }
        }
        
        if (library.TypeLibrary != null)
        {
            _script.TypeLibraries.Add(library.TypeLibrary);
        }
    }

    #endregion

    #region Token Holder

    public IFunction GetFunction(string name) => _script.GetFunction(name);

    public bool HasFunction(string name) => _script.HasFunction(name);

    public BaseTerm GetTerm(string name) => _script.GetTerm(name);

    public bool HasTerm(string name) => _script.HasTerm(name);

    public void AddCall(TokenCall call) => _script.AddCall(call);

    public void AddTerm(BaseTerm term) => _script.AddTerm(term);

    public void AddFunc(IFunction function) => _script.AddFunc(function);

    public bool HasKeyword(string name) => _script.HasKeyword(name);

    public IKeyword GetKeyword(string name) => _script.GetKeyword(name);

    #endregion

    public ActionCompiler(string path, params ILibrary[] libraries)
    {
        _scriptPath = path;
        _libraries = libraries;
    }
}