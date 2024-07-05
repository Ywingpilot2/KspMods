using System;
using System.Collections.Generic;
using System.Globalization;
using SteelLanguage.Reflection;
using SteelLanguage.Token.Interaction;
using SteelLanguage.Utils;

namespace SteelLanguage.Token.Terms.Literal;

public sealed class BoolTerm : BaseTerm
{
    public override string ValueType => "bool";
    private bool _value;

    #region Casting

    public override string CastToStr()
    {
        return _value.ToString();
    }

    public override bool CanImplicitCastToInt => true;

    public override int CastToInt()
    {
        return _value ? 1 : 0;
    }

    public override bool CastToBool()
    {
        return _value;
    }

    public override double CastToDouble()
    {
        return CastToInt();
    }

    public override uint CastToUint()
    {
        return (uint)Math.Abs(CastToInt());
    }

    public override float CastToFloat()
    {
        return CastToInt();
    }

    #endregion

    public override BoolOperatorKind[] AllowedBoolOps => new[]
    {
        BoolOperatorKind.And,
        BoolOperatorKind.Or
    };

    public override bool ConductBoolOp(BoolOperatorKind kind, BaseTerm subject)
    {
        if (kind == BoolOperatorKind.And)
            return _value && subject.CastToBool();

        return _value || subject.CastToBool();
    }

    public override IEnumerable<TermConstructor> GetConstructors()
    {
        yield return new TermConstructor(_ => new ReturnValue(false, "bool"));
    }

    public override bool Parse(string value)
    {
        if (bool.TryParse(value.ToLower(), out bool b))
        {
            _value = b;
            Kind = TermKind.Basic;
            return true;
        }

        if (int.TryParse(value, NumberStyles.Integer & NumberStyles.HexNumber, new NumberFormatInfo(), out int i))
        {
            _value = i >= 1;
            Kind = TermKind.Basic;
            return true;
        }

        return false;
    }

    public override bool SetValue(object value)
    {
        if (value is not bool b)
            return false;
        
        _value = b;
        Kind = TermKind.Basic;
        return true;
    }

    public override bool CopyFrom(BaseTerm term)
    {
        if (term is BoolTerm boolTerm)
        {
            _value = boolTerm._value;
            return true;
        }

        if (term is NumberTerm numberTerm)
        {
            _value = numberTerm.CastToBool();
            return true;
        }

        return false;
    }

    public override object GetValue()
    {
        return _value;
    }
}