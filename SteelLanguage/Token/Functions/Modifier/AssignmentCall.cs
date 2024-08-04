using System;
using SteelLanguage.Exceptions;
using SteelLanguage.Library.System.Terms.Literal;
using SteelLanguage.Reflection.Type;
using SteelLanguage.Token.Interaction;
using SteelLanguage.Token.Terms;

namespace SteelLanguage.Token.Functions.Modifier;

public class AssignmentCall : TokenCall
{
    private Input Input { get; }
    
    private readonly string _term;
    private readonly string _field;
    private readonly AssignmentCallKind _kind;
    
    private enum AssignmentCallKind
    {
        Term = 0,
        LocalField = 2,
        StaticField = 3
    }

    public override ReturnValue Call()
    {
        switch (_kind)
        {
            case AssignmentCallKind.Term:
            {
                if (!GetTerm(_term).CopyFrom(Input.GetValue()))
                    throw new InvalidAssignmentException(Line, GetTerm(_term));
            } break;
            case AssignmentCallKind.LocalField:
            {
                BaseTerm term = Input.GetValue();
                object value = term.GetValue();
                if (GetTerm(_term) is NullTerm)
                    throw new NullReferenceException(
                        $"The requested assignment call at line {Line} for the field {_field} cannot be conducted because the referenced term({_term}) was null");
                
                if (GetTerm(_term).GetField(_field).Value.Type != term.ValueType)
                    value = term.CastToType(GetTerm(_term).GetField(_field).Value.Type);
            
                if (!GetTerm(_term).SetField(_field, value))
                    throw new InvalidAssignmentException(Line, GetTerm(_term));
            } break;
            case AssignmentCallKind.StaticField:
            {
                BaseTerm term = Input.GetValue();
                object value = term.GetValue();
                TermType type = GetTermType(_term);
                if (type.GetStaticField(_field).Value.Type != term.ValueType)
                    value = term.CastToType(GetTerm(_term).GetField(_field).Value.Type);

                if (!type.SetStaticField(_field, value))
                    throw new InvalidAssignmentException(Line, _field, _term);
            } break;
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
        _kind = AssignmentCallKind.Term;
    }
    
    public AssignmentCall(BaseTerm term, string field, Input input, ITokenHolder container, int line) : base(container, line)
    {
        _term = term.Name;
        _field = field;
        Input = input;
        _kind = AssignmentCallKind.LocalField;
    }

    public AssignmentCall(TermType type, string field, Input input, ITokenHolder container, int line) : base(container,
        line)
    {
        _term = type.Name;
        _field = field;
        Input = input;
        _kind = AssignmentCallKind.StaticField;
    }
}