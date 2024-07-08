﻿using System;
using System.Collections.Generic;
using SteelLanguage.Exceptions;
using SteelLanguage.Extensions;
using SteelLanguage.Token.Functions;

namespace SteelLanguage.Token.KeyWords;

public struct FuncKeyword : IKeyword
{
    public string Name => "func";

    private SteelCompiler _compiler;
    private SteelScript _script;
    public void CompileKeyword(string token, SteelCompiler compiler, SteelScript script, ITokenHolder holder)
    {
        _compiler = compiler;
        _script = script;
        UserFunction function = ParseFunction(token);
        _script.AddFunc(function);
        ParseFunctionTokens(token, function);
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
        for (var i = 0; i < inputTokens.Count; i++)
        {
            var inputToken = inputTokens[i];
            string[] inTs = inputToken.SanitizedSplit(' ', 2, StringSplitOptions.RemoveEmptyEntries,
                ScanDirection.RightToLeft);
            
            if (inTs.Length != 2)
                throw new FunctionParamsInvalidException(_compiler.CurrentLine, token);

            if (inTs[0].StartsWith("params") && i != inputTokens.Count - 1)
                throw new FunctionParamsInvalidException(_compiler.CurrentLine, token, "\"params\" can only be used as the last parameter");
            
            inputMapping.Add(inTs[1], inTs[0]);
        }

        UserFunction function = new UserFunction(_script, name, returnType, inputMapping);

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
                
            }
            _compiler.ParseToken(line, function);
            
            line = _compiler.ReadCleanLine();
        }
    }
}