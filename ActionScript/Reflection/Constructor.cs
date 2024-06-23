using System;
using ActionLanguage.Library;
using ActionLanguage.Token.Functions;
using ActionLanguage.Token.Interaction;
using ActionLanguage.Token.Terms;

namespace ActionLanguage.Reflection;

public struct TermConstructor : IExecutable
{
    public string[] Inputs { get; }
    public ConstructorKind Kind { get; }
    
    private Func<BaseTerm[], ReturnValue> _action;

    public ReturnValue Execute(params BaseTerm[] terms)
    {
        return _action.Invoke(terms);
    }

    public string GetSig()
    {
        if (Kind == ConstructorKind.Empty)
        {
            return "";
        }

        return string.Join(" ", Inputs);
    }

    public TermConstructor()
    {
        Inputs = new string[0];
    }

    public TermConstructor(Func<BaseTerm[], ReturnValue> action, params string[] inputs)
    {
        _action = action;
        Inputs = inputs;
        Kind = ConstructorKind.Filled;
    }

    public void PreExecution()
    {
    }

    public void PostExecution()
    {
    }

    public void PostCompilation()
    {
    }
}