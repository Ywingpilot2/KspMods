using System.Collections.Generic;
using SteelLanguage.Token.Fields;
using SteelLanguage.Token.Functions;
using SteelLanguage.Token.Interaction;

namespace SteelLanguage.Token.Terms.Complex.Enumerators;

public class ReadOnlyCollectionTerm : EnumeratorTerm
{
    public override string ValueType => "readonly_collection";
    
    public override IEnumerable<TermField> GetFields()
    {
        foreach (TermField field in base.GetFields())
        {
            yield return field;
        }

        yield return new TermField("length", "int", ((TermArray)Value).Length);
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
    }
    
    public override bool CopyFrom(BaseTerm term)
    {
        if (term is ReadOnlyCollectionTerm array)
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
}