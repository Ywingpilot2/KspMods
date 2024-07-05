using System;
using SteelLanguage.Exceptions;
using SteelLanguage.Reflection;
using SteelLanguage.Token.Functions;
using SteelLanguage.Token.Terms;
using SteelLanguage.Token.Terms.Literal;

namespace SteelLanguage.Token.Interaction;

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
            }
            case InputType.Call:
            {
                Call.PreExecution();
                ReturnValue value = Call.Call();
                Call.PostExecution();
                
                if (!value.HasValue)
                    return new NullTerm();
                    
                //return new BaseTerm(Guid.NewGuid().ToString(), value.Value.ToString(), Call.Function.ValueType);
                if (!_script.GetLibraryManager().HasTermType(value.Type))
                    throw new TypeNotExistException(Call.Line, value.Type);

                TermType type = _script.GetLibraryManager().GetTermType(value.Type);
                BaseTerm term = type.Construct(Guid.NewGuid().ToString(), Call.Line, _script.GetLibraryManager());
                if (term.SetValue(value.Value) && term.Kind == TermKind.Null)
                {
                    term.Kind = TermKind.Basic;
                }

                return term;
            }
            default:
            {
                return new NullTerm();
            }
        }
    }

    public void PostCompilation()
    {
        if (Type == InputType.Call)
        {
            Call.PostCompilation();
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