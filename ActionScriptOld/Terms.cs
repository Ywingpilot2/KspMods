using System;
using System.Globalization;

namespace ActionScript
{
    public enum TermType
    {
        Null = 0,
        Basic = 1,
        Class = 2
    }
    
    public struct Term
    {
        public string Name { get; }
        public TermType Type { get; }
        private string Value { get; set; }
        
        #region Getting as

        public string GetAsStr()
        {
            return Value.ToString();
        }

        public Guid GetAsGuid()
        {
            if (Guid.TryParse((string)Value, out Guid guid))
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
            string value = ((string)Value).StartsWith("0x") ? ((string)Value).Replace("0x", "") : ((string)Value);
            
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
            string value = ((string)Value).StartsWith("0x") ? ((string)Value).Replace("0x", "") : ((string)Value);
            
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
            if (!float.TryParse(((string)Value), NumberStyles.Float, new NumberFormatInfo(), out float i))
            {
                return 0;
            }
            else
            {
                return i;
            }
        }
        
        public double GetAsDouble()
        {
            if (!double.TryParse(((string)Value), NumberStyles.Float, new NumberFormatInfo(), out double i))
            {
                return 0;
            }
            else
            {
                return i;
            }
        }
        
        public bool GetAsBool()
        {
            if (bool.TryParse(((string)Value).ToLower(), out bool b))
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

        public void CopyFrom(Term term)
        {
            Value = term.Value;
        }

        public Term(string name, string value)
        {
            Name = name;
            Value = value;
            Type = TermType.Basic;
        }
    }
}