﻿using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using SteelLanguage.Exceptions;
using SteelLanguage.Library;
using SteelLanguage.Library.System.Terms.Complex;
using SteelLanguage.Library.System.Terms.Literal;
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

/// <summary>
/// The base class used for all terms in Steel.
/// All terms inherit from this, and everything from the <see cref="Reflection"/> system to <see cref="TokenCall"/>s revolve around storing values here.
/// <remarks>
/// <list>
/// <listheader>
/// <term>Life Cycle</term>
/// <description>The stages at which terms are created and used</description>
/// </listheader>
/// 
/// <item>
/// <term>Token </term>
/// <description>an unparsed, unhandled token. This is the literal code written by the programmer.</description>
/// </item>
///
/// <item>
/// <term>Pre-Compilation </term>
/// <description>The <see cref="TermType"/> is constructed with the root term found in the <see cref="ILibrary"/>, this root term initializes and constructs all other terms</description>
/// </item>
/// 
/// <item>
/// <term>Initialization </term>
/// <description>A token is parsed into a term by the compiler, and a term is created from the <see cref="TermType"/>. Initialized terms don't have a value until constructed.</description>
/// </item>
///
/// <item>
/// <term>Construction </term>
/// <description>Construction is the process of giving an initialized term a value. It occurs during runtime when a <see cref="AssignmentCall"/> assigns a term a value with <see cref="CopyFrom"/></description>
/// </item>
///
/// </list>
/// </remarks>
///
/// <seealso cref="TermType"/>
/// <seealso cref="TermConstructor"/>
/// <seealso cref="TermField"/>
/// </summary>
public abstract class BaseTerm : IToken
{
    #region Reflection

    /// <summary>
    /// The kind of term this is, borderline obsolete and not actively maintained.
    /// </summary>
    /// <seealso cref="TermType"/>
    public TermKind Kind { get; set; }
    
    /// <inheritdoc />
    public int Line { get; set; }
    
    /// <summary>
    /// The <see cref="LibraryManager"/> this term is under. Use this to get Reflection info.
    /// </summary>
    /// <seealso cref="TermType"/>
    public LibraryManager TypeLibrary { get; set; }
    
    /// <summary>
    /// Gets the <see cref="TermType"/> representing this term
    /// </summary>
    /// <returns>A copy of the <see cref="TermType"/> gotten from <see cref="LibraryManager"/> used to construct this <see cref="BaseTerm"/></returns>
    /// <seealso cref="TermType"/>
    /// <seealso cref="LibraryManager"/>
    public TermType GetTermType()
    {
        TermType example = TypeLibrary.GetTermType(ValueType);
        TermType type = new TermType(this, example.BaseClass, example.IsAbstract, example.IsNullable);
        if (ContainsType)
            type.ContainedType = ContainedType;
        
        return type;
    }

    #endregion

    #region Basic info

    /// <summary>
    /// The name of this term. Null if this term wasn't generated by the compiler.
    /// </summary>
    public string Name { get; set; }
    
    /// <summary>
    /// The name of the <see cref="TermType"/>, this will be used by the <see cref="Reflection"/> system to identify this term's type.
    /// </summary>
    /// <seealso cref="ReturnValue"/>
    /// <seealso cref="TermType"/>
    public abstract string ValueType { get; }

    /// <summary>
    /// Whether or not this contains a value.
    /// If true, syntax for declaring this type will be `type&lt;<see cref="ContainedType"/>&gt; name`
    /// If false, syntax for declaring this type will be `type name`
    /// </summary>
    /// <seealso cref="TermType"/>
    public virtual bool ContainsType => false;
    
    /// <summary>
    /// The names of the valid types for <see cref="ContainsType"/>
    /// </summary>
    public virtual string[] ContainedTypeInputs { get; }

    /// <summary>
    /// If <see cref="ContainsType"/> is true, the name of the type this term contains. Otherwise null
    /// </summary>
    /// <seealso cref="TermType"/>
    public virtual string[] ContainedType { get; set; } = new string[0];

    #endregion

    #region Fields

    #region Static

    /// <summary>
    /// Returns an <see cref="IEnumerable{T}"/> containing all of the globally available <see cref="TermField"/>s for this type.
    /// </summary>
    /// <remarks>
    /// <list type="bullet">
    /// <listheader>
    /// <description>For derived classes</description>
    /// </listheader>
    /// 
    /// <item>
    /// <description>These values are requested at runtime through the <see cref="Reflection"/> system, specifically the <see cref="TermType"/>.</description>
    /// </item>
    ///
    /// <item>
    /// <description>These values are requested at compile-time, meaning the term may not be initialized or constructed.</description>
    /// </item>
    /// 
    /// <item>
    /// <term>For users: </term>
    /// <description>Use <see cref="TermType"/>.StaticFields instead to access Static <see cref="TermField"/>s</description>
    /// </item>
    /// </list>
    /// </remarks>
    /// <returns>When implemented in a derived class, an enumerable of all the static <see cref="TermField"/>s in this term</returns>
    /// <seealso cref="TermField"/>
    public virtual IEnumerable<TermField> GetStaticFields()
    {
        return new TermField[0];
    } 

    /// <summary>
    /// This retrieves a static value.
    /// </summary>
    /// <remarks>Do not call this directly, instead get the <see cref="TermType"/> from the current <see cref="LibraryManager"/> and call <see cref="TermType"/>.GetStaticField()</remarks>
    /// <param name="name">The name of the <see cref="TermField"/> to find</param>
    /// <returns>The <see cref="TermField"/> with the specified name</returns>
    /// <exception cref="FieldNotExistException">Occurs if this <see cref="TermType"/> lacks a field of this name</exception>
    /// <seealso cref="TermField"/>
    public TermField GetStaticField(string name)
    {
        foreach (TermField field in GetStaticFields())
        {
            if (field.Name == name)
                return field;
        }
        
        throw new FieldNotExistException(0, name);
    }

    /// <summary>
    /// Enumerates over all Static <see cref="TermField"/>s and returns a boolean indicating if any of them match the requested name
    /// </summary>
    /// <param name="name">The name of the <see cref="TermField"/> to search for</param>
    /// <returns>A boolean indicating whether a <see cref="TermField"/> of that name exists</returns>
    /// <seealso cref="TermField"/>
    public bool HasStaticField(string name)
    {
        foreach (TermField field in GetStaticFields())
        {
            if (field.Name == name)
                return true;
        }
        
        return false;
    }

    /// <summary>
    /// Sets a field of the specified name
    /// </summary>
    /// <param name="name">The name of the field to set</param>
    /// <param name="value">The value to set the field to</param>
    /// <exception cref="FieldNotExistException">Occurs if the field does not exist and this method has not been implemented in a derived class</exception>
    /// <exception cref="FieldReadOnlyException">Occurs if the field exists, but nothing has been implemented to set it in a derived class</exception>
    /// <returns>A boolean indicating whether the operation was a success</returns>
    /// <seealso cref="TermField"/>
    public virtual bool SetStaticField(string name, object value)
    {
        if (!HasStaticField(name))
            throw new FieldNotExistException(0, name);
        
        throw new FieldReadOnlyException(0, name);
    }

    #endregion

    #region Local

    /// <summary>
    /// Returns an <see cref="IEnumerable{T}"/> containing all of the available <see cref="TermField"/>s for this term.
    /// </summary>
    /// <remarks>
    /// <list type="bullet">
    /// <listheader>
    /// <description>For derived classes</description>
    /// </listheader>
    /// 
    /// <item>
    /// <description>These values are requested at runtime through the <see cref="Reflection"/> system, specifically the <see cref="TermType"/>.</description>
    /// </item>
    ///
    /// <item>
    /// <description>These values are requested at compile-time, meaning the term may not be initialized or constructed.</description>
    /// </item>
    /// 
    /// </list>
    /// </remarks>
    /// <returns>When implemented in a derived class, an enumerable of all the <see cref="TermField"/>s a term contains</returns>
    /// <seealso cref="TermField"/>
    public virtual IEnumerable<TermField> GetFields()
    {
        return new TermField[0];
    }

    /// <summary>
    /// Gets a <see cref="TermField"/> of the specified name
    /// </summary>
    /// <param name="name">The name of the <see cref="TermField"/> to get</param>
    /// <returns>A <see cref="TermField"/> with a matching name</returns>
    /// <exception cref="FieldNotExistException">Occurs if no <see cref="TermField"/>s exist of that name</exception>
    /// <seealso cref="TermField"/>
    public TermField GetField(string name)
    {
        foreach (TermField field in GetFields())
        {
            if (field.Name == name)
                return field;
        }
        
        throw new FieldNotExistException(0, name);
    }

    /// <summary>
    /// Sets a field of the specified name
    /// </summary>
    /// <param name="name">The name of the field to set</param>
    /// <param name="value">The value to set the field to</param>
    /// <returns>A boolean indicating whether the operation was a success</returns>
    /// <seealso cref="TermField"/>
    public virtual bool SetField(string name, object value)
    {
        return false;
    }
    
    /// <summary>
    /// Whether or not this term has a field of the specified name
    /// </summary>
    /// <param name="name">The name of the field to find</param>
    /// <returns>A boolean indicating whether the operation was a success</returns>
    /// <seealso cref="TermField"/>
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
    /// This method fetches all of the <see cref="IFunction"/>s this term has.
    /// </summary>
    /// <remarks>
    /// <list type="bullet">
    /// <item>
    /// <description>For derived classes, it is suggested to iterate over <c>base.GetFunctions()</c> as well, that way functions such as Equals are included in the enumeration</description>
    /// </item>
    ///
    /// <item>
    /// <description>These values are requested at compile-time, meaning the term may not be initialized or constructed.</description>
    /// </item>
    ///
    /// <item>
    /// <description>To avoid memory usage buildup, it is suggested to use the yield return keywords instead of returning an enumerable directly. This will also give your functions proper access to the term</description>
    /// </item>
    /// 
    /// </list>
    /// </remarks>
    /// <example>
    /// <code>
    /// // example of returning the base types functions, as to include functions such as Equals
    /// foreach (IFunction func in base.GetFunctions())
    /// {
    ///     yield return func;
    /// }
    /// </code>
    /// </example>
    /// <example>
    /// <code>
    /// // example of returning your own functions
    /// yield return new Function("increment", "int", () => new ReturnValue(CastToInt() + 1, "int"));
    /// </code>
    /// </example>
    /// <returns>An enumerable which provides the terms functions</returns>
    /// <seealso cref="Function"/>
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

    /// <summary>
    /// Gets a <see cref="IFunction"/> of the specified name
    /// </summary>
    /// <param name="name">The name of the function to find</param>
    /// <returns>A <see cref="IFunction"/> of a matching name</returns>
    /// <exception cref="FunctionNotExistException">Occurs when no functions of that name exists</exception>
    /// <remarks>This will enumerate over all functions until a function with a matching name is found, using the method <see cref="GetFunctions"/></remarks>
    /// <seealso cref="Function"/>
    public IFunction GetFunction(string name)
    {
        foreach (IFunction function in GetFunctions())
        {
            if (function.Name == name)
                return function;
        }
        throw new FunctionNotExistException(0, name);
    }

    /// <summary>
    /// Whether or not this term has a <see cref="IFunction"/> of the specified name
    /// </summary>
    /// <param name="name">THe name of the function to find</param>
    /// <returns>A boolean indicating whether any functions match the specified name</returns>
    /// <remarks>This will enumerate over all functions until a function with a matching name is found, using the method <see cref="GetFunctions"/></remarks>
    /// <seealso cref="Function"/>
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

    /// <summary>
    /// This method fetches all of the <see cref="TermConstructor"/>s this term has. 
    /// </summary>
    /// <remarks>
    /// <list type="bullet">
    ///
    /// <item>
    /// <description>These values are requested at compile-time, meaning the term may not be initialized or constructed.</description>
    /// </item>
    ///
    /// <item>
    /// <description>To avoid memory usage buildup, it is suggested to use the yield return keywords instead of returning an enumerable directly.</description>
    /// </item>
    /// 
    /// <item>
    /// <description>These values are static, meaning they will only ever be requested by the <see cref="TermType"/> through the <see cref="Reflection"/> system. As a result, the <see cref="TermConstructor"/>s should not use anything in the term directly, only the provided inputs.</description>
    /// </item>
    ///
    /// <item>
    /// <description>Construction works by calling one of these functions with a matching signature to create a <see cref="ReturnValue"/> of this <see cref="ValueType"/>, then it will set a terms value to said return value.</description>
    /// </item>
    /// 
    /// </list>
    /// </remarks>
    /// <returns>An enumerable which provides the terms constructors</returns>
    /// <seealso cref="ReturnValue"/>
    /// <seealso cref="TermConstructor"/>
    public virtual IEnumerable<TermConstructor> GetConstructors()
    {
        yield return new TermConstructor(_ => new ReturnValue(GetValue(), ValueType));
    }

    /// <summary>
    /// Gets a <see cref="TermConstructor"/> with a matching signature
    /// </summary>
    /// <param name="sig">The constructor signature to find</param>
    /// <returns>A <see cref="TermConstructor"/> with a matching signature</returns>
    /// <exception cref="ConstructorNotFoundException">Occurs if no constructors with matching signatures are found</exception>
    /// <remarks>This uses <see cref="GetConstructors"/> to enumerate over the <see cref="TermConstructor"/>s this has until a matching signature is found</remarks>
    public TermConstructor GetConstructor(string sig)
    {
        foreach (TermConstructor constructor in GetConstructors())
        {
            if (constructor.GetSig() == sig)
                return constructor;
        }

        throw new ConstructorNotFoundException(0, sig);
    }

    /// <summary>
    /// Whether or not this term has a <see cref="TermConstructor"/> with a matching signature
    /// </summary>
    /// <param name="sig">The constructor signature to find</param>
    /// <returns>A boolean indicating whether or not any <see cref="TermConstructor"/>s in this term have a matching signature</returns>
    /// <remarks>This uses <see cref="GetConstructors"/> to enumerate over the <see cref="TermConstructor"/>s this has until a matching signature is found</remarks>
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

    /// <summary>
    /// Whether or not this can implicitly cast to <see cref="StringTerm"/>.
    /// In the process of being removed, please move functionality to <see cref="CanImplicitCastToType"/>
    /// </summary>
    [Obsolete]
    public virtual bool CanImplicitCastToStr => false;

    /// <summary>
    /// Programmer utility, casts this terms value to <see cref="StringTerm"/>
    /// </summary>
    /// <returns>This terms value casted to a string</returns>
    public virtual string CastToStr()
    {
        return (string)CastToType("string");
    }

    /// <summary>
    /// In the process of being removed, please move functionality to <see cref="CanImplicitCastToType"/>
    /// </summary>
    [Obsolete]
    public virtual bool CanImplicitCastToGuid => false;
    
    /// <summary>
    /// In the process of being removed, please move functionality to <see cref="CastToType"/>
    /// </summary>
    [Obsolete]
    public virtual Guid CastToGuid()
    {
        throw new InvalidTermCastException(Line, ValueType, "guid");
    }
    
    /// <summary>
    /// In the process of being removed, please move functionality to <see cref="CanImplicitCastToType"/>
    /// </summary>
    [Obsolete]
    public virtual bool CanImplicitCastToUint => false;
    
    /// <summary>
    /// Programmer utility, casts this terms value to <see cref="TermU"/>
    /// </summary>
    /// <returns>This terms value casted to a uint</returns>
    public virtual uint CastToUint()
    {
        return (uint)CastToType("uint");
    }
    
    /// <summary>
    /// In the process of being removed, please move functionality to <see cref="CanImplicitCastToType"/>
    /// </summary>
    [Obsolete]
    public virtual bool CanImplicitCastToInt => false;
    
    /// <summary>
    /// Programmer utility, casts this terms value to <see cref="TermI"/>
    /// </summary>
    /// <returns>This terms value casted to an integer</returns>
    public virtual int CastToInt()
    {
        return (int)CastToType("int");
    }
    
    /// <summary>
    /// In the process of being removed, please move functionality to <see cref="CanImplicitCastToType"/>
    /// </summary>
    [Obsolete]
    public virtual bool CanImplicitCastToFloat => false;
    
    /// <summary>
    /// Programmer utility, casts this terms value to <see cref="TermF"/>
    /// </summary>
    /// <returns>This terms value casted to an float</returns>
    public virtual float CastToFloat()
    {
        return (float)CastToType("float");
    }
    
    /// <summary>
    /// In the process of being removed, please move functionality to <see cref="CanImplicitCastToType"/>
    /// </summary>
    [Obsolete]
    public virtual bool CanImplicitCastToDouble => false;
    
    /// <summary>
    /// Programmer utility, casts this terms value to <see cref="TermD"/>
    /// </summary>
    /// <returns>This terms value casted to an double</returns>
    public virtual double CastToDouble()
    {
        return (double)CastToType("double");
    }
    
    /// <summary>
    /// In the process of being removed, please move functionality to <see cref="CanImplicitCastToType"/>
    /// </summary>
    [Obsolete]
    public virtual bool CanImplicitCastToBool => false;
    
    /// <summary>
    /// Programmer utility, casts this terms value to <see cref="BoolTerm"/>
    /// </summary>
    /// <returns>This terms value casted to an bool</returns>
    public virtual bool CastToBool()
    {
        return (bool)CastToType("bool");
    }

    public virtual bool CanImplicitCastToType(TermType type)
    {
        switch (type.Name) // TODO: these are obsolete
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
    
    /// <summary>
    /// Casts this terms value into the specified type
    /// </summary>
    /// <param name="name">The name of the type to cast to</param>
    /// <returns>The casted value</returns>
    /// <exception cref="InvalidTermCastException">Occurs when the cast was not valid</exception>
    public virtual object CastToType(string name)
    {
        if (name == ValueType)
            return GetValue();
        
        throw new InvalidTermCastException(0, ValueType, name);
    }

    #endregion

    #region Operators

    #region Math

    /// <summary>
    /// An array of all the math operations supported by this term
    /// </summary>
    public virtual MathOperatorKind[] AllowedMathOps => new MathOperatorKind[0];

    /// <summary>
    /// This conducts a math operation with another <see cref="BaseTerm"/>
    /// </summary>
    /// <param name="kind">The kind of math operation being conducted</param>
    /// <param name="subject">The input to the math operation</param>
    /// <returns>The result of the operation</returns>
    /// <exception cref="NotImplementedException">Occurs if this term supports no math operations</exception>
    public virtual object ConductMath(MathOperatorKind kind, BaseTerm subject)
    {
        throw new NotImplementedException($"{ValueType} does not support any operators");
    }

    #endregion
    
    #region Comparison

    /// <summary>
    /// An array of all the comparison operations supported by this term 
    /// </summary>
    public virtual ComparisonOperatorKind[] AllowedComparisons => new[]
    {
        ComparisonOperatorKind.Equal,
        ComparisonOperatorKind.NotEqual
    };

    /// <summary>
    /// Conducts a comparison against another term
    /// </summary>
    /// <param name="kind">The kind of comparison to conduct</param>
    /// <param name="subject">The term to compare</param>
    /// <returns>A boolean which was the result of the comparison</returns>
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

        return !GetValue().Equals(subject.GetValue());
    }

    #endregion
    
    #region Bool

    /// <summary>
    /// An array of all the allowed Bool Operators
    /// </summary>
    public virtual BoolOperatorKind[] AllowedBoolOps => new BoolOperatorKind[0];

    /// <summary>
    /// Conducts a boolean operation with another term
    /// </summary>
    /// <param name="kind">The kind of boolean operation to conduct</param>
    /// <param name="subject">The input to the boolean operation</param>
    /// <returns>The result of the boolean operation</returns>
    /// <exception cref="NotImplementedException">Occurs if this term does not support any boolean operators</exception>
    public virtual bool ConductBoolOp(BoolOperatorKind kind, BaseTerm subject)
    {
        throw new NotImplementedException($"{ValueType} term does not support any operators");
    }

    #endregion

    #region Indexing

    /// <summary>
    /// Whether or not this term supports indexing operations
    /// </summary>
    public virtual bool SupportsIndexing { get; }
    public virtual string IndexerType { get; }
    public virtual string IndexingReturnType => "term";

    public virtual ReturnValue ConductIndexingOperation(BaseTerm input)
    {
        throw new NotImplementedException($"{ValueType} does not support indexing");
    }

    #endregion

    #endregion

    #region Value manipulation

    /// <summary>
    /// Parse the term from a string.
    /// </summary>
    /// <param name="value">The pure value from the token</param>
    /// <returns>A bool indicating whether the operation was a success. If false a <see cref="CompilationException"/> will be thrown</returns>
    /// <remarks>This value is hard coded to work with literal types(<see cref="BoolTerm"/>, <see cref="NumberTerm"/>, <see cref="StringTerm"/>) by the compiler.</remarks>
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
}