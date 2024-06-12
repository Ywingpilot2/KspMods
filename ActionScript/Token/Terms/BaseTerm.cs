using System;
using System.Collections.Generic;
using ActionScript.Exceptions;
using ActionScript.Library;
using ActionScript.Token.Functions;
using ActionScript.Token.Interaction;
using ActionScript.Utils;

namespace ActionScript.Token.Terms;

public enum TermKind
{
    Null = 0,
    Basic = 1,
    Class = 2
}

public abstract class BaseTerm : IToken
{
    #region Meta

    public TermKind Kind { get; set; }
    public int Line { get; set; }
    public TypeLibrary TypeLibrary { get; set; }

    #endregion

    #region Basic info

    public string Name { get; set; }
    public abstract string ValueType { get; }

    #endregion

    #region Functions

    private readonly IEnumerable<IFunction> _functions = new IFunction[]
    {
        new Function("equals", "bool", inputTypes:new []{"term"}, action: terms =>
        {
            object a = terms[0].GetValue();
            object b = terms[1].GetValue();
            return new ReturnValue(a.Equals(b), "bool");
        }),
        new Function("to-string", "string", terms =>
        {
            if (terms[0].CanImplicitCastToStr)
            {
                return new ReturnValue(terms[0].CastToStr(), "string");
            }
            else
            {
                return new ReturnValue(terms[0].GetValue().ToString(), "string");
            }
        })
    };
    
    /// <summary>
    /// This method enumerates over all of the <see cref="IFunction"/>s this Term has.
    /// It is suggested to iterate over the base.GetFunctions as well, that way functions such as Equals are included in the enumeration
    /// </summary>
    /// <returns>An enumerable which provides the terms functions</returns>
    public virtual IEnumerable<IFunction> GetFunctions()
    {
        foreach (IFunction function in _functions)
        {
            yield return function;
        }
    }

    public IFunction GetFunction(string name)
    {
        foreach (IFunction function in GetFunctions())
        {
            if (function.Name == name)
                return function;
        }
        throw new FunctionNotExistException(0, name);
    }

    public bool HasFunction(string name)
    {
        foreach (IFunction function in GetFunctions())
        {
            if (function.Name == name)
                return true;
        }

        return false;
    }

    #endregion

    #region Casting

    public virtual bool CanImplicitCastToStr => false;

    public virtual string CastToStr()
    {
        throw new InvalidTermCastException(Line, ValueType, "string");
    }

    public virtual bool CanImplicitCastToGuid => false;
    public virtual Guid CastToGuid()
    {
        throw new InvalidTermCastException(Line, ValueType, "guid");
    }
    
    public virtual bool CanImplicitCastToUint => false;
    public virtual uint CastToUint()
    {
        throw new InvalidTermCastException(Line, ValueType, "uint");
    }
    
    public virtual bool CanImplicitCastToInt => false;
    public virtual int CastToInt()
    {
        throw new InvalidTermCastException(Line, ValueType, "int");
    }
    
    public virtual bool CanImplicitCastToFloat => false;
    public virtual float CastToFloat()
    {
        throw new InvalidTermCastException(Line, ValueType, "float");
    }
    
    public virtual bool CanImplicitCastToDouble => false;
    public virtual double CastToDouble()
    {
        throw new InvalidTermCastException(Line, ValueType, "double");
    }
    
    public virtual bool CanImplicitCastToBool => false;
    public virtual bool CastToBool()
    {
        throw new InvalidTermCastException(Line, ValueType, "bool");
    }

    public virtual bool CanImplicitCastToType(string name)
    {
        switch (name)
        {
            case "bool":
            {
                return CanImplicitCastToBool;
            }
            case "double":
            {
                return CanImplicitCastToDouble;
            }
            case "float":
            {
                return CanImplicitCastToFloat;
            }
            case "int":
            {
                return CanImplicitCastToInt;
            }
            case "uint":
            {
                return CanImplicitCastToUint;
            }
            case "guid":
            {
                return CanImplicitCastToGuid;
            }
            case "string":
            {
                return CanImplicitCastToStr;
            }
        }
        
        return false;
    }
    
    public virtual object CastToType(string name)
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
        
        return false;
    }

    #endregion

    #region Operators

    public virtual OperatorKind[] AllowedOperators { get; }

    public virtual object ConductOperation(OperatorKind kind, BaseTerm subject)
    {
        throw new NotImplementedException("This term does not support any operators");
    }

    #endregion

    #region Value manipulation

    /// <summary>
    /// Parse the term from a string value
    /// </summary>
    /// <param name="value">The pure value from the token</param>
    /// <returns>A bool indicating whether the operation was a success. If false a <see cref="CompilationException"/> will be thrown</returns>
    public abstract bool Parse(string value);

    public abstract bool SetValue(object value);
    
    /// <summary>
    /// Copy another terms 
    /// </summary>
    /// <param name="term"></param>
    /// <returns></returns>
    public abstract bool CopyFrom(BaseTerm term);

    /// <summary>
    /// Get the raw value of this term
    /// </summary>
    /// <returns>The raw, or C# representation of this terms value</returns>
    public abstract object GetValue();

    #endregion

    public TermType GetTermType()
    {
        return TypeLibrary.GetTermType(ValueType, 0);
    }
}