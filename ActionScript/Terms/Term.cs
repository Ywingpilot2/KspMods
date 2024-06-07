using System;
using System.Globalization;
using ActionScript.Token;

namespace ActionScript.Terms
{
    public enum TermType
    {
        Null = 0,
        Basic = 1,
        Class = 2
    }
    
    public class Term : IToken
    {
        public string Name { get; }
        public TermType Type { get; private set; }
        public string ValueType { get; set; } // We are allowed to set the value type externally due to dynamic typing
        private string _value;
        
        public int Line { get; set; }
        
        #region Getting as

        public string GetAsStr()
        {
            return _value;
        }

        public Guid GetAsGuid()
        {
            if (Guid.TryParse(_value, out Guid guid))
            {
                return guid;
            }
            else
            {
                return Guid.Empty;
            }
        }

        public uint GetAsUint()
        {
            string value = _value.StartsWith("0x") ? _value.Replace("0x", "") : _value;
            
            if (!uint.TryParse(value, NumberStyles.Integer & NumberStyles.HexNumber, new NumberFormatInfo(), out uint i))
            {
                return 0;
            }
            else
            {
                return i;
            }
        }
        
        public int GetAsInt()
        {
            string value = _value.StartsWith("0x") ? _value.Replace("0x", "") : _value;
            
            if (!int.TryParse(value, NumberStyles.Integer & NumberStyles.HexNumber, new NumberFormatInfo(), out int i))
            {
                return 0;
            }
            else
            {
                return i;
            }
        }
        
        public float GetAsFloat()
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
                return GetAsInt();
            }
        }
        
        public double GetAsDouble()
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
                return GetAsInt();
            }
        }
        
        public bool GetAsBool()
        {
            if (bool.TryParse(_value.ToLower(), out bool b))
            {
                return b;
            }
            else
            {
                if (GetAsInt() >= 1)
                {
                    return true;
                }
            }
            
            return false;
        }

        #endregion

        /// <summary>
        /// This gets the value depending on what the <see cref="ValueType"/> is
        /// </summary>
        /// <returns></returns>
        public object GetAsType()
        {
            switch (ValueType)
            {
                case "int":
                {
                    return GetAsInt();
                }
                case "uint":
                {
                    return GetAsUint();
                }
                case "float":
                {
                    return GetAsFloat();
                } 
                case "double":
                {
                    return GetAsDouble();
                } 
                case "bool":
                {
                    return GetAsInt();
                } 
                case "guid":
                {
                    return GetAsGuid();
                } 
                default:
                {
                    return GetAsStr();
                } 
            }
        }

        public void CopyFrom(Term term)
        {
            _value = term._value;
            Type = term.Type;
        }

        public Term(string name, string value = null, string valueType = null)
        {
            Name = name;
            _value = value;
            
            Type = value != null ? TermType.Basic : TermType.Null;
            ValueType = valueType;
        }
        
        public Term(string value)
        {
            Name = Guid.NewGuid().ToString();
            _value = value;
            Type = TermType.Basic;
        }
    }
}