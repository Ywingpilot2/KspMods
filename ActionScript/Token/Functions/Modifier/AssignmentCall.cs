using ActionLanguage.Exceptions;
using ActionLanguage.Token.Interaction;
using ActionLanguage.Token.Terms;

namespace ActionLanguage.Token.Functions.Modifier;

public class AssignmentCall : TokenCall
{
    private Input Input { get; }

    private bool _isField;
    private string _term;
    private string _field;

    public override ReturnValue Call()
    {
        if (_isField)
        {
            if (!GetTerm(_term).SetField(_field, Input.GetValue().GetValue()))
                throw new InvalidAssignmentException(Line, GetTerm(_term));
        }
        else
        {
            if (!GetTerm(_term).CopyFrom(Input.GetValue()))
                throw new InvalidAssignmentException(Line, GetTerm(_term));
        }
        
        return new ReturnValue();
    }

    public override void PostCompilation()
    {
        Input.PostCompilation();
    }

    public AssignmentCall(BaseTerm term, Input input, ITokenHolder script, int line) : base(script, line)
    {
        _term = term.Name;
        Input = input;
    }
    
    public AssignmentCall(BaseTerm term, string field, Input input, ITokenHolder script, int line) : base(script, line)
    {
        _term = term.Name;
        _field = field;
        _isField = true;
        Input = input;
    }
}