﻿using ActionLanguage.Token.Interaction;
using ActionLanguage.Token.Terms;

namespace ActionLanguage.Token.Functions.Single;

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

public class ReturnCall : TokenCall
{
    private Input _returnValue;
    
    public ReturnCall(ITokenHolder script, int line, Input input) : base(script, line)
    {
        _returnValue = input;
    }
    
    public ReturnCall(ITokenHolder script, int line) : base(script, line)
    {
    }

    public override ReturnValue Call()
    {
        if (_returnValue.Type == InputType.Null)
            return new ReturnValue();
        
        BaseTerm term = _returnValue.GetValue();
        return new ReturnValue(term.GetValue(), term.ValueType);
    }

    public override void PostCompilation()
    {
        _returnValue.PostCompilation();
    }
}