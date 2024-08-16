using System;
using System.Collections.Generic;
using System.Globalization;
using SteelLanguage.Exceptions;
using SteelLanguage.Token.Fields;
using SteelLanguage.Token.Functions;
using SteelLanguage.Token.Functions.Modifier;
using SteelLanguage.Token.Functions.Operator;
using SteelLanguage.Token.Functions.Single;
using SteelLanguage.Token.Interaction;
using SteelLanguage.Token.Terms;
using SteelLanguage.Utils;

namespace SteelLanguage.Library.System.Terms.Literal;

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
    public override string ValueType => "number";

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
    protected static readonly NumberFormatInfo Info = new NumberFormatInfo();

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

    public override object CastToType(string name)
    {
        switch (name)
        {
            case "bool":
            {
                return CastToBool();
            }
            case "double":
            {
                return CastToDouble();
            }
            case "float":
            {
                return CastToFloat();
            }
            case "int":
            {
                return CastToInt();
            }
            case "uint":
            {
                return CastToUint();
            }
            case "guid":
            {
                return CastToGuid();
            }
            case "string":
            {
                return CastToStr();
            }
        }

        return null;
    }

    #endregion

    #region Math

    public override MathOperatorKind[] AllowedMathOps => new[]
    {
        MathOperatorKind.Add,
        MathOperatorKind.Subtract,
        MathOperatorKind.Divide,
        MathOperatorKind.Multiply
    };

    public override object ConductMath(MathOperatorKind kind, BaseTerm subject)
    {
        double a = CastToDouble();
        double b = subject.CastToDouble();
        
        switch (kind)
        {
            case MathOperatorKind.Add:
            {
                return Convert.ChangeType(a + b, NumberType);
            }
            case MathOperatorKind.Subtract:
            {
                return Convert.ChangeType(a - b, NumberType);
            }
            case MathOperatorKind.Multiply:
            {
                return Convert.ChangeType(a * b, NumberType);
            }
            case MathOperatorKind.Divide:
            {
                return Convert.ChangeType(a / b, NumberType);
            }
            default:
                throw new InvalidActionException(0, $"{kind} is not a supported math operator for {ValueType}");
        }
    }

    #endregion

    #region Comparison

    public override ComparisonOperatorKind[] AllowedComparisons => new[]
    {
        ComparisonOperatorKind.Equal,
        ComparisonOperatorKind.NotEqual,
        ComparisonOperatorKind.Greater,
        ComparisonOperatorKind.GreaterEqual,
        ComparisonOperatorKind.Lesser,
        ComparisonOperatorKind.LesserEqual
    };

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

    public override string ToString()
    {
        if (Number == null)
            return "NULL";

        return Number.ToString();
    }
}

public class TermI : NumberTerm
{
    public override string ValueType => "int";
    protected override Type NumberType => typeof(int);

    public override bool CanImplicitCastToBool => true;

    public override object CastToType(string name)
    {
        if (TypeLibrary.GetTermType(name).IsSubclassOf("enum"))
        {
            return Number;
        }
        
        return base.CastToType(name);
    }

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

        yield return new Function("increment", () =>
        {
            SetValue(CastToInt() + 1);
        });
        yield return new Function("decrement", () =>
        {
            SetValue(CastToInt() - 1);
        });
    }
    
    public override IEnumerable<TermField> GetStaticFields()
    {
        foreach (TermField staticField in base.GetStaticFields())
        {
            yield return staticField;
        }
        
        yield return new TermField("min", "int", int.MinValue);
        yield return new TermField("max", "int", int.MaxValue);
    }
    
    public override bool SetValue(object value)
    {
        if (value is IConvertible convertible)
        {
            Number = convertible.ToInt32(Info);
            Kind = TermKind.Basic;
            return true;
        }

        return false;
    }
    
    #region Math

    public override MathOperatorKind[] AllowedMathOps => new[]
    {
        MathOperatorKind.Add,
        MathOperatorKind.Subtract,
        MathOperatorKind.Divide,
        MathOperatorKind.Multiply,
        MathOperatorKind.And,
        MathOperatorKind.Or,
        MathOperatorKind.Power,
        MathOperatorKind.Remaining
    };

    public override object ConductMath(MathOperatorKind kind, BaseTerm subject)
    {
        int a = CastToInt();
        int b = subject.CastToInt();

        return kind switch
        {
            MathOperatorKind.Add => Convert.ChangeType(a + b, NumberType),
            MathOperatorKind.Subtract => Convert.ChangeType(a - b, NumberType),
            MathOperatorKind.Multiply => Convert.ChangeType(a * b, NumberType),
            MathOperatorKind.Divide => Convert.ChangeType(a / b, NumberType),
            MathOperatorKind.And => a & b,
            MathOperatorKind.Or => a | b,
            MathOperatorKind.Power => a ^ b,
            MathOperatorKind.Remaining => a % b,
            _ => throw new InvalidActionException(0, $"{kind} is not a supported math operator for {ValueType}")
        };
    }

    #endregion

    #region Comparison

    public override bool ConductComparison(ComparisonOperatorKind kind, BaseTerm subject)
    {
        int a = (int)Number;
        int b = subject.CastToInt();

        return kind switch
        {
            ComparisonOperatorKind.Equal => a == b,
            ComparisonOperatorKind.NotEqual => a != b,
            ComparisonOperatorKind.Greater => a > b,
            ComparisonOperatorKind.GreaterEqual => a >= b,
            ComparisonOperatorKind.Lesser => a < b,
            ComparisonOperatorKind.LesserEqual => a <= b,
            _ => throw new InvalidActionException(0, $"{kind} is not a supported equality operator for {ValueType}")
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

        if (float.TryParse(value, NumberStyles.Float, Info, out float i))
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
            Number = convertible.ToSingle(Info);
            Kind = TermKind.Basic;
            return true;
        }

        return false;
    }
    
    public override IEnumerable<TermField> GetStaticFields()
    {
        foreach (TermField staticField in base.GetStaticFields())
        {
            yield return staticField;
        }

        yield return new TermField("positive_infinity", "float", float.PositiveInfinity);
        yield return new TermField("negative_infinity", "float", float.NegativeInfinity);
        yield return new TermField("nan", "float", float.NaN);
        yield return new TermField("min", "float", float.MinValue);
        yield return new TermField("max", "float", float.MaxValue);
        yield return new TermField("epsilon", "float", float.Epsilon);
    }
    
    #region Math

    public override MathOperatorKind[] AllowedMathOps => new[]
    {
        MathOperatorKind.Add,
        MathOperatorKind.Subtract,
        MathOperatorKind.Divide,
        MathOperatorKind.Multiply
    };

    public override object ConductMath(MathOperatorKind kind, BaseTerm subject)
    {
        float a = CastToFloat();
        float b = subject.CastToFloat();
        
        switch (kind)
        {
            case MathOperatorKind.Add:
            {
                return Convert.ChangeType(a + b, NumberType);
            }
            case MathOperatorKind.Subtract:
            {
                return Convert.ChangeType(a - b, NumberType);
            }
            case MathOperatorKind.Multiply:
            {
                return Convert.ChangeType(a * b, NumberType);
            }
            case MathOperatorKind.Divide:
            {
                return Convert.ChangeType(a / b, NumberType);
            }
            default:
                throw new InvalidActionException(0, $"{kind} is not a supported math operator for {ValueType}");
        }
    }

    #endregion
    
    #region Comparison

    public override bool ConductComparison(ComparisonOperatorKind kind, BaseTerm subject)
    {
        float a = (float)Number;
        float b = subject.CastToFloat();

        return kind switch
        {
            ComparisonOperatorKind.Equal => Math.Abs(a - b) < 0.005,
            ComparisonOperatorKind.NotEqual => Math.Abs(a - b) > 0.005,
            ComparisonOperatorKind.Greater => a > b,
            ComparisonOperatorKind.GreaterEqual => a >= b,
            ComparisonOperatorKind.Lesser => a < b,
            ComparisonOperatorKind.LesserEqual => a <= b,
            _ => throw new InvalidActionException(0, $"{kind} is not a supported equality operator for {ValueType}")
        };
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

        if (uint.TryParse(value, NumberStyles.Integer & NumberStyles.HexNumber, Info, out uint i))
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
    
    public override IEnumerable<TermField> GetStaticFields()
    {
        foreach (TermField staticField in base.GetStaticFields())
        {
            yield return staticField;
        }
        
        yield return new TermField("min", "int", uint.MinValue);
        yield return new TermField("max", "int", uint.MaxValue);
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

    public override MathOperatorKind[] AllowedMathOps => new[]
    {
        MathOperatorKind.Add,
        MathOperatorKind.Subtract,
        MathOperatorKind.Divide,
        MathOperatorKind.Multiply,
        MathOperatorKind.And,
        MathOperatorKind.Or,
        MathOperatorKind.Power,
        MathOperatorKind.Remaining
    };

    public override object ConductMath(MathOperatorKind kind, BaseTerm subject)
    {
        uint a = CastToUint();
        uint b = subject.CastToUint();

        return kind switch
        {
            MathOperatorKind.Add => Convert.ChangeType(a + b, NumberType),
            MathOperatorKind.Subtract => Convert.ChangeType(a - b, NumberType),
            MathOperatorKind.Multiply => Convert.ChangeType(a * b, NumberType),
            MathOperatorKind.Divide => Convert.ChangeType(a / b, NumberType),
            MathOperatorKind.And => a & b,
            MathOperatorKind.Or => a | b,
            MathOperatorKind.Power => a ^ b,
            MathOperatorKind.Remaining => a % b,
            _ => throw new InvalidActionException(0, $"{kind} is not a supported equality operator for {ValueType}")
        };
    }

    #endregion
    
    #region Comparison

    public override bool ConductComparison(ComparisonOperatorKind kind, BaseTerm subject)
    {
        uint a = (uint)Number;
        uint b = subject.CastToUint();

        return kind switch
        {
            ComparisonOperatorKind.Equal => a == b,
            ComparisonOperatorKind.NotEqual => a != b,
            ComparisonOperatorKind.Greater => a > b,
            ComparisonOperatorKind.GreaterEqual => a >= b,
            ComparisonOperatorKind.Lesser => a < b,
            ComparisonOperatorKind.LesserEqual => a <= b,
            _ => throw new InvalidActionException(0, $"{kind} is not a supported equality operator for {ValueType}")
        };
    }

    #endregion
}

public class TermD : NumberTerm
{
    public override string ValueType => "double";
    protected override Type NumberType => typeof(double);

    public override IEnumerable<TermField> GetStaticFields()
    {
        foreach (TermField staticField in base.GetStaticFields())
        {
            yield return staticField;
        }

        yield return new TermField("positive_infinity", "double", double.PositiveInfinity);
        yield return new TermField("negative_infinity", "double", double.NegativeInfinity);
        yield return new TermField("nan", "double", double.NaN);
        yield return new TermField("min", "double", double.MinValue);
        yield return new TermField("max", "double", double.MaxValue);
        yield return new TermField("epsilon", "double", double.Epsilon);
    }

    public override bool Parse(string value)
    {
        value = value.StartsWith("0x") ? value.Replace("0x", "") : value;

        if (double.TryParse(value, NumberStyles.Float, Info, out double i))
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
    
    #region Comparison

    public override bool ConductComparison(ComparisonOperatorKind kind, BaseTerm subject)
    {
        double a = (double)Number;
        double b = subject.CastToDouble();

        return kind switch
        {
            ComparisonOperatorKind.Equal => Math.Abs(a - b) < 0.005,
            ComparisonOperatorKind.NotEqual => Math.Abs(a - b) > 0.005,
            ComparisonOperatorKind.Greater => a > b,
            ComparisonOperatorKind.GreaterEqual => a >= b,
            ComparisonOperatorKind.Lesser => a < b,
            ComparisonOperatorKind.LesserEqual => a <= b,
            _ => throw new InvalidActionException(0, $"{kind} is not a supported equality operator for {ValueType}")
        };
    }

    #endregion
}