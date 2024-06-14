using ActionLanguage.Token.Interaction;

namespace ActionLanguage.Token.Functions;

public class FieldCall : TokenCall
{
    private Input _input;
    private string _field;
    
    public FieldCall(ITokenHolder script, int line, Input input, string field) : base(script, line)
    {
        _input = input;
        _field = field;
    }

    public override ReturnValue Call()
    {
        return _input.GetValue().GetField(_field).Value;
    }

    public override void PostCompilation()
    {
        _input.PostCompilation();
    }
}