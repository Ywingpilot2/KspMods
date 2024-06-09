namespace ActionScript.Token.KeyWords;

public interface IKeyword
{
    public string Name { get; }

    public void CompileKeyword(string token, ActionCompiler compiler, ActionScript script, ITokenHolder tokenHolder);
}