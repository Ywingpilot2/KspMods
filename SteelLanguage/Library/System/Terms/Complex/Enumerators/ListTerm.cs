using System.Collections;
using System.Collections.Generic;
using SteelLanguage.Exceptions;
using SteelLanguage.Library.System.Terms.Literal;
using SteelLanguage.Token.Functions.Modifier;
using SteelLanguage.Token.Functions.Single;
using SteelLanguage.Token.Interaction;
using SteelLanguage.Token.Terms;
using SteelLanguage.Utils;

namespace SteelLanguage.Library.System.Terms.Complex.Enumerators;

public class TermList : ICollection<BaseTerm>
{
    public int Count => _baseTerms.Count;
    public string ValueType { get; }
    public bool IsReadOnly => false;
    private readonly List<BaseTerm> _baseTerms;

    public IEnumerator<BaseTerm> GetEnumerator()
    {
        return _baseTerms.GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }

    public void Add(BaseTerm item)
    {
        if (item == null)
            return;
        
        if (item.GetTermType().Name != ValueType && !item.GetTermType().IsSubclassOf(ValueType))
            throw new InvalidParametersException(0, new[] { ValueType });
        
        _baseTerms.Add(item);
    }

    public void Clear()
    {
        _baseTerms.Clear();
    }

    public bool Contains(BaseTerm item)
    {
        if (item == null)
            item = new NullTerm(); // compare against null on our terms instead to avoid complications
        
        for (int i = 0; i < _baseTerms.Count; i++)
        {
            if (_baseTerms[i].ConductComparison(ComparisonOperatorKind.Equal, item))
                return true;
        }

        return false;
    }

    public void CopyTo(BaseTerm[] array, int arrayIndex)
    {
        throw new global::System.NotImplementedException();
    }

    public bool Remove(BaseTerm item)
    {
        if (!Contains(item))
            return false;

        for (int i = 0; i < _baseTerms.Count; i++)
        {
            if (!_baseTerms[i].ConductComparison(ComparisonOperatorKind.Equal, item))
                continue;
            
            _baseTerms.RemoveAt(i);
            break;
        }
        
        return false;
    }

    public TermList(string valueType)
    {
        _baseTerms = new List<BaseTerm>();
        ValueType = valueType;
    }
}

public class ListTerm : CollectionTerm
{
    public override string ValueType => "List";
    protected override int Count => ((TermList)Value).Count;

    public override IEnumerable<TermConstructor> GetConstructors()
    {
        yield return new TermConstructor(_ => new ReturnValue(new TermList(ContainedType), "list"));
        yield return new TermConstructor(terms =>
        {
            TermList list = new TermList(ContainedType);
            EnumeratorTerm term = (EnumeratorTerm)terms[0];

            foreach (BaseTerm baseTerm in term.Value)
            {
                list.Add(baseTerm);
            }

            return new ReturnValue(list, "list");
        }, "enumerable");
    }

    protected override void Add(BaseTerm term)
    {
        ((TermList)Value).Add(term);
    }

    protected override bool Remove(BaseTerm term)
    {
        return ((TermList)Value).Remove(term);
    }

    protected override bool Contains(BaseTerm term)
    {
        return ((TermList)Value).Contains(term);
    }

    protected override void Clear()
    {
        ((TermList)Value).Clear();
    }
    
    public override bool SetValue(object value)
    {
        if (value is TermList array)
        {
            Value = array;
            ContainedType = array.ValueType;
            Kind = TermKind.Class;
            return true;
        }

        if (value == null)
        {
            Value = null;
            ContainedType = null;
            Kind = TermKind.Null;
            return true;
        }

        return false;
    }
    
    public override bool CopyFrom(BaseTerm term)
    {
        if (term is ListTerm list)
        {
            Value = list.Value;
            ContainedType = list.ContainedType; // TODO: Should we do this if the contained types don't match?
            return true;
        }

        return false;
    }
}