using SteelLanguage.Exceptions;
using SteelLanguage.Extensions;
using SteelLanguage.Token.Functions.Conditional;
using SteelLanguage.Token.Interaction;
using SteelLanguage.Utils;

namespace SteelLanguage.Token.KeyWords;

public struct WhileKeyword : IKeyword
{
    public string Name => "while";

    private SteelCompiler _compiler;
    private SteelScript _script;

    public void CompileKeyword(string token, SteelCompiler compiler, SteelScript script, ITokenHolder holder)
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