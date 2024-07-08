using System;
using SteelLanguage.Token.Functions;
using SteelLanguage.Token.Interaction;
using SteelLanguage.Token.Terms;

namespace SteelLanguage.Reflection;

public struct TermConstructor : IFunction
{
    public string Name => GetSig();
    public string ReturnType { get; }

    public string[] InputTypes { get; }
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

        return string.Join(" ", InputTypes);
    }

    public TermConstructor()
    {
        InputTypes = new string[0];
    }

    public TermConstructor(Func<BaseTerm[], ReturnValue> action, params string[] inputTypes)
    {
        _action = action;
        InputTypes = inputTypes;
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