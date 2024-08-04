using System;
using SteelLanguage.Library.System.Terms.Literal;
using SteelLanguage.Token.Interaction;
using SteelLanguage.Token.Terms;

namespace SteelLanguage.Token.Functions.Modifier;

public class FieldCall : TokenCall
{
    private readonly bool _static;
    private readonly string _field;
    
    private readonly Input _input;
    private readonly string _type;
    
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

        BaseTerm value = _input.GetValue();
        if (value is NullTerm)
            throw new NullReferenceException($"Field {_field} requested at line {Line} cannot be gotten because the term referenced was null");
        
        return value.GetField(_field).Value;
    }

    public override void PostCompilation()
    {
        if (_static)
            return;

        _input.PostCompilation();
    }
}