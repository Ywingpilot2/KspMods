using System;
using System.Collections.Generic;
using System.Globalization;
using SteelLanguage.Exceptions;
using SteelLanguage.Token.Fields;
using SteelLanguage.Token.Functions;
using SteelLanguage.Token.Interaction;
using SteelLanguage.Token.Terms.Literal;

namespace SteelLanguage.Token.Terms.Complex;

public sealed class StringTerm : BaseTerm
{
    public override string ValueType => "string";

    #region Fields

    public override IEnumerable<TermField> GetFields()
    {
        foreach (TermField field in base.GetFields())
        {
            yield return field;
        }

        yield return new TermField("length", "int", _value.Length);
    }

    #endregion

    #region Functions
    
    public override IEnumerable<IFunction> GetFunctions()
    {
        foreach (IFunction function in base.GetFunctions()) yield return function;

        yield return new Function("upper", "string", _ => new ReturnValue(_value.ToUpper(), "string"));
        yield return new Function("lower", "string", terms => new ReturnValue(_value.ToLower(), "string"));
        yield return new Function("remove", "string", inputTypes: "string", action: terms =>
        {
            BaseTerm remove = terms[0];

            string str = _value;
            string rem = remove.CastToStr();
            if (str.IndexOf(rem, StringComparison.Ordinal) == -1)
                return new ReturnValue(str, "string");

            return new ReturnValue(str.Remove(str.IndexOf(rem, StringComparison.Ordinal), rem.Length), "string");
        });
        yield return new Function("replace", "string", inputTypes: new[] { "string", "string" },
            action: terms => new ReturnValue(_value.Replace(terms[0].CastToStr(), terms[1].CastToStr()),
                "string"));
    }

    #endregion

    #region Casting

    public override string CastToStr()
    {
        return _value;
    }

    public override Guid CastToGuid()
    {
        if (Guid.TryParse(_value, out Guid guid))
        {
            return guid;
        }
        else
        {
            throw new InvalidTermCastException(0);
        }
    }

    public override int CastToInt()
    {
        if (int.TryParse(_value, NumberStyles.Integer, new NumberFormatInfo(), out int i))
        {
            return i;
        }
        else
        {
            throw new InvalidTermCastException(0);
        }
    }

    public override uint CastToUint()
    {
        if (uint.TryParse(_value, NumberStyles.Integer, new NumberFormatInfo(), out uint i))
        {
            return i;
        }
        else
        {
            throw new InvalidTermCastException(0);
        }
    }

    public override float CastToFloat()
    {
        // We check to see if this value has a decimal point before conducting conversion
        // We do this to maintain accuracy to the original number, without this we may get values like 2.0 when in reality we have 2 thousand
        if (_value.Contains("."))
        {
            if (!float.TryParse(_value, NumberStyles.Float, new NumberFormatInfo(), out float i))
            {
                return 0;
            }
            else
            {
                return i;
            }
        }
        else
        {
            return CastToInt();
        }
    }
    
    public override double CastToDouble()
    {
        // We check to see if this value has a decimal point before conducting conversion
        // We do this to maintain accuracy to the original number, without this we may get values like 2.0 when in reality we have 2 thousand
        if (_value.Contains("."))
        {
            if (!double.TryParse(_value, NumberStyles.Float, new NumberFormatInfo(), out double i))
            {
                return 0;
            }
            else
            {
                return i;
            }
        }
        else
        {
            return CastToInt();
        }
    }

    public override bool CastToBool()
    {
        if (bool.TryParse(_value.ToLower(), out bool b))
        {
            return b;
        }
        else if (int.TryParse(_value, NumberStyles.Integer, new NumberFormatInfo(), out int i))
        {
            if (i >= 1)
            {
                return true;
            }
        }
            
        return false;
    }

    #endregion

    #region Value

    // to prevent issues with fields & compilation we set a default value
    private string _value = "";
    
    public override bool Parse(string value)
    {
        if (value.StartsWith("\""))
        {
            value = value.Remove(0, 1);
        }
        else
        {
            return false;
        }

        if (value.EndsWith("\""))
        {
            value = value.Remove(value.LastIndexOf('"'), 1);
        }
        else
        {
            return false;
        }

        // json escape procedure
        string escaped = "";
        bool isEsc = false;
        if (value.Length > 1)
        {
            for (int i = 1; i < value.Length; i++)
            {
                char p = value[i - 1];
                char c = value[i];
                if (isEsc)
                {
                    isEsc = false;
                    continue;
                }

                if (p == '\\')
                {
                    isEsc = true;
                    switch (c)
                    {
                        case '\\':
                        {
                            escaped += '\\';
                            continue;
                        }
                        case 't':
                        {
                            escaped += '\t';
                            continue;
                        }
                        case 'n':
                        {
                            escaped += "\n";
                            continue;
                        }
                    }
                }

                escaped += p;
                if (i + 1 >= value.Length)
                {
                    escaped += c;
                }
            }
        }
        else if(value.Length != 0)
        {
            escaped = value;
        }

        _value = escaped;
        
        Kind = TermKind.Basic;
        return true;
    }

    public override bool SetValue(object value)
    {
        _value = value.ToString();
        Kind = TermKind.Basic;
        return true;
    }
    
    public override object GetValue()
    {
        return _value;
    }

    public override bool CopyFrom(BaseTerm term)
    {
        if (term is StringTerm stringTerm)
        {
            _value = stringTerm._value;
            return true;
        }
        else if (term is NullTerm)
        {
            _value = null;
            Kind = TermKind.Null;
            return true;
        }

        return false;
    }

    #endregion
}