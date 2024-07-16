using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using SteelLanguage.Exceptions;
using SteelLanguage.Library;
using SteelLanguage.Reflection.Library;
using SteelLanguage.Reflection.Type;
using SteelLanguage.Token.Fields;
using SteelLanguage.Token.Functions;
using SteelLanguage.Token.Functions.Modifier;
using SteelLanguage.Token.Functions.Operator;
using SteelLanguage.Token.Functions.Single;
using SteelLanguage.Token.Interaction;
using SteelLanguage.Utils;

namespace SteelLanguage.Token.Terms;

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
    public LibraryManager TypeLibrary { get; set; }

    #endregion

    #region Basic info

    public string Name { get; set; }
    public abstract string ValueType { get; }
    
    public virtual bool ContainsType { get; }
    public virtual string ContainedType { get; set; }

    #endregion

    #region Fields

    #region Static

    public virtual IEnumerable<TermField> GetStaticFields()
    {
        return new TermField[0];
    }

    public TermField GetStaticField(string name)
    {
        foreach (TermField field in GetStaticFields())
        {
            if (field.Name == name)
                return field;
        }
        
        throw new FieldNotExistException(0, name);
    }
    
    public virtual bool SetStaticField(string name, object value)
    {
        throw new FieldReadOnlyException(0, name);
    }

    #endregion

    #region Local

    public virtual IEnumerable<TermField> GetFields()
    {
        return new TermField[0];
    }

    public TermField GetField(string name)
    {
        foreach (TermField field in GetFields())
        {
            if (field.Name == name)
                return field;
        }
        
        throw new FieldNotExistException(0, name);
    }

    public virtual bool SetField(string name, object value)
    {
        return false;
    }
    
    public bool HasField(string name)
    {
        foreach (TermField field in GetFields())
        {
            if (field.Name == name)
                return true;
        }

        return false;
    }

    #endregion

    #endregion

    #region Functions

    /// <summary>
    /// This method enumerates over all of the <see cref="IFunction"/>s this Term has.
    /// It is suggested to iterate over the base.GetFunctions as well, that way functions such as Equals are included in the enumeration
    /// </summary>
    /// <returns>An enumerable which provides the terms functions</returns>
    [SuppressMessage("ReSharper", "HeapView.BoxingAllocation")]
    public virtual IEnumerable<IFunction> GetFunctions()
    {
        yield return new Function("equals", "bool", inputTypes: new[] { "term" }, action: terms =>
        {
            object a = terms[0].GetValue();
            object b = terms[1].GetValue();
            return new ReturnValue(a.Equals(b), "bool");
        });
        yield return new Function("to_string", "string", terms =>
        {
            if (terms[0].CanImplicitCastToStr)
            {
                return new ReturnValue(terms[0].CastToStr(), "string");
            }
            else
            {
                return new ReturnValue(terms[0].GetValue().ToString(), "string");
            }
        });
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

    #region Construction

    public virtual IEnumerable<TermConstructor> GetConstructors()
    {
        yield return new TermConstructor(_ => new ReturnValue(GetValue(), ValueType));
    }

    public TermConstructor GetConstructor(string sig)
    {
        foreach (TermConstructor constructor in GetConstructors())
        {
            if (constructor.GetSig() == sig)
                return constructor;
        }

        throw new ConstructorNotFoundException(0, sig);
    }

    public bool HasConstructor(string sig)
    {
        foreach (TermConstructor constructor in GetConstructors())
        {
            if (constructor.GetSig() == sig)
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

    public virtual bool CanImplicitCastToType(TermType type)
    {
        switch (type.Name)
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

    #region Math

    public virtual MathOperatorKind[] AllowedMathOps => new MathOperatorKind[0];

    public virtual object ConductMath(MathOperatorKind kind, BaseTerm subject)
    {
        throw new NotImplementedException($"{ValueType} does not support any operators");
    }

    #endregion
    
    #region Comparison

    public virtual ComparisonOperatorKind[] AllowedComparisons => new[]
    {
        ComparisonOperatorKind.Equal,
        ComparisonOperatorKind.NotEqual
    };

    public virtual bool ConductComparison(ComparisonOperatorKind kind, BaseTerm subject)
    {
        if (kind == ComparisonOperatorKind.Equal)
        {
            if (subject.GetValue() == null && GetValue() == null)
                return true;

            if (GetValue() == null)
                return subject.GetValue() == null;

            return GetValue().Equals(subject.GetValue());
        }

        if (subject.GetValue() == null && GetValue() == null)
            return false;

        if (GetValue() == null)
            return subject.GetValue() != null;

        return GetValue().Equals(subject.GetValue());
    }

    #endregion
    
    #region Bool

    public virtual BoolOperatorKind[] AllowedBoolOps => new BoolOperatorKind[0];

    public virtual bool ConductBoolOp(BoolOperatorKind kind, BaseTerm subject)
    {
        throw new NotImplementedException($"{ValueType} term does not support any operators");
    }

    #endregion

    #endregion

    #region Value manipulation

    /// <summary>
    /// Parse the term from a string value. This method is hard coded to basic types. 
    /// </summary>
    /// <param name="value">The pure value from the token</param>
    /// <returns>A bool indicating whether the operation was a success. If false a <see cref="CompilationException"/> will be thrown</returns>
    public virtual bool Parse(string value)
    {
        throw new InvalidActionException(0, $"{ValueType} can't parse {value} as a literal");
    }

    /// <summary>
    /// Set this terms raw value
    /// </summary>
    /// <param name="value">The value to set</param>
    /// <returns>A bool indicating whether the operation was a success. If false a <see cref="CompilationException"/> will be thrown</returns>
    public abstract bool SetValue(object value);
    
    /// <summary>
    /// Copy another term's value
    /// </summary>
    /// <param name="term">The term to copy the value from</param>
    /// <returns>A bool indicating whether the operation was a success. If false a <see cref="CompilationException"/> will be thrown</returns>
    public abstract bool CopyFrom(BaseTerm term);

    /// <summary>
    /// Get the raw value of this term
    /// </summary>
    /// <returns>The raw, or C# representation of this terms value</returns>
    public abstract object GetValue();

    #endregion

    /// <summary>
    /// Gets the <see cref="TermType"/> representing this term
    /// </summary>
    /// <returns>A copy of the <see cref="TermType"/> gotten from <see cref="LibraryManager"/> used to construct this <see cref="BaseTerm"/></returns>
    public TermType GetTermType()
    {
        TermType example = TypeLibrary.GetTermType(ValueType);
        TermType type = new TermType(this, example.BaseClass, example.IsAbstract, example.IsNullable);
        if (ContainsType)
            type.ContainedType = ContainedType;
        
        return type;
    }
}