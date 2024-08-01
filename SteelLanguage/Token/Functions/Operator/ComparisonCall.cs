using SteelLanguage.Token.Functions.Modifier;
using SteelLanguage.Token.Interaction;
using SteelLanguage.Token.Terms;
using SteelLanguage.Utils;

namespace SteelLanguage.Token.Functions.Operator;

public class ComparisonCall : TokenCall
{
    private readonly Input _a;
    private readonly Input _b;
    private readonly ComparisonOperatorKind _kind;

    public ComparisonCall(ITokenHolder container, int line, Input a, Input b, ComparisonOperatorKind kind) : base(container, line)
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