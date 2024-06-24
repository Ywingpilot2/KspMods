using ActionLanguage.Token.Interaction;
using ActionLanguage.Token.Terms;
using ActionLanguage.Utils;

namespace ActionLanguage.Token.Functions.Operator;

public class ComparisonCall : TokenCall
{
    private Input _a;
    private Input _b;
    private readonly ComparisonOperatorKind _kind;

    public ComparisonCall(ITokenHolder script, int line, Input a, Input b, ComparisonOperatorKind kind) : base(script, line)
    {
        _a = a;
        _b = b;
        _kind = kind;
    }

    public override ReturnValue Call()
    {
        BaseTerm a = _a.GetValue();
        BaseTerm b = _b.GetValue();
        return new ReturnValue(a.ConductComparison(_kind, b), "bool");
    }

    public override void PostCompilation()
    {
        _a.PostCompilation();
        _b.PostCompilation();
    }
}