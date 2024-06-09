using System;
using System.Collections.Generic;
using ActionScript.Exceptions;
using ActionScript.Extensions;
using ActionScript.Library;
using ActionScript.Token.Functions;
using ActionScript.Token.Interaction;
using ActionScript.Token.Terms;

namespace ActionScript.Token.KeyWords;

public struct FuncKeyword : IKeyword
{
    public string Name => "func";

    private ActionCompiler _compiler;
    private ActionScript _script;
    public void CompileKeyword(string token, ActionCompiler compiler, ActionScript script, ITokenHolder holder)
    {
        _compiler = compiler;
        _script = script;
        UserFunction function = ParseFunction(token);
        _script.AddFunc(function);
    }
    
    public UserFunction ParseFunction(string token)
    {
        string[] split = token.SanitizedSplit(' ', 3);
        string returnType = split[1].Trim();

        string[] value = split[2].Trim().SanitizedSplit('(', 2);
        string name = value[0];
        string prms = value[1].Trim(')', ' ');

        List<string> inputTokens = _compiler.ParseCallInputs(prms);

        Dictionary<string, string> inputMapping = new Dictionary<string, string>();
        foreach (string inputToken in inputTokens)
        {
            string[] inTs = inputToken.SanitizedSplit(' ', 2);
            if (inTs.Length != 2)
                throw new FunctionParamsInvalidException(_compiler.CurrentLine, token);
            
            inputMapping.Add(inTs[1], inTs[0]);
        }

        UserFunction function = new UserFunction(name, returnType, inputMapping, _script);
        ParseFunctionTokens(token, function);

        return function;
    }

    private void ParseFunctionTokens(string funcToken, UserFunction function)
    {
        string line = _compiler.ReadCleanLine();
        while (line != "{")
        {
            line = _compiler.ReadCleanLine();
            if (line == null)
                throw new FunctionLacksEndException(_compiler.CurrentLine, funcToken);
        }

        line = _compiler.ReadCleanLine();
        bool hasReturned = false;
        while (line != "}")
        {
            if (line == null)
                throw new FunctionLacksEndException(_compiler.CurrentLine, funcToken);

            if (line == "" || hasReturned)
            {
                line = _compiler.ReadCleanLine();
                continue;
            }

            if (line.StartsWith("return"))
            {
                hasReturned = true;
                if (_compiler.TermTypeExists(function.ReturnType))
                {
                    if (function.ReturnType == "void")
                        continue;
                    
                    function.SetReturnValue(ParseReturn(line, function));
                }
                else
                {
                    throw new TypeNotExistException(_compiler.CurrentLine, function.ReturnType);
                }
            }
            else
            {
                _compiler.ParseToken(line, function);
            }
            
            line = _compiler.ReadCleanLine();
        }
    }

    private Input ParseReturn(string token, UserFunction function)
    {
        string[] split = token.SanitizedSplit(' ', 2, StringSplitOptions.RemoveEmptyEntries);
        string value = split[1].Trim();
        
        if (token.EndsWith(")"))
        {
            FunctionCall functionCall = _compiler.ParseFunctionCall(value, function);
            return new Input(function, functionCall);
        }
        else if (function.HasTerm(value))
        {
            return new Input(function.GetTerm(value));
        }
        else // hopefully a constant, try parsing it
        {
            TermType type = _compiler.GetTermType(function.ReturnType);
            BaseTerm term = type.Construct(Guid.NewGuid().ToString(), _compiler.CurrentLine);
            if (!term.Parse(value))
                throw new InvalidCompilationException(_compiler.CurrentLine, $"Return at line {_compiler.CurrentLine} returns an invalid value");

            return new Input(term);
        }
    }
}