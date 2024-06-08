using System.Collections.Generic;
using System.Globalization;
using ActionScript.Exceptions;
using ActionScript.Functions;

namespace ActionScript.Terms;

public sealed class BoolTerm : BaseTerm
{
    public override string ValueType => "bool";
    public override IEnumerable<Function> Functions { get; }
    private bool _value;

    #region Casting

    public override string CastToStr()
    {
        return _value.ToString();
    }

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
        return (uint)CastToInt();
    }

    public override float CastToFloat()
    {
        return CastToInt();
    }

    #endregion

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
}