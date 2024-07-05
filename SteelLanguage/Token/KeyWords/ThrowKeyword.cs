using SteelLanguage.Extensions;
using SteelLanguage.Token.Functions.Single;
using SteelLanguage.Token.Interaction;
using SteelLanguage.Utils;

namespace SteelLanguage.Token.KeyWords;

public struct ThrowKeyword : IKeyword
{
    public string Name => "throw";
    public void CompileKeyword(string token, SteelCompiler compiler, SteelScript script, ITokenHolder tokenHolder)
    {
        string[] split = token.SanitizedSplit(' ', 2);
        string value = split[1].Trim();

        Input input = CompileUtils.HandleToken(value, "string", tokenHolder, compiler);
        tokenHolder.AddCall(new ThrowCall(tokenHolder, compiler.CurrentLine, input));
    }
}