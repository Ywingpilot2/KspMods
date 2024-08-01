using System.Collections.Generic;
using SteelLanguage.Library.System.Terms.Complex;
using SteelLanguage.Library.System.Terms.Complex.Enumerators;
using SteelLanguage.Token.Fields;
using SteelLanguage.Token.Functions;
using SteelLanguage.Token.Interaction;
using SteelLanguage.Token.Terms;

namespace SteelLanguage.Library.Collections.Terms;

public class ReadOnlyCollectionTerm : EnumeratorTerm
{
    public override string ValueType => "ReadOnlyCollection";
    private TermArray _value = new TermArray();
    
    public override IEnumerable<TermField> GetFields()
    {
        foreach (TermField field in base.GetFields())
        {
            yield return field;
        }

        yield return new TermField("length", "int", _value.Length);
    }
    
    public override IEnumerable<IFunction> GetFunctions()
    {
        foreach (IFunction function in base.GetFunctions())
        {
            yield return function;
        }
        
        yield return new Function("get", GetEnumerationType(), terms =>
        {
            BaseTerm term = _value.GetValue(terms[0].CastToInt());
            return new ReturnValue(term.GetValue(), _value.ValueType);
        }, "int");
    }
    
    public override bool CopyFrom(BaseTerm term)
    {
        if (term is ReadOnlyCollectionTerm array)
        {
            _value = array._value;
            ContainedType = array.ContainedType; // TODO: Should we do this if the contained types don't match?
            return true;
        }

        return false;
    }
    
    public override bool SetValue(object value)
    {
        if (value is TermArray array)
        {
            _value = array;
            ContainedType.SetValue(array.ValueType, 0);
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
    
    public override object GetValue()
    {
        return _value;
    }
}