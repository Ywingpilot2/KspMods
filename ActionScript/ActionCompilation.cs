using System;
using System.Collections.Generic;
using System.IO;
using ActionScript.Exceptions;
using ActionScript.Extensions;
using ActionScript.Functions;
using ActionScript.Library;
using ActionScript.Terms;
using ActionScript.Token;

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

            return library.GetTermType(name, _currentLine);
        }
            
        throw new TypeNotExistException(_currentLine, name);
    }

    #endregion
    
    #region Tokens

    public void ParseToken(string token, ITokenHolder holder)
    {
        if (token.StartsWith("func"))
        {
            UserFunction function = ParseFunction(token);
            _script.AddFunc(function);
        }
        else
        {
            string[] split = token.SmartSplit('=', 2, StringSplitOptions.RemoveEmptyEntries);
            if (split.Length == 1)
            {
                FunctionCall call = ParseFunctionCall(token, holder);
                holder.AddCall(call);
            }
            else if (split.Length == 2)
            {
                BaseTerm term = ParseTerm(token, holder, out TokenCall call);
                if (term != null)
                {
                    if (holder.HasTerm(term.Name))
                        throw new TermAlreadyExistsException(_currentLine, term.Name);
                    holder.AddTerm(term);
                }
                if (call != null)
                {
                    holder.AddCall(call);
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

    public BaseTerm ParseTerm(string token, ITokenHolder holder, out TokenCall assignmentCall)
    {
        assignmentCall = null;
        string[] split = token.SmartSplit('=', 2, StringSplitOptions.RemoveEmptyEntries);
        if (TermTypeExists(split[0].Split(' ')[0].Trim()))
        {
            BaseTerm term = ParseTermConstant(token, holder);
            if (term.Kind == TermKind.Null)
            {
                string value = split[1].Trim();
                if (token.EndsWith(")")) // T a = call()
                {
                    FunctionCall functionCall = ParseFunctionCall(value, holder);
                    if (functionCall.Function.ReturnType == "void")
                        throw new FunctionReturnsVoidException(_currentLine, functionCall.Function.Name);
                    assignmentCall = new AssignmentCall(term, new Input(holder, functionCall), holder, _currentLine);
                }
                else if (holder.HasTerm(value)) // T a = b
                {
                    BaseTerm assigningTerm = holder.GetTerm(value);
                    assignmentCall = new AssignmentCall(term, new Input(assigningTerm), holder, _currentLine);
                }
            }

            return term;
        }
        else if (holder.HasTerm(split[0].Split(' ')[0].Trim()))
        {
            BaseTerm original = holder.GetTerm(split[0].Split(' ')[0].Trim());
            TermType type = original.GetTermType();
            string value = split[1].Trim();

            if (token.EndsWith(")"))
            {
                FunctionCall functionCall = ParseFunctionCall(value, holder);
                if (functionCall.Function.ReturnType == "void")
                    throw new FunctionReturnsVoidException(_currentLine, functionCall.Function.Name);
                assignmentCall = new AssignmentCall(original, new Input(holder, functionCall), holder, _currentLine);
            }
            else
            {
                BaseTerm newValue = type.Construct(Guid.NewGuid().ToString(), _currentLine);
                if (!newValue.Parse(value))
                    throw new InvalidAssignmentException(_currentLine, original);

                assignmentCall = new AssignmentCall(original, new Input(newValue), holder, _currentLine);
            }

            return null;
        }

        throw new InvalidAssignmentException(_currentLine);
    }

    public BaseTerm ParseTermConstant(string token, ITokenHolder holder)
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
            if (holder.HasTerm(termDat[1].Trim()))
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

    #region Function Declaration

    private UserFunction ParseFunction(string token)
    {
        string[] split = token.SmartSplit(' ', 3, StringSplitOptions.RemoveEmptyEntries);
        string returnType = split[1].Trim();

        string[] value = split[2].Trim().Split('(');
        string name = value[0];
        string prms = value[1].Trim(')', ' ');

        List<string> inputTokens = ParseInputs(prms);

        Dictionary<string, string> inputMapping = new Dictionary<string, string>();
        foreach (string inputToken in inputTokens)
        {
            string[] inTs = inputToken.SmartSplit(' ', 2, StringSplitOptions.RemoveEmptyEntries);
            if (inTs.Length != 2)
                throw new FunctionParamsInvalidException(_currentLine, token);
            
            inputMapping.Add(inTs[1], inTs[0]);
        }

        UserFunction function = new UserFunction(name, returnType, inputMapping, _script);
        ParseFunctionTokens(token, function);

        return function;
    }

    private void ParseFunctionTokens(string funcToken, UserFunction function)
    {
        string line = ReadCleanLine();
        while (line != "{")
        {
            line = ReadCleanLine();
            if (line == null)
                throw new FunctionLacksEndException(_currentLine, funcToken);
        }

        line = ReadCleanLine();
        bool hasReturned = false;
        while (line != "}")
        {
            if (line == null)
                throw new FunctionLacksEndException(_currentLine, funcToken);

            if (line == "" || hasReturned)
            {
                line = ReadCleanLine();
                continue;
            }

            if (line.StartsWith("return"))
            {
                hasReturned = true;
                if (TermTypeExists(function.ReturnType))
                {
                    if (function.ReturnType == "void")
                        continue;
                    
                    function.SetReturnValue(ParseReturn(line, function));
                }
                else
                {
                    throw new TypeNotExistException(_currentLine, function.ReturnType);
                }
            }
            else
            {
                ParseToken(line, function);
            }
            
            line = ReadCleanLine();
        }
    }

    private Input ParseReturn(string token, UserFunction function)
    {
        string[] split = token.SmartSplit(' ', 2, StringSplitOptions.RemoveEmptyEntries);
        string value = split[1].Trim();
        
        if (token.EndsWith(")"))
        {
            FunctionCall functionCall = ParseFunctionCall(value, function);
            return new Input(function, functionCall);
        }
        else if (function.HasTerm(value))
        {
            return new Input(function.GetTerm(value));
        }
        else // hopefully a constant, try parsing it
        {
            TermType type = GetTermType(function.ReturnType);
            BaseTerm term = type.Construct(Guid.NewGuid().ToString(), _currentLine);
            if (!term.Parse(value))
                throw new InvalidCompilationException(_currentLine, $"Return at line {_currentLine} returns an invalid value");

            return new Input(term);
        }
    }

    #endregion

    #region Function Calls

    private FunctionCall ParseFunctionCall(string token, ITokenHolder holder)
    {
        // Is this a global or local function?
        if (holder.HasFunction(token.Split('(')[0]))
        {
            return ParseGlobalCall(token, holder);
        }
        else
        {
            string[] dets = token.SmartSplit('.', 2, StringSplitOptions.RemoveEmptyEntries);
            if (dets.Length > 1 && holder.HasTerm(dets[0].Trim()))
            {
                return ParseLocalCall(token, holder);
            }
            else
            {
                throw new FunctionNotExistException(_currentLine, token);
            }
        }
    }

    private FunctionCall ParseGlobalCall(string token, ITokenHolder holder)
    {
        string[] split = token.SmartSplit('(', 2, StringSplitOptions.RemoveEmptyEntries);
        string name = split[0].Trim();
        string prms = split[1].Trim(' ', '\t');
        prms = prms.Remove(prms.Length - 1);

        IFunction func = holder.GetFunction(name);
        List<Input> inputTokens = ParseInputTokens(prms, func, holder);

        return new FunctionCall(holder, func, inputTokens, _currentLine);
    }

    private TermCall ParseLocalCall(string token, ITokenHolder holder)
    {
        string[] dets = token.SmartSplit('.', 2, StringSplitOptions.RemoveEmptyEntries);
        BaseTerm term = holder.GetTerm(dets[0].Trim());

        string funcToken = dets[1].Trim(' ', '.');
        string[] split = funcToken.SmartSplit('(', 2, StringSplitOptions.RemoveEmptyEntries);
        string name = split[0].Trim();
        string prms = split[1].Trim();
        prms = prms.Remove(prms.Length - 1);
        
        IFunction func = term.GetFunction(name);
        List<Input> inputs = ParseInputTokens(prms, func, holder);

        return new TermCall(holder, _currentLine, func, term.Name, inputs);
    }

    private List<Input> ParseInputTokens(string prms, IFunction func, ITokenHolder holder)
    {
        List<string> inputs =  ParseInputs(prms);
        
        if (inputs.Count != func.InputTypes.Length)
            throw new InvalidParametersException(_currentLine, func.InputTypes);

        // Determine tokens
        List<Input> inputTokens = new List<Input>();
        for (var i = 0; i < inputs.Count; i++)
        {
            var input = inputs[i];
            if (input.EndsWith(")"))
            {
                FunctionCall call = ParseFunctionCall(input, holder);

                if (call.Function.ReturnType != func.InputTypes[i])
                {
                    TermType type = holder.GetTermType(call.Function.ReturnType);
                    if (!type.IsSubclassOf(func.InputTypes[i]))
                        throw new InvalidParametersException(_currentLine, call.Function.InputTypes);
                }

                inputTokens.Add(new Input(holder, call));
            }
            else if (!holder.HasTerm(input))
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
                BaseTerm term = holder.GetTerm(input);
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

    private List<string> ParseInputs(string prms)
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