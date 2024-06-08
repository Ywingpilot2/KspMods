﻿using System;
using System.Globalization;
using ActionScript.Exceptions;

namespace ActionScript.Terms;

public sealed class StringTerm : BaseTerm
{
    public override string ValueType => "string";
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
            value = value.Remove(value.LastIndexOf('"') - 1, 1);
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
        return true;
    }

    public override bool CopyFrom(BaseTerm term)
    {
        if (term is StringTerm stringTerm)
        {
            _value = stringTerm._value;
            return true;
        }

        return false;
    }
}