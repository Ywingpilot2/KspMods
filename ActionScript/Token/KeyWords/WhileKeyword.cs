using ActionLanguage.Exceptions;
using ActionLanguage.Extensions;
using ActionLanguage.Token.Functions;
using ActionLanguage.Token.Functions.Conditional;
using ActionLanguage.Token.Interaction;
using ActionLanguage.Utils;

namespace ActionLanguage.Token.KeyWords;

public struct WhileKeyword : IKeyword
{
    public string Name => "while";

    private ActionCompiler _compiler;
    private ActionScript _script;

    public void CompileKeyword(string token, ActionCompiler compiler, ActionScript script, ITokenHolder holder)
    {
        _compiler = compiler;
        _script = script;

        string[] split = token.SanitizedSplit('(', 2);
        string prms = split[1].Trim();
        prms = prms.Remove(prms.Length - 1);

        Input condition = CompileUtils.HandleToken(prms, "bool", holder, compiler);

        WhileFunction whileFunction = new WhileFunction(condition, holder);
        ParseWhileTokens(token, whileFunction);
        
        holder.AddCall(new WhileCall(holder, compiler.CurrentLine, whileFunction));
    }

    private void ParseWhileTokens(string whileToken, WhileFunction whileFunction)
    {
        string line = _compiler.ReadCleanLine();
        while (line != "{")
        {
            line = _compiler.ReadCleanLine();
            if (line == null)
                throw new FunctionLacksEndException(_compiler.CurrentLine, whileToken);
        }

        line = _compiler.ReadCleanLine();
        while (line != "}")
        {
            if (line == null)
                throw new FunctionLacksEndException(_compiler.CurrentLine, whileToken);

            if (line == "")
            {
                line = _compiler.ReadCleanLine();
                continue;
            }

            _compiler.ParseToken(line, whileFunction);
            
            line = _compiler.ReadCleanLine();
        }
    }
}