﻿using System;
using System.Collections.Generic;
using System.Globalization;
using ActionScript.Exceptions;
using ActionScript.Token.Functions;
using ActionScript.Token.Interaction;

namespace ActionScript.Token.Terms;

public sealed class StringTerm : BaseTerm
{
    public override string ValueType => "string";

    private readonly IEnumerable<IFunction> _functions = new IFunction[]
    {
        new Function("upper", "string", terms => new ReturnValue(terms[0].CastToStr().ToUpper(), "string")),
        new Function("lower", "string", terms => new ReturnValue(terms[0].CastToStr().ToLower(), "string")),
        new Function("remove", "string", inputTypes: "string", action: terms =>
        {
            BaseTerm ths = terms[0];
            BaseTerm remove = terms[1];

            string str = ths.CastToStr();
            string rem = remove.CastToStr();
            if (str.IndexOf(rem, StringComparison.Ordinal) == -1)
                return new ReturnValue(str, "string");

            return new ReturnValue(str.Remove(str.IndexOf(rem, StringComparison.Ordinal), rem.Length), "string");
        }),
        new Function("replace", "string", inputTypes: new[] { "string", "string" },
            action: terms =>
            {
                return new ReturnValue(terms[0].CastToStr().Replace(terms[1].CastToStr(), terms[2].CastToStr()),
                    "string");
            })
    };
    public override IEnumerable<IFunction> GetFunctions()
    {
        foreach (IFunction function in base.GetFunctions()) yield return function;
        
        foreach (IFunction function in _functions)
        {
            yield return function;
        }
    }

    private string _value;

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
            throw new InvalidTermCastException(Line); // TODO: Get the current executing line
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
            throw new InvalidTermCastException(Line); // TODO: Get the current executing line
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
            throw new InvalidTermCastException(Line); // TODO: Get the current executing line
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
}