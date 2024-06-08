using System;
using System.Collections.Generic;
using System.IO;
using ActionScript.Exceptions;
using ActionScript.Functions;
using ActionScript.Library;
using ActionScript.Terms;

namespace ActionScript;

public class ActionCompiler
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
                
                    ParseToken(line);
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

            return library.GetTermType(name, _currentLine);
        }
            
        throw new TypeNotExistException(_currentLine, name);
    }

    #endregion
    
    #region Tokens

    public void ParseToken(string token)
    {
        if (token.StartsWith("func"))
        {
        }
        else
        {
            string[] split = token.Split(new[] { '=' }, 2, StringSplitOptions.RemoveEmptyEntries);
            if (split.Length == 1)
            {
                FunctionCall call = ParseFunctionCall(token);
                _script.TokenCalls.Add(call);
            }
            else if (split.Length == 2 && TermTypeExists(split[0].Split(' ')[0].Trim()))
            {
                BaseTerm term = ParseTermConstant(token);
                _script.Terms.Add(term.Name, term);
                if (term.Kind == TermKind.Null)
                {
                    string value = split[1].Trim();
                    if (token.EndsWith(")"))
                    {
                        FunctionCall functionCall = ParseFunctionCall(value);
                        _script.TokenCalls.Add(new AssignmentCall(term, new Input(_script, functionCall), _script, _currentLine));
                    }
                    else if (_script.Terms.ContainsKey(value))
                    {
                        BaseTerm assigningTerm = _script.Terms[value];
                        _script.TokenCalls.Add(new AssignmentCall(term, new Input(assigningTerm), _script, _currentLine));
                    }
                }
            }
            else if (split.Length == 2 && _script.Terms.ContainsKey(split[0].Split(' ')[0].Trim()))
            {
                BaseTerm original = _script.Terms[split[0].Split(' ')[0].Trim()];
                TermType type = original.GetTermType();
                string value = split[1].Trim();

                if (token.EndsWith(")"))
                {
                    FunctionCall functionCall = ParseFunctionCall(value);
                    _script.TokenCalls.Add(new AssignmentCall(original, new Input(_script, functionCall), _script, _currentLine));
                }
                else
                {
                    BaseTerm newValue = type.Construct(Guid.NewGuid().ToString(), _currentLine);
                    if (!newValue.Parse(value))
                        throw new InvalidAssignmentException(_currentLine, original);
                
                    _script.TokenCalls.Add(new AssignmentCall(original, new Input(newValue), _script, _currentLine));
                }
            }
            else // TODO: More in depth error logging for this. Should tell them if it was an issue with a function call or type not being found
            {
                throw new InvalidCompilationException(_currentLine, $"Could not determine token at line {_currentLine}");
            }
        }
    }

    #endregion

    #region Terms

    public BaseTerm ParseTermConstant(string token)
    {
        string[] split = token.Split(new[] { ' ' }, 2);
        string typeName = split[0].Trim();
        if (!TermTypeExists(typeName))
            throw new TypeNotExistException(_currentLine, typeName);
        
        TermType termType = GetTermType(typeName);
        string[] termDat = split[1].Trim().Split(new []{'='}, 2);
        BaseTerm term = termType.Construct(termDat[0].Trim(), _currentLine);

        if (termDat.Length == 1 || split[1].EndsWith(")"))
        {
            return term;
        }

        if (!term.Parse(termDat[1].Trim()))
        {
            if (_script.Terms.ContainsKey(termDat[1].Trim()))
            {
                return term;
            }
            else
            {
                throw new InvalidCompilationException(_currentLine,
                    $"Inputted value for {term.Name} was not valid given its type or could not be parsed");
            }
        }

        return term;
    }

    #endregion

    #region Functions

    public FunctionCall ParseFunctionCall(string token)
    {
        // Is this a global or local function?
        if (_script.HasFunction(token.Split('(')[0]))
        {
            return ParseGlobalCall(token);
        }
        else
        {
            string[] dets = token.Split(new[] { '.' }, 2, StringSplitOptions.RemoveEmptyEntries);
            if (dets.Length > 1 && _script.Terms.ContainsKey(dets[0].Trim()))
            {
                return ParseLocalCall(token);
            }
            else
            {
                throw new FunctionNotExistException(_currentLine, token);
            }
        }
    }

    public FunctionCall ParseGlobalCall(string token)
    {
        string[] split = token.Split(new []{'('}, 2, StringSplitOptions.RemoveEmptyEntries);
        string name = split[0].Trim();
        string prms = split[1].Trim(' ', '\t');
        prms = prms.Remove(prms.Length - 1);

        Function func = _script.GetFunction(name);
        List<Input> inputTokens = ParseInputTokens(prms, func);

        return new FunctionCall(_script, func, inputTokens, _currentLine);
    }

    public TermCall ParseLocalCall(string token)
    {
        string[] dets = token.Split(new[] { '.' }, 2, StringSplitOptions.RemoveEmptyEntries);
        BaseTerm term = _script.Terms[dets[0].Trim()];

        string funcToken = dets[1].Trim(' ', '.');
        string[] split = funcToken.Split(new []{'('}, 2, StringSplitOptions.RemoveEmptyEntries);
        string name = split[0].Trim();
        string prms = split[1].Trim();
        prms = prms.Remove(prms.Length - 1);
        
        Function func = term.GetFunction(name);
        List<Input> inputs = ParseInputTokens(prms, func);

        return new TermCall(_script, _currentLine, func, term.Name, inputs);
    }

    public List<Input> ParseInputTokens(string prms, Function func)
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
        
        if (inputs.Count != func.InputTypes.Length)
            throw new InvalidParametersException(_currentLine, func.InputTypes);

        // Determine tokens
        List<Input> inputTokens = new List<Input>();
        for (var i = 0; i < inputs.Count; i++)
        {
            var input = inputs[i];
            if (input.EndsWith(")"))
            {
                FunctionCall call = ParseFunctionCall(input);

                if (call.Function.ValueType != func.InputTypes[i])
                {
                    TermType type = GetTermType(call.Function.ValueType);
                    if (!type.IsSubclassOf(func.InputTypes[i]))
                        throw new InvalidParametersException(_currentLine, call.Function.InputTypes);
                }

                inputTokens.Add(new Input(_script, call));
            }
            else if (!_script.Terms.ContainsKey(input))
            {
                string typeName = func.InputTypes[i];
                TermType type = GetTermType(typeName);
                BaseTerm term = type.Construct(Guid.NewGuid().ToString(), _currentLine);
                if (!term.Parse(input))
                    throw new InvalidCompilationException(_currentLine,
                        "Unable to parse specified value, either this constant is of the incorrect type or the value cannot be parsed");
                
                inputTokens.Add(new Input(term));
            }
            else
            {
                BaseTerm term = _script.Terms[input];
                if (term.ValueType != func.InputTypes[i])
                {
                    if (!term.GetTermType().IsSubclassOf(func.InputTypes[i]))
                        throw new InvalidParametersException(_currentLine, func.InputTypes);
                }
                inputTokens.Add(new Input(term));
            }
        }

        return inputTokens;
    }

    #endregion

    #region Reading
    
    private int _currentLine;
    private TextReader _reader;

    private string ReadCleanLine()
    {
        _currentLine++;
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

    public void ImportLibrary(ILibrary library)
    {
        if (library.GlobalFunctions != null)
        {
            foreach (Function globalFunction in library.GlobalFunctions)
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

        if (library.TypeLibrary != null)
        {
            _script.TypeLibraries.Add(library.TypeLibrary);
        }
    }

    #endregion

    public ActionCompiler(string path, params ILibrary[] libraries)
    {
        _scriptPath = path;
        _libraries = libraries;
    }
}