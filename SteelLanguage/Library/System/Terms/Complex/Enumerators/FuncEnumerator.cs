using System.Collections;
using System.Collections.Generic;
using SteelLanguage.Reflection.Library;
using SteelLanguage.Reflection.Type;
using SteelLanguage.Token.Functions;
using SteelLanguage.Token.Interaction;
using SteelLanguage.Token.Terms;
using NotImplementedException = System.NotImplementedException;

namespace SteelLanguage.Library.System.Terms.Complex.Enumerators;

public record struct FuncEnumerable : IEnumerable
{
    private readonly IEnumerable<ReturnValue> _enumerable;
    public LibraryManager Manager;
    public string ValueType { get; }
    
    public FuncEnumerable(IEnumerable<ReturnValue> enumerable, string valueType)
    {
        _enumerable = enumerable;
        ValueType = valueType;
    }

    public IEnumerator GetEnumerator()
    {
        foreach (ReturnValue value in _enumerable)
        {
            TermType type = Manager.GetTermType(value.Type);
            BaseTerm term = type.Construct(null, 0, Manager); // TODO: this is so hacky!!!
            term.SetValue(value.Value);

            yield return term;
        }
    }
}

/// <summary>
/// Term representing <see cref="Function"/>s set to YieldReturn
/// TODO: Hacky work around
/// </summary>
public class FuncEnumeratorTerm : EnumeratorTerm
{
    public override string ValueType => "FuncEnumerator";
    public override bool SetValue(object value)
    {
        if (value is FuncEnumerable enumerable)
        {
            enumerable.Manager = TypeLibrary;
            Value = enumerable;
            return true;
        }
        
        return false;
    }

    public override bool CopyFrom(BaseTerm term)
    {
        if (term is FuncEnumeratorTerm enumeratorTerm)
        {
            Value = enumeratorTerm.Value;
            return true;
        }
        
        return false;
    }
}