using System;
using System.Collections.Generic;
using SteelLanguage.Exceptions;
using SteelLanguage.Extensions;
using SteelLanguage.Reflection.Library;
using SteelLanguage.Token.Fields;
using SteelLanguage.Token.Functions;
using SteelLanguage.Token.Functions.Modifier;
using SteelLanguage.Token.Functions.Operator;
using SteelLanguage.Token.Functions.Single;
using SteelLanguage.Token.Interaction;
using SteelLanguage.Token.Terms;
using SteelLanguage.Utils;

namespace SteelLanguage.Reflection.Type;

public sealed record TermType
{
    /// <summary>
    /// <see cref="Name"/> except with the type args stripped
    /// </summary>
    public string ShortName => Term.ValueType;
    public string Name
    {
        get
        {
            if (ContainsType && ContainedType != null)
            {
                return $"{Term.ValueType}<{string.Join(", ", ContainedType)}>";
            }

            return Term.ValueType;
        }
    }
    public TermType BaseClass { get; }
    public bool IsAbstract { get; }
    public bool IsNullable { get; }
    public bool DefaultConstruction => HasConstructor("");

    public bool ContainsType => Term.ContainsType;
    public string[] ContainedType { get; set; }
    public string[] TypeArgs => Term.ContainedTypeInputs;

    public bool SupportsIndexing => Term.SupportsIndexing;
    public string IndexingType => Term.IndexerType;
    public string IndexingReturnType => Term.IndexingReturnType;

    public MathOperatorKind[] AllowedMathOps => Term.AllowedMathOps;
    public ComparisonOperatorKind[] AllowedComparisons => Term.AllowedComparisons;
    public BoolOperatorKind[] AllowedBoolOps => Term.AllowedBoolOps;
    
    public IEnumerable<IFunction> Functions => Term.GetFunctions();
    public IEnumerable<TermField> Fields => Term.GetFields();
    public IEnumerable<TermConstructor> Constructors => Term.GetConstructors();

    public IEnumerable<TermField> StaticFields => Term.GetStaticFields();
    public TermField GetStaticField(string name) => Term.GetStaticField(name);
    public bool SetStaticField(string name, object value) => Term.SetStaticField(name, value);

    internal BaseTerm Term { get; }

    public bool HasConstructor(string sig) => Term.HasConstructor(sig);
    public TermConstructor GetConstructor(string sig) => Term.GetConstructor(sig);

    public BaseTerm Construct(string name, int line, LibraryManager manager)
    {
        if (IsAbstract)
            throw new TypeNotConstructableException(line, Name);

        BaseTerm copy = (BaseTerm)Activator.CreateInstance(Term.GetType());
        copy.Name = name;
        copy.Line = line;
        copy.TypeLibrary = manager;
        copy.ContainedType = ContainedType;

        return copy;
    }

    public ReturnValue Construct(string sig, params BaseTerm[] inputs)
    {
        if (inputs.Length == 0)
            return Term.GetConstructor("").Execute();
        
        return Term.GetConstructor(sig).Execute(inputs);
    }
    
    public bool IsSubclassOf(string name)
    {
        TermType current = BaseClass;

        if (ContainsType && name.EndsWith(">"))
        {
            string[] split = name.SanitizedSplit('<', 2, StringSplitOptions.RemoveEmptyEntries);
            string actualName = split[0].Trim();

            while (current != null) // TODO: is this how we should do this?
            {
                if (current.Name == actualName)
                    return true;
                current = current.BaseClass;
            }
        }
        else
        {
            while (current != null)
            {
                if (current.Name == name)
                    return true;

                current = current.BaseClass;
            }
        }

        return false;
    }

    public bool CanImplicitCastTo(TermType type) => Term.CanImplicitCastToType(type);

    public bool HasFunction(string name) => Term.HasFunction(name);
    public IFunction GetFunction(string name) => Term.GetFunction(name);

    public bool HasField(string name) => Term.HasField(name);
    public TermField GetField(string name) => Term.GetField(name);

    public string GetClassName() => Term.GetType().Name;

    public TermType(BaseTerm term, TermType baseClass = null, bool isAbstract = false, bool isNullable = false)
    {
        Term = term;
        BaseClass = baseClass;
        IsAbstract = isAbstract;
        IsNullable = isNullable;
    }

    public override string ToString()
    {
        return Name;
    }
}