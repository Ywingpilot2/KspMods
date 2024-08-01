using System;
using SteelLanguage.Reflection.Type;
using SteelLanguage.Token.Interaction;
using SteelLanguage.Token.Terms;

namespace SteelLanguage.Token.Functions.Single;

public readonly record struct TermConstructor : IFunction
{
    public string Name => GetSig();
    public string ReturnType { get; }

    public string[] InputTypes { get; }
    public ConstructorKind Kind { get; }
    
    private readonly Func<BaseTerm[], ReturnValue> _filled;
    private readonly Func<ReturnValue> _partial;

    public ReturnValue Execute(params BaseTerm[] terms)
    {
        if (Kind == ConstructorKind.Partial)
            _partial.Invoke();
        
        return _filled.Invoke(terms);
    }

    public ReturnValue Execute()
    {
        if (Kind == ConstructorKind.Filled)
            _filled.Invoke(new BaseTerm[0]);
        
        return _partial.Invoke();
    }

    public string GetSig()
    {
        if (Kind is ConstructorKind.Empty or ConstructorKind.Partial)
        {
            return "";
        }

        return string.Join(" ", InputTypes);
    }

    public TermConstructor()
    {
        InputTypes = new string[0];
    }

    public TermConstructor(Func<BaseTerm[], ReturnValue> filled, params string[] inputTypes)
    {
        _filled = filled;
        InputTypes = inputTypes;
        
        Kind = ConstructorKind.Filled;
    }

    public TermConstructor(Func<ReturnValue> partial)
    {
        _partial = partial;
        Kind = ConstructorKind.Partial;
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