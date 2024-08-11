using System;
using SteelLanguage.Exceptions;
using SteelLanguage.Library.System.Terms.Literal;
using SteelLanguage.Reflection.Type;
using SteelLanguage.Token.Functions;
using SteelLanguage.Token.Terms;

namespace SteelLanguage.Token.Interaction;

public enum InputType
{
    Null,
    Constant,
    Term,
    Call
}

public readonly record struct Input
{
    public InputType Type { get; }
    private TokenCall Call { get; }
    private readonly TermHolder _term;
    private readonly ITokenHolder _container;

    public BaseTerm GetValue()
    {
        switch (Type)
        {
            case InputType.Constant:
            case InputType.Term:
            {
                return _term.GetTerm();
            }
            case InputType.Call:
            {
                Call.PreExecution();
                ReturnValue value = Call.Call();
                Call.PostExecution();
                
                if (!value.HasValue)
                    return new NullTerm();
                    
                //return new BaseTerm(Guid.NewGuid().ToString(), value.Value.ToString(), Call.Function.ValueType);
                if (!_container.GetLibraryManager().HasTermType(value.Type))
                    throw new TypeNotExistException(Call.Line, value.Type);

                TermType type = _container.GetLibraryManager().GetTermType(value.Type);
                BaseTerm term = type.Construct(null, Call.Line, _container.GetLibraryManager());
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

    public Input(ITokenHolder container, TokenCall call)
    {
        Type = InputType.Call;
        Call = call;
        _term = null;
        _container = container;
    }

    public Input(BaseTerm term)
    {
        Type = InputType.Term;
        _term = new TermHolder(term);
        Call = null;
    }

    public Input(TermHolder holder)
    {
        Type = InputType.Term;
        _term = holder;
        Call = null;
    }
}