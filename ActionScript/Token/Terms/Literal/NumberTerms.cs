using System;
using System.Collections.Generic;
using System.Globalization;
using ActionLanguage.Library;
using ActionLanguage.Token.Functions;
using ActionLanguage.Token.Interaction;
using ActionLanguage.Utils;

namespace ActionLanguage.Token.Terms.Literal;

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
    protected virtual Type NumberType { get; }

    protected static string[] NumberTypes = new[]
    {
        "int",
        "uint",
        "float",
        "double"
    };

    #region Casting

    public override bool CanImplicitCastToInt => true;
    public override bool CanImplicitCastToFloat => true;
    public override bool CanImplicitCastToDouble => true;

    public override bool CastToBool()
    {
        int value = (int)Number;
        return value >= 1;
    }

    public override int CastToInt()
    {
        return Convert.ToInt32(Number);
    }
    
    public override uint CastToUint()
    {
        return (uint)Math.Abs(Convert.ToInt32(Number));
    }
    
    public override double CastToDouble()
    {
        return Convert.ToDouble(Number);
    }
    
    public override float CastToFloat()
    {
        return Convert.ToSingle(Number);
    }

    public override string CastToStr()
    {
        return Number.ToString();
    }

    #endregion

    #region Math

    public override OperatorKind[] AllowedOperators => new[]
    {
        OperatorKind.Add,
        OperatorKind.Subtract,
        OperatorKind.Divide,
        OperatorKind.Multiply
    };

    public override object ConductOperation(OperatorKind kind, BaseTerm subject)
    {
        double a = CastToDouble();
        double b = subject.CastToDouble();
        
        switch (kind)
        {
            case OperatorKind.Add:
            {
                return Convert.ChangeType(a + b, NumberType);
            }
            case OperatorKind.Subtract:
            {
                return Convert.ChangeType(a - b, NumberType);
            }
            case OperatorKind.Multiply:
            {
                return Convert.ChangeType(a * b, NumberType);
            }
            case OperatorKind.Divide:
            {
                return Convert.ChangeType(a / b, NumberType);
            }
            default:
                throw new ArgumentOutOfRangeException(nameof(kind), kind, null);
        }
    }

    #endregion

    #region Creation

    public override IEnumerable<TermConstructor> GetConstructors()
    {
        yield return new TermConstructor(_ => new ReturnValue(0, "int"));
    }

    #endregion

    public override bool CopyFrom(BaseTerm term)
    {
        if (term.Kind == TermKind.Null)
            return false;
        
        if (term is not NumberTerm num)
            return false;

        Number = (ValueType)num.CastToType(ValueType);
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
    protected override Type NumberType => typeof(int);

    public override bool CanImplicitCastToBool => true;

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

    public override IEnumerable<IFunction> GetFunctions()
    {
        foreach (IFunction function in base.GetFunctions())
        {
            yield return function;
        }

        yield return new Function("increment", "void", terms =>
        {
            BaseTerm term = terms[0];
            term.SetValue(term.CastToInt() + 1);

            return new ReturnValue();
        });
        yield return new Function("decrement", "void", terms =>
        {
            BaseTerm term = terms[0];
            term.SetValue(term.CastToInt() - 1);

            return new ReturnValue();
        });
    }

    public override bool SetValue(object value)
    {
        if (value is IConvertible convertible)
        {
            Number = convertible.ToInt32(new NumberFormatInfo());
            Kind = TermKind.Basic;
            return true;
        }

        return false;
    }
    
    #region Math

    public override OperatorKind[] AllowedOperators => new[]
    {
        OperatorKind.Add,
        OperatorKind.Subtract,
        OperatorKind.Divide,
        OperatorKind.Multiply,
        OperatorKind.And,
        OperatorKind.Or,
        OperatorKind.Power,
        OperatorKind.Remaining
    };

    public override object ConductOperation(OperatorKind kind, BaseTerm subject)
    {
        int a = CastToInt();
        int b = subject.CastToInt();

        return kind switch
        {
            OperatorKind.Add => Convert.ChangeType(a + b, NumberType),
            OperatorKind.Subtract => Convert.ChangeType(a - b, NumberType),
            OperatorKind.Multiply => Convert.ChangeType(a * b, NumberType),
            OperatorKind.Divide => Convert.ChangeType(a / b, NumberType),
            OperatorKind.And => a & b,
            OperatorKind.Or => a | b,
            OperatorKind.Power => a ^ b,
            OperatorKind.Remaining => a % b,
            _ => throw new ArgumentOutOfRangeException(nameof(kind), kind, null)
        };
    }

    #endregion
}

public class TermF : NumberTerm
{
    public override string ValueType => "float";
    protected override Type NumberType => typeof(float);
    
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
        if (value is IConvertible convertible)
        {
            Number = convertible.ToSingle(new NumberFormatInfo());
            Kind = TermKind.Basic;
            return true;
        }

        return false;
    }
    
    #region Math

    public override OperatorKind[] AllowedOperators => new[]
    {
        OperatorKind.Add,
        OperatorKind.Subtract,
        OperatorKind.Divide,
        OperatorKind.Multiply
    };

    public override object ConductOperation(OperatorKind kind, BaseTerm subject)
    {
        float a = CastToFloat();
        float b = subject.CastToFloat();
        
        switch (kind)
        {
            case OperatorKind.Add:
            {
                return Convert.ChangeType(a + b, NumberType);
            }
            case OperatorKind.Subtract:
            {
                return Convert.ChangeType(a - b, NumberType);
            }
            case OperatorKind.Multiply:
            {
                return Convert.ChangeType(a * b, NumberType);
            }
            case OperatorKind.Divide:
            {
                return Convert.ChangeType(a / b, NumberType);
            }
            default:
                throw new ArgumentOutOfRangeException(nameof(kind), kind, null);
        }
    }

    #endregion
}

public class TermU : NumberTerm
{
    public override string ValueType => "uint";
    protected override Type NumberType => typeof(uint);

    public override bool CanImplicitCastToBool => true;

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
    
    #region Math

    public override OperatorKind[] AllowedOperators => new[]
    {
        OperatorKind.Add,
        OperatorKind.Subtract,
        OperatorKind.Divide,
        OperatorKind.Multiply,
        OperatorKind.And,
        OperatorKind.Or,
        OperatorKind.Power,
        OperatorKind.Remaining
    };

    public override object ConductOperation(OperatorKind kind, BaseTerm subject)
    {
        uint a = CastToUint();
        uint b = subject.CastToUint();

        return kind switch
        {
            OperatorKind.Add => Convert.ChangeType(a + b, NumberType),
            OperatorKind.Subtract => Convert.ChangeType(a - b, NumberType),
            OperatorKind.Multiply => Convert.ChangeType(a * b, NumberType),
            OperatorKind.Divide => Convert.ChangeType(a / b, NumberType),
            OperatorKind.And => a & b,
            OperatorKind.Or => a | b,
            OperatorKind.Power => a ^ b,
            OperatorKind.Remaining => a % b,
            _ => throw new ArgumentOutOfRangeException(nameof(kind), kind, null)
        };
    }

    #endregion
}

public class TermD : NumberTerm
{
    public override string ValueType => "double";
    protected override Type NumberType => typeof(double);
    
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