using System.Collections.Generic;
using SteelLanguage.Library.System.Terms.Literal;
using SteelLanguage.Token.Fields;
using SteelLanguage.Token.Terms;

namespace SteelLanguage.Library.System.Terms.Complex;

public class EnumTerm : BaseTerm
{
    public override string ValueType => "enum";
    
    /// <summary>
    /// The Enum values this enum contains.
    /// These all need to be ints with their type property set to <see cref="ValueType"/>
    /// they will be passed through the reflection system as Static <see cref="TermField"/>s
    /// </summary>
    protected virtual string[] Values { get; }
    protected int Value { get; set; }

    public override IEnumerable<TermField> GetStaticFields()
    {
        foreach (TermField staticField in base.GetStaticFields())
        {
            yield return staticField;
        }

        for (var i = 0; i < Values.Length; i++)
        {
            var name = Values[i];
            yield return new TermField(name, ValueType, i);
        }
    }

    public override bool CanImplicitCastToInt => true;
    public override int CastToInt()
    {
        return Value;
    }

    public override string CastToStr()
    {
        return Values[Value];
    }

    public override bool SetValue(object value)
    {
        if (value is int i)
        {
            Value = i;
        }

        return false;
    }

    public override bool CopyFrom(BaseTerm term)
    {
        if (term is EnumTerm enumTerm && enumTerm.ValueType == term.ValueType)
        {
            Value = enumTerm.Value;
            return true;
        }

        if (term is NumberTerm)
        {
            Value = term.CastToInt();
            return true;
        }
        
        return false;
    }

    public override object GetValue()
    {
        return Value;
    }
}