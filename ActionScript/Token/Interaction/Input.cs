using System;
using ActionLanguage.Exceptions;
using ActionLanguage.Library;
using ActionLanguage.Token.Functions;
using ActionLanguage.Token.Terms;

namespace ActionLanguage.Token.Interaction;

public enum InputType
{
    Null,
    Constant,
    Term,
    Call
}
    
public struct Input
{
    public InputType Type { get; }
    private TokenCall Call { get; }
    private BaseTerm _term;
    private ITokenHolder _script;

    public BaseTerm GetValue()
    {
        switch (Type)
        {
            case InputType.Constant:
            case InputType.Term:
            {
                return _term;
            }break;
            case InputType.Call:
            {
                ReturnValue value = Call.Call();
                if (!value.HasValue)
                    return new NullTerm();
                    
                //return new BaseTerm(Guid.NewGuid().ToString(), value.Value.ToString(), Call.Function.ValueType);
                if (!_script.TermTypeExists(value.Type))
                    throw new TypeNotExistException(Call.Line, value.Type);

                TermType type = _script.GetTermType(value.Type);
                BaseTerm term = type.Construct(Guid.NewGuid().ToString(), Call.Line);
                if (term.SetValue(value.Value) && term.Kind == TermKind.Null)
                {
                    term.Kind = TermKind.Basic;
                }

                return term;
            } break;
            default:
            {
                return null;
            } break;
        }
    }

    public Input(ITokenHolder script, TokenCall call)
    {
        Type = InputType.Call;
        Call = call;
        _term = null;
        _script = script;
    }

    public Input(BaseTerm term)
    {
        Type = InputType.Term;
        _term = term;
        Call = null;
    }
}