using System.Collections;
using System.Collections.Generic;
using SteelLanguage.Exceptions;
using SteelLanguage.Library.System.Terms.Complex;
using SteelLanguage.Library.System.Terms.Literal;
using SteelLanguage.Token.Functions;
using SteelLanguage.Token.Functions.Modifier;
using SteelLanguage.Token.Functions.Single;
using SteelLanguage.Token.Interaction;
using SteelLanguage.Token.Terms;

namespace SteelLanguage.Library.Collections.Terms;

internal record TermList : ICollection<BaseTerm>
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

    public BaseTerm AtIndex(int idx)
    {
        return _baseTerms[idx];
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
    protected override int Count => _value.Count;
    private TermList _value = new TermList("term");

    public override IEnumerable<TermConstructor> GetConstructors()
    {
        yield return new TermConstructor(() => new ReturnValue(new TermList(GetEnumerationType()), "List"));
        yield return new TermConstructor(terms =>
        {
            TermList list = new TermList(GetEnumerationType());
            EnumeratorTerm term = (EnumeratorTerm)terms[0];

            foreach (BaseTerm baseTerm in term.GetEnumerableValue())
            {
                list.Add(baseTerm);
            }

            return new ReturnValue(list, "List");
        }, $"Enumerable<{GetEnumerationType()}>");
    }

    #region Functions

    public override IEnumerable<IFunction> GetFunctions()
    {
        foreach (IFunction function in base.GetFunctions())
        {
            yield return function;
        }

        yield return new Function("contains", "bool", terms => new ReturnValue(_value.Contains(terms[0]), ValueType), GetEnumerationType());
    }

    #region Collection

    protected override void Add(BaseTerm[] term)
    {
        _value.Add(term[0]);
    }

    protected override bool Remove(BaseTerm[] term)
    {
        return _value.Remove(term[0]);
    }

    protected override void Clear()
    {
        _value.Clear();
    }
    
    public override bool SetValue(object value)
    {
        if (value is TermList array)
        {
            _value = array;
            ContainedType = new[]
            {
                _value.ValueType
            };
            Kind = TermKind.Class;
            return true;
        }

        if (value == null)
        {
            _value = null;
            ContainedType = null;
            Kind = TermKind.Null;
            return true;
        }

        return false;
    }

    #endregion

    #endregion

    public override string IndexerType => "int";
    public override bool SupportsIndexing => true;
    public override string IndexingReturnType => GetEnumerationType();

    public override ReturnValue ConductIndexingOperation(BaseTerm input)
    {
        BaseTerm element = _value.AtIndex(input.CastToInt());
        return new ReturnValue(element.GetValue(), element.ValueType);
    }

    public override bool CopyFrom(BaseTerm term)
    {
        if (term is ListTerm list)
        {
            _value = list._value;
            ContainedType = list.ContainedType; // TODO: Should we do this if the contained types don't match?
            return true;
        }

        return false;
    }
    
    public override object GetValue()
    {
        return _value;
    }
}