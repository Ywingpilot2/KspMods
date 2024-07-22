using SteelLanguage.Token.Functions.Single;
using SteelLanguage.Token.KeyWords.Container;

namespace SteelLanguage.Token.KeyWords.Single;

internal record BreakKeyword : IKeyword
{
    public string Name => "break";
    public void CompileKeyword(string token, SteelCompiler compiler, SteelScript script, ITokenHolder tokenHolder)
    {
        tokenHolder.AddCall(new BreakCall(tokenHolder, compiler.CurrentLine));
    }
}