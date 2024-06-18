using System;
using ActionLanguage.Exceptions;
using ActionLanguage.Extensions;
using ActionLanguage.Library;
using ActionLanguage.Token.Functions;
using ActionLanguage.Token.Interaction;
using ActionLanguage.Token.Terms;
using ActionLanguage.Utils;

namespace ActionLanguage.Token.KeyWords;

public struct ForeachKeyword : IKeyword
{
    public string Name => "foreach";

    private ActionCompiler _compiler;
    public void CompileKeyword(string token, ActionCompiler compiler, ActionScript script, ITokenHolder tokenHolder)
    {
        _compiler = compiler;

        string prms = token.SanitizedSplit('(', 2, StringSplitOptions.RemoveEmptyEntries)[1];
        prms = prms.Remove(prms.Length - 1);

        string[] split = prms.SplitAt(prms.SanitizedIndexOf(" in "), 2, StringSplitOptions.RemoveEmptyEntries);
        string termToken = split[0].Trim();
        string inputToken = split[1].Remove(0, 2).Trim();
        
        Input input = CompileUtils.HandleToken(inputToken, "enumerable", tokenHolder, compiler);

        string[] typeName = termToken.Split(new []{' '}, 2, StringSplitOptions.RemoveEmptyEntries);
        TermType type = tokenHolder.GetTermType(typeName[0].Trim());
        string name = typeName[1].Trim();
        
        ForeachFunc func = new ForeachFunc(tokenHolder, input, name);

        BaseTerm term = type.Construct(name, compiler.CurrentLine);
        func.AddTerm(term);

        ParseForeachTokens(token, func);
        
        tokenHolder.AddCall(new ForeachCall(tokenHolder, compiler.CurrentLine, func));
    }
    
    private void ParseForeachTokens(string foreachToken, ForeachFunc foreachFunc)
    {
        string line = _compiler.ReadCleanLine();
        while (line != "{")
        {
            line = _compiler.ReadCleanLine();
            if (line == null)
                throw new FunctionLacksEndException(_compiler.CurrentLine, foreachToken);
        }

        line = _compiler.ReadCleanLine();
        while (line != "}")
        {
            if (line == null)
                throw new FunctionLacksEndException(_compiler.CurrentLine, foreachToken);

            if (line == "")
            {
                line = _compiler.ReadCleanLine();
                continue;
            }

            _compiler.ParseToken(line, foreachFunc);
            
            line = _compiler.ReadCleanLine();
        }
    }
}