using SteelLanguage.Token.Functions.Single;

namespace SteelLanguage.Token.KeyWords;

public struct BreakKeyword : IKeyword
{
    public string Name => "break";
    public void CompileKeyword(string token, SteelCompiler compiler, SteelScript script, ITokenHolder tokenHolder)
    {
        tokenHolder.AddCall(new BreakCall(tokenHolder, compiler.CurrentLine));
    }
}