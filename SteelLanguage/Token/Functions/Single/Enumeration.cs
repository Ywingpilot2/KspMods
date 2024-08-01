using SteelLanguage.Token.Interaction;
using SteelLanguage.Token.Terms;

namespace SteelLanguage.Token.Functions.Single;

/// <summary>
/// Now this is just wasteful...
/// TODO: find a better way to break/continue loops
/// </summary>
public class BreakCall : TokenCall
{
    public BreakCall(ITokenHolder container, int line) : base(container, line)
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
    public ContinueCall(ITokenHolder container, int line) : base(container, line)
    {
    }

    public override ReturnValue Call()
    {
        return new ReturnValue(this, "keyword");
    }
}

public class ReturnCall : TokenCall
{
    private readonly Input _returnValue;
    
    public ReturnCall(ITokenHolder container, int line, Input input) : base(container, line)
    {
        _returnValue = input;
    }
    
    public ReturnCall(ITokenHolder container, int line) : base(container, line)
    {
    }

    public override ReturnValue Call()
    {
        if (_returnValue.Type == InputType.Null)
            return ReturnValue.None;
        
        BaseTerm term = _returnValue.GetValue();
        return new ReturnValue(term.GetValue(), term.ValueType);
    }

    public override void PostCompilation()
    {
        _returnValue.PostCompilation();
    }
}