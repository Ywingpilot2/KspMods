using ActionLanguage.Token.Functions;

namespace ActionLanguage.Token.KeyWords;

public struct BreakKeyword : IKeyword
{
    public string Name => "break";
    public void CompileKeyword(string token, ActionCompiler compiler, ActionScript script, ITokenHolder tokenHolder)
    {
        tokenHolder.AddCall(new BreakCall(tokenHolder, compiler.CurrentLine));
    }
}