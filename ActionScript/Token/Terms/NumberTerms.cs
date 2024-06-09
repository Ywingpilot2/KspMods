using System;
using System.Collections.Generic;
using System.Globalization;
using ActionScript.Token.Functions;

namespace ActionScript.Token.Terms;

public static class NumberConversion
{
    public static string[] NumberTypes = new[]
    {
        "int",
        "uint",
        "float",
        "double"
    };
}

public class NumberTerm : BaseTerm
{
    public override string ValueType => "number-term";

    public override bool Parse(string value)
    {
        throw new NotImplementedException();
    }

    public override bool SetValue(object value)
    {
        throw new NotImplementedException();
    }

    protected ValueType Number;

    protected static string[] NumberTypes = new[]
    {
        "int",
        "uint",
        "float",
        "double"
    };

    #region Casting

    public override bool CastToBool()
    {
        int value = (int)Number;
        return value >= 1;
    }

    public override int CastToInt()
    {
        return (int)Number;
    }
    
    public override uint CastToUint()
    {
        return (uint)Math.Abs((int)Number);
    }
    
    public override double CastToDouble()
    {
        return (double)Number;
    }
    
    public override float CastToFloat()
    {
        return (float)Number;
    }

    public override string CastToStr()
    {
        return Number.ToString();
    }

    #endregion

    public override bool CopyFrom(BaseTerm term)
    {
        if (term.Kind == TermKind.Null)
            return false;
        
        if (term is not NumberTerm num)
            return false;

        Number = num.Number;
        Kind = num.Kind;

        return true;
    }
    
    public override object GetValue()
    {
        return Number;
    }
}

public class TermI : NumberTerm
{
    public override string ValueType => "int";
    
    public override bool Parse(string value)
    {
        if (int.TryParse(value, NumberStyles.Integer & NumberStyles.AllowLeadingSign, new NumberFormatInfo(), out int i))
        {
            Number = i;
            Kind = TermKind.Basic;
            return true;
        }
        else
        {
            return false;
        }
    }

    public override bool SetValue(object value)
    {
        if (value is not int i)
            return false;

        Number = i;
        Kind = TermKind.Basic;
        return true;
    }
}

public class TermF : NumberTerm
{
    public override string ValueType => "float";
    
    public override bool Parse(string value)
    {
        value = value.StartsWith("0x") ? value.Replace("0x", "") : value;

        if (float.TryParse(value, NumberStyles.Float, new NumberFormatInfo(), out float i))
        {
            Number = i;
            Kind = TermKind.Basic;
            return true;
        }
        else
        {
            return false;
        }
    }
    
    public override bool SetValue(object value)
    {
        if (value is not float i)
            return false;

        Number = i;
        Kind = TermKind.Basic;
        return true;
    }
}

public class TermU : NumberTerm
{
    public override string ValueType => "uint";
    
    public override bool Parse(string value)
    {
        value = value.StartsWith("0x") ? value.Replace("0x", "") : value;

        if (uint.TryParse(value, NumberStyles.Integer & NumberStyles.HexNumber, new NumberFormatInfo(), out uint i))
        {
            Number = i;
            Kind = TermKind.Basic;
            return true;
        }
        else
        {
            return false;
        }
    }
    
    public override bool SetValue(object value)
    {
        if (value is not uint i)
            return false;

        Number = i;
        Kind = TermKind.Basic;
        return true;
    }
}

public class TermD : NumberTerm
{
    public override string ValueType => "double";
    
    public override bool Parse(string value)
    {
        value = value.StartsWith("0x") ? value.Replace("0x", "") : value;

        if (double.TryParse(value, NumberStyles.Float, new NumberFormatInfo(), out double i))
        {
            Number = i;
            Kind = TermKind.Basic;
            return true;
        }
        else
        {
            return false;
        }
    }
    
    public override bool SetValue(object value)
    {
        if (value is not double i)
            return false;

        Number = i;
        Kind = TermKind.Basic;
        return true;
    }
}