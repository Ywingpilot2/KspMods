using ActionScript.Extensions;
using ActionScript.Token.Functions;
using ActionScript.Token.Interaction;
using ActionScript.Utils;

namespace ActionScript.Token.KeyWords;

public struct ThrowKeyword : IKeyword
{
    public string Name => "throw";
    public void CompileKeyword(string token, ActionCompiler compiler, ActionScript script, ITokenHolder tokenHolder)
    {
        string[] split = token.SanitizedSplit(' ', 2);
        string value = split[1].Trim();

        Input input = CompileUtils.HandleToken(value, "string", tokenHolder, compiler);
        tokenHolder.AddCall(new ThrowCall(tokenHolder, compiler.CurrentLine, input));
    }
}