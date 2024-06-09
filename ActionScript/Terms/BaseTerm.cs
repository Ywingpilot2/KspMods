using System;
using System.Collections.Generic;
using System.Globalization;
using ActionScript.Exceptions;
using ActionScript.Functions;
using ActionScript.Library;
using ActionScript.Token;

namespace ActionScript.Terms;

public enum TermKind
{
    Null = 0,
    Basic = 1,
    Class = 2
}

public abstract class BaseTerm : IToken
{
    #region Meta

    public TermKind Kind { get; protected set; }
    public int Line { get; set; }
    public TypeLibrary TypeLibrary { get; set; }

    #endregion

    #region Basic info

    public string Name { get; set; }
    public abstract string ValueType { get; }

    #endregion

    #region Functions

    /// <summary>
    /// An array of all the <see cref="Function"/>s this <see cref="Term"/> has.
    /// The first <see cref="Term"/> input is always the term this local function belongs to.
    /// </summary>
    public abstract IEnumerable<IFunction> Functions { get; }

    public IFunction GetFunction(string name)
    {
        foreach (IFunction function in Functions)
        {
            if (function.Name == name)
                return function;
        }
        throw new FunctionNotExistException(0, name);
    }

    public bool HasFunction(string name)
    {
        foreach (IFunction function in Functions)
        {
            if (function.Name == name)
                return true;
        }

        return false;
    }

    #endregion

    #region Casting

    public virtual string CastToStr()
    {
        throw new InvalidTermCastException(Line, ValueType, "string");
    }

    public virtual Guid CastToGuid()
    {
        throw new InvalidTermCastException(Line, ValueType, "guid");
    }
    
    public virtual uint CastToUint()
    {
        throw new InvalidTermCastException(Line, ValueType, "uint");
    }
    
    public virtual int CastToInt()
    {
        throw new InvalidTermCastException(Line, ValueType, "int");
    }
    
    public virtual float CastToFloat()
    {
        throw new InvalidTermCastException(Line, ValueType, "float");
    }
    
    public virtual double CastToDouble()
    {
        throw new InvalidTermCastException(Line, ValueType, "double");
    }
    
    public virtual bool CastToBool()
    {
        throw new InvalidTermCastException(Line, ValueType, "bool");
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