using System;
using System.Globalization;
using ActionScript.Exceptions;
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
    public int Line { get; set; }
    public string Name { get; set; }
    public TypeLibrary TypeLibrary { get; set; }

    public abstract string ValueType { get; }
    public TermKind Kind { get; protected set; }

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

    public TermType GetTermType()
    {
        return TypeLibrary.GetTermType(ValueType, 0);
    }
}