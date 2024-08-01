using SteelLanguage.Token.Interaction;
using SteelLanguage.Token.Terms;
using SteelLanguage.Utils;

namespace SteelLanguage.Token.Functions.Operator;

public class BoolOpCall : TokenCall
{
    private readonly Input _a;
    private readonly Input _b;
    private readonly BoolOperatorKind _kind;

    public BoolOpCall(ITokenHolder container, int line, Input a, Input b, BoolOperatorKind kind) : base(container, line)
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