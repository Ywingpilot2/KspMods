﻿using ActionLanguage.Token.Interaction;
using ActionLanguage.Token.Terms;
using ActionLanguage.Utils;

namespace ActionLanguage.Token.Functions.Operator;

public class BoolOpCall : TokenCall
{
    private Input _a;
    private Input _b;
    private readonly BoolOperatorKind _kind;

    public BoolOpCall(ITokenHolder script, int line, Input a, Input b, BoolOperatorKind kind) : base(script, line)
    {
        _a = a;
        _b = b;
        _kind = kind;
    }

    public override ReturnValue Call()
    {
        BaseTerm a = _a.GetValue();
        BaseTerm b = _b.GetValue();
        return new ReturnValue(a.ConductBoolOp(_kind, b), a.ValueType);
    }

    public override void PostCompilation()
    {
        _a.PostCompilation();
        _b.PostCompilation();
    }
}