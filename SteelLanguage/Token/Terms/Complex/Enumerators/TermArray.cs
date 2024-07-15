using System.Collections;
using System.Collections.Generic;
using SteelLanguage.Exceptions;
using SteelLanguage.Reflection;
using SteelLanguage.Token.Fields;
using SteelLanguage.Token.Functions;
using SteelLanguage.Token.Interaction;
using SteelLanguage.Token.Terms.Literal;

namespace SteelLanguage.Token.Terms.Complex.Enumerators;

public class TermArray : IEnumerable
{
    public string ValueType { get; }
    public int Length { get; }
    private BaseTerm[] _terms;

    public void SetValue(BaseTerm value, int idx)
    {
        if (value.GetTermType().Name != ValueType && !value.GetTermType().IsSubclassOf(ValueType))
            throw new InvalidParametersException(0, new[] { ValueType });
        
        _terms.SetValue(value, idx);
    }

    public BaseTerm GetValue(int idx)
    {
        return _terms[idx];
    }

    public IEnumerator GetEnumerator()
    {
        return _terms.GetEnumerator();
    }

    public TermArray()
    {
        ValueType = "term";
        Length = 0;
        _terms = new BaseTerm[0];
    }
    
    public TermArray(string valueType, int length)
    {
        ValueType = valueType;
        Length = length;
        _terms = new BaseTerm[length];
        for (int i = 0; i < length; i++)
        {
            _terms.SetValue(new NullTerm(), i); // initialize everything to null by default to avoid issues
        }
    }
}

public class ArrayTerm : EnumeratorTerm
{
    public override string ValueType => "array";

    public override IEnumerable<TermField> GetFields()
    {
        foreach (TermField field in base.GetFields())
        {
            yield return field;
        }

        yield return new TermField("length", "int", ((TermArray)Value).Length);
    }

    public override IEnumerable<TermConstructor> GetConstructors()
    {
        yield return new TermConstructor(terms =>
        {
            int length = terms[0].CastToInt();
            return new ReturnValue(new TermArray(ContainedType, length), "array");
        }, "int");
    }

    public override IEnumerable<IFunction> GetFunctions()
    {
        foreach (IFunction function in base.GetFunctions())
        {
            yield return function;
        }
        
        yield return new Function("get", ContainedType, terms =>
        {
            TermArray array = (TermArray)Value;
            BaseTerm term = array.GetValue(terms[0].CastToInt());
            return new ReturnValue(term.GetValue(), array.ValueType);
        }, "int");
        yield return new Function("set", terms =>
        {
            TermArray array = (TermArray)Value;
            BaseTerm term = terms[0];
            BaseTerm i = terms[1];
            array.SetValue(term, i.CastToInt());
        }, "term", "int");
    }

    public override bool CopyFrom(BaseTerm term)
    {
        if (term is ArrayTerm array)
        {
            Value = array.Value;
            ContainedType = array.ContainedType; // TODO: Should we do this if the contained types don't match?
            return true;
        }

        return false;
    }
    
    public override bool SetValue(object value)
    {
        if (value is TermArray array)
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

    public ArrayTerm()
    {
        Value = new TermArray();
    }
}