using System;
using System.Collections.Generic;
using ActionLanguage.Exceptions;
using ActionLanguage.Library;
using ActionLanguage.Token.Fields;
using ActionLanguage.Token.Functions;
using ActionLanguage.Token.Interaction;
using ActionLanguage.Token.Terms;
using ActionLanguage.Utils;

namespace ActionLanguage.Reflection;

public sealed class TermType
{
    public string Name
    {
        get
        {
            if (ContainsType && ContainedType != null)
            {
                return $"{Term.ValueType}<{ContainedType}>";
            }

            return Term.ValueType;
        }
    }
    public TermType BaseClass { get; }
    public bool IsAbstract { get; }
    public bool IsNullable { get; }
    public bool DefaultConstruction => HasConstructor("");

    public bool ContainsType => Term.ContainsType;
    public string ContainedType { get; set; }
    
    public MathOperatorKind[] AllowedMathOps => Term.AllowedMathOps;
    public ComparisonOperatorKind[] AllowedComparisons => Term.AllowedComparisons;
    public BoolOperatorKind[] AllowedBoolOps => Term.AllowedBoolOps;
    
    public IEnumerable<IFunction> Functions => Term.GetFunctions();
    public IEnumerable<TermField> Fields => Term.GetFields();
    public IEnumerable<TermConstructor> Constructors => Term.GetConstructors();
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
        return Term.GetConstructor(sig).Execute(inputs);
    }
    
    public bool IsSubclassOf(string name)
    {
        TermType current = BaseClass;

        while (current != null)
        {
            if (current.Name == name)
                return true;

            current = current.BaseClass;
        }

        return false;
    }

    public bool CanImplicitCastTo(string name) => Term.CanImplicitCastToType(name);

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