using ActionScript.Token.Interaction;

namespace ActionScript.Token.Functions;

/// <summary>
/// Now this is just wasteful...
/// TODO: find a better way to break/continue loops
/// </summary>
public class BreakCall : TokenCall
{
    public BreakCall(ITokenHolder script, int line) : base(script, line)
    {
    }

    public override ReturnValue Call()
    {
        return new ReturnValue(this, "keyword");
    }
}

/// <summary>
/// Now this is just wasteful...
/// TODO: find a better way to break/continue loops
/// </summary>
public class ContinueCall : TokenCall
{
    public ContinueCall(ITokenHolder script, int line) : base(script, line)
    {
    }

    public override ReturnValue Call()
    {
        return new ReturnValue(this, "keyword");
    }
}