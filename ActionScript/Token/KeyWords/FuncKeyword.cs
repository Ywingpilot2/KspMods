using System.Collections.Generic;
using ActionLanguage.Exceptions;
using ActionLanguage.Extensions;
using ActionLanguage.Library;
using ActionLanguage.Token.Functions;
using ActionLanguage.Token.Terms;

namespace ActionLanguage.Token.KeyWords;

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
        // first add global terms from the compiler
        foreach (ILibrary library in _compiler.EnumerateLibraries())
        {
            if (library.GlobalTerms == null)
                continue;

            foreach (BaseTerm globalTerm in library.GlobalTerms)
            {
                function.AddTerm(globalTerm);
            }
        }
        
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
                
            }
            _compiler.ParseToken(line, function);
            
            line = _compiler.ReadCleanLine();
        }
    }
}