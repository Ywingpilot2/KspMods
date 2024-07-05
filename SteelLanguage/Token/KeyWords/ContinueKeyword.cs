using SteelLanguage.Token.Functions.Single;

namespace SteelLanguage.Token.KeyWords;

public struct ContinueKeyword : IKeyword
{
    public string Name => "continue";
    public void CompileKeyword(string token, SteelCompiler compiler, SteelScript script, ITokenHolder tokenHolder)
    {
        tokenHolder.AddCall(new ContinueCall(tokenHolder, compiler.CurrentLine));
    }
}