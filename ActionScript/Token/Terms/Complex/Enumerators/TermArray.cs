using System;
using System.Collections;
using System.Collections.Generic;
using ActionLanguage.Exceptions;
using ActionLanguage.Library;
using ActionLanguage.Token.Fields;
using ActionLanguage.Token.Functions;
using ActionLanguage.Token.Interaction;
using ActionLanguage.Token.Terms.Literal;

namespace ActionLanguage.Token.Terms.Complex.Enumerators;

public struct TermArray : IEnumerable
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

    public override bool SetValue(object value)
    {
        // TODO: This is really expensive!!!!
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
        
        // TODO: Find a way to make the return type the contained type, that way we don't need to always cast
        yield return new Function("get", "term", terms =>
        {
            TermArray array = (TermArray)terms[0].GetValue();
            BaseTerm term = array.GetValue(terms[1].CastToInt());
            return new ReturnValue(term.GetValue(), array.ValueType);
        }, "int");
        yield return new Function("set", "void", terms =>
        {
            TermArray array = (TermArray)terms[0].GetValue();
            BaseTerm term = terms[1];
            BaseTerm i = terms[2];
            array.SetValue(term, i.CastToInt());
            return new ReturnValue();
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

    public ArrayTerm()
    {
        Value = new TermArray();
    }
}