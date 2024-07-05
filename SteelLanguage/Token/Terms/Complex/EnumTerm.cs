using System;
using System.Collections.Generic;
using SteelLanguage.Token.Fields;
using SteelLanguage.Token.Functions;
using SteelLanguage.Token.Interaction;
using SteelLanguage.Token.Terms.Literal;

namespace SteelLanguage.Token.Terms.Complex;

public class EnumTerm : BaseTerm
{
    public override string ValueType => "enum";
    
    /// <summary>
    /// The Enum values this enum contains.
    /// These all need to be ints with their type property set to <see cref="ValueType"/>
    /// they will be passed through the reflection system as Static <see cref="TermField"/>s
    /// </summary>
    protected virtual TermField[] Values { get; }
    protected int Value { get; set; }

    public override IEnumerable<TermField> GetStaticFields()
    {
        foreach (TermField staticField in base.GetStaticFields())
        {
            yield return staticField;
        }

        foreach (TermField value in Values)
        {
            yield return value;
        }
    }

    public override bool CanImplicitCastToInt => true;

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