using SteelLanguage.Exceptions;
using SteelLanguage.Token.Interaction;
using SteelLanguage.Token.Terms;

namespace SteelLanguage.Token.Functions.Modifier;

public class AssignmentCall : TokenCall
{
    private Input Input { get; }

    private readonly bool _isField;
    private readonly string _term;
    private readonly string _field;

    public override ReturnValue Call()
    {
        if (_isField)
        {
            BaseTerm term = Input.GetValue();
            object value = term.GetValue();
            if (GetTerm(_term).GetField(_field).Value.Type != term.ValueType)
                value = term.CastToType(GetTerm(_term).GetField(_field).Value.Type);
            
            if (!GetTerm(_term).SetField(_field, value))
                throw new InvalidAssignmentException(Line, GetTerm(_term));
        }
        else
        {
            if (!GetTerm(_term).CopyFrom(Input.GetValue()))
                throw new InvalidAssignmentException(Line, GetTerm(_term));
        }
        
        return ReturnValue.None;
    }

    public override void PostCompilation()
    {
        Input.PostCompilation();
    }

    public AssignmentCall(BaseTerm term, Input input, ITokenHolder container, int line) : base(container, line)
    {
        _term = term.Name;
        Input = input;
    }
    
    public AssignmentCall(BaseTerm term, string field, Input input, ITokenHolder container, int line) : base(container, line)
    {
        _term = term.Name;
        _field = field;
        _isField = true;
        Input = input;
    }
}