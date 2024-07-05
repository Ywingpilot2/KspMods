using SteelLanguage.Token.Interaction;

namespace SteelLanguage.Token.Functions.Modifier;

public class FieldCall : TokenCall
{
    private bool _static;
    private string _field;
    
    private Input _input;
    private string _type;
    
    public FieldCall(ITokenHolder container, int line, Input input, string field) : base(container, line)
    {
        _input = input;
        _field = field;
    }

    public FieldCall(ITokenHolder container, int line, string type, string field) : base(container, line)
    {
        _static = true;
        _type = type;
        _field = field;
    }

    public override ReturnValue Call()
    {
        if (_static)
            return Container.GetLibraryManager().GetTermType(_type).GetStaticField(_field).Value;
        
        return _input.GetValue().GetField(_field).Value;
    }

    public override void PostCompilation()
    {
        if (_static)
            return;

        _input.PostCompilation();
    }
}