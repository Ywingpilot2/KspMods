using ActionLanguage.Token.Interaction;
using ActionLanguage.Token.Terms;
using ActionLanguage.Utils;

namespace ActionLanguage.Token.Functions.Operator;

public class MathCall : TokenCall
{
    private Input _a;
    private Input _b;
    private readonly MathOperatorKind _kind;

    public MathCall(ITokenHolder script, int line, Input a, Input b, MathOperatorKind kind) : base(script, line)
    {
        _a = a;
        _b = b;
        _kind = kind;
    }

    public override ReturnValue Call()
    {
        BaseTerm a = _a.GetValue();
        BaseTerm b = _b.GetValue();
        return new ReturnValue(a.ConductMath(_kind, b), a.ValueType);
    }

    public override void PostCompilation()
    {
        _a.PostCompilation();
        _b.PostCompilation();
    }
}