using System;
using System.Collections.Generic;
using System.Globalization;
using ActionScript.Functions;
using ActionScript.Terms;

namespace ActionScript.Library
{
    public class BaseActionLibrary : ILibrary
    {
        public IEnumerable<Function> Functions => new[]
        {
            #region basic math

            new Function("add", terms =>
            {
                if (terms.Length != 2)
                    return new ReturnValue(0);

                Term a = terms[0];
                Term b = terms[1];
                switch (a.ValueType)
                {
                    case "uint":
                    {
                        return new ReturnValue(a.GetAsUint() + b.GetAsUint());
                    }
                    case "int":
                    {
                        return new ReturnValue(a.GetAsInt() + b.GetAsInt());
                    }
                    default: // asume floats as to avoid loss of precision
                    {
                        return new ReturnValue(a.GetAsFloat() + b.GetAsFloat());
                    }
                }
            }),
            new Function("subtract", terms =>
            {
                if (terms.Length != 2)
                    return new ReturnValue(0);

                Term a = terms[0];
                Term b = terms[1];
                switch (a.ValueType)
                {
                    case "uint":
                    {
                        return new ReturnValue(a.GetAsUint() - b.GetAsUint());
                    }
                    case "int":
                    {
                        return new ReturnValue(a.GetAsInt() - b.GetAsInt());
                    }
                    default: // asume floats as to avoid loss of precision
                    {
                        return new ReturnValue(a.GetAsFloat() - b.GetAsFloat());
                    }
                }
            }),
            new Function("multiply", terms =>
            {
                if (terms.Length != 2)
                    return new ReturnValue(0);

                Term a = terms[0];
                Term b = terms[1];
                switch (a.ValueType)
                {
                    case "uint":
                    {
                        return new ReturnValue(a.GetAsUint() * b.GetAsUint());
                    }
                    case "int":
                    {
                        return new ReturnValue(a.GetAsInt() * b.GetAsInt());
                    }
                    default: // asume floats as to avoid loss of precision
                    {
                        return new ReturnValue(a.GetAsFloat() * b.GetAsFloat());
                    }
                }
            }),
            new Function("divide", terms =>
            {
                if (terms.Length != 2)
                    return new ReturnValue(0);

                Term a = terms[0];
                Term b = terms[1];
                switch (a.ValueType)
                {
                    case "uint":
                    {
                        return new ReturnValue(a.GetAsUint() / b.GetAsUint());
                    }
                    case "int":
                    {
                        return new ReturnValue(a.GetAsInt() / b.GetAsInt());
                    }
                    default: // asume floats as to avoid loss of precision
                    {
                        return new ReturnValue(a.GetAsFloat() / b.GetAsFloat());
                    }
                }
            }),

            #endregion

            #region Random

            new Function("get-random", valueType:"int", action: terms =>
            {
                Random random = new Random();
                switch (terms.Length)
                {
                    case 2:
                    {
                        return new ReturnValue(random.Next(terms[0].GetAsInt(), terms[1].GetAsInt()));
                    } break;
                    case 1:
                    {
                        return new ReturnValue(random.Next(terms[0].GetAsInt()));
                    } break;
                    default:
                    {
                        return new ReturnValue(random.Next());
                    } break;
                }
            }),

            #endregion

            #region conversion

            new Function("to-float", valueType:"float", action: terms =>
            {
                Term a = terms[0];
                switch (a.ValueType)
                {
                    case "string":
                    {
                        bool attempt = float.TryParse(a.GetAsStr(), out float f);
                        if (!attempt)
                        {
                            return new ReturnValue((float)a.GetAsInt());
                        }

                        return new ReturnValue(f);
                    } break;
                    default:
                    {
                        return new ReturnValue((float)a.GetAsInt());
                    } break;
                }
            }),
            
            new Function("to-int", valueType:"int", action: terms =>
            {
                Term a = terms[0];
                switch (a.ValueType)
                {
                    case "string":
                    {
                        bool attempt = float.TryParse(a.GetAsStr(), out float f);
                        if (!attempt)
                        {
                            return new ReturnValue((float)a.GetAsInt());
                        }

                        return new ReturnValue(f);
                    } break;
                    default:
                    {
                        return new ReturnValue(a.GetAsInt());
                    } break;
                }
            }),
            
            new Function("to-uint", valueType:"uint", action: terms =>
            {
                Term a = terms[0];
                switch (a.ValueType)
                {
                    case "string":
                    {
                        bool attempt = float.TryParse(a.GetAsStr(), out float f);
                        if (!attempt)
                        {
                            return new ReturnValue((float)a.GetAsInt());
                        }

                        return new ReturnValue(f);
                    } break;
                    default:
                    {
                        return new ReturnValue((uint)(Math.Abs(a.GetAsInt())));
                    } break;
                }
            }),

            #endregion
        };

        public IEnumerable<Term> GlobalTerms => new[]
        {
            new Term("int-max", int.MaxValue.ToString(), "int"),
            new Term("uint-max", uint.MaxValue.ToString(), "uint"),
            new Term("float-max", float.MaxValue.ToString(CultureInfo.InvariantCulture), "float"),
        };
    }
}