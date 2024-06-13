﻿using System;
using System.Collections.Generic;
using ActionLanguage.Exceptions;
using ActionLanguage.Token.Functions;
using ActionLanguage.Token.Terms;
using ActionLanguage.Utils;

namespace ActionLanguage.Library;

public class TermType
{
    public string Name => Term.ValueType;
    public TermType BaseClass { get; }
    public bool IsAbstract { get; }
    public TypeLibrary Library { get; }
    public OperatorKind[] AllowedOperators => Term.AllowedOperators;
    
    public IEnumerable<IFunction> Functions => Term.GetFunctions();
    private BaseTerm Term { get; }

    public BaseTerm Construct(string name, int line)
    {
        if (IsAbstract)
            throw new TypeNotConstructableException(line, Name);
        
        BaseTerm copy = (BaseTerm)Activator.CreateInstance(Term.GetType());
        copy.Name = name;
        copy.Line = line;
        copy.TypeLibrary = Library;
        return copy;
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

    public TermType(BaseTerm term, TypeLibrary library, TermType baseClass = null, bool isAbstract = false)
    {
        Term = term;
        Library = library;
        BaseClass = baseClass;
        IsAbstract = isAbstract;
    }

    public override string ToString()
    {
        return Name;
    }
}

public class TypeLibrary
{
    public Dictionary<string, TermType> Types { get; }

    public bool HasTermType(string name)
    {
        return Types.ContainsKey(name);
    }

    public TermType GetTermType(string name, int line)
    {
        if (!Types.ContainsKey(name))
            throw new TypeNotExistException(line, name);

        return Types[name];
    }

    public void AddTermType(TermType type)
    {
        if (HasTermType(type.Name))
            return;
        
        Types.Add(type.Name, type);
    }

    public IEnumerable<TermType> EnumerateTypes()
    {
        foreach (TermType type in Types.Values)
        {
            yield return type;
        }
    }

    public TypeLibrary(IEnumerable<TermType> types)
    {
        Types = new Dictionary<string, TermType>();
        foreach (TermType type in types)
        {
            AddTermType(type);
        }
    }

    public TypeLibrary(IEnumerable<BaseTerm> terms)
    {
        Types = new Dictionary<string, TermType>();
        foreach (BaseTerm term in terms)
        {
            AddTermType(term.GetTermType());
        }
    }

    public TypeLibrary(params TermType[] types)
    {
        Types = new Dictionary<string, TermType>();
        foreach (TermType type in types)
        {
            AddTermType(type);
        }
    }
}