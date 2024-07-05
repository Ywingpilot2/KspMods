using SteelLanguage.Token.Interaction;
using SteelLanguage.Token.Terms;
using SteelLanguage.Utils;

namespace SteelLanguage.Token.Functions.Operator;

public class MathCall : TokenCall
{
    private Input _a;
    private Input _b;
    private readonly MathOperatorKind _kind;

    public MathCall(ITokenHolder container, int line, Input a, Input b, MathOperatorKind kind) : base(container, line)
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