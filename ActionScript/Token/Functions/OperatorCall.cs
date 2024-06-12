﻿using ActionScript.Token.Interaction;
using ActionScript.Token.Terms;
using ActionScript.Utils;

namespace ActionScript.Token.Functions;

public class OperatorCall : TokenCall
{
    private Input _a;
    private Input _b;
    private readonly OperatorKind _kind;

    public OperatorCall(ITokenHolder script, int line, Input a, Input b, OperatorKind kind) : base(script, line)
    {
        _a = a;
        _b = b;
        _kind = kind;
    }

    public override ReturnValue Call()
    {
        BaseTerm a = _a.GetValue();
        BaseTerm b = _b.GetValue();
        return new ReturnValue(a.ConductOperation(_kind, b), a.ValueType);
    }
}