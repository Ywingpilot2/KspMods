using SteelLanguage.Token.Functions.Single;
using SteelLanguage.Token.KeyWords.Container;

namespace SteelLanguage.Token.KeyWords.Single;

internal record ContinueKeyword : IKeyword
{
    public string Name => "continue";
    public void CompileKeyword(string token, SteelCompiler compiler, SteelScript script, ITokenHolder tokenHolder)
    {
        tokenHolder.AddCall(new ContinueCall(tokenHolder, compiler.CurrentLine));
    }
}