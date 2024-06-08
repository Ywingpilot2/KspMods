using System;
using System.Collections.Generic;
using ActionScript.Exceptions;
using ActionScript.Functions;
using ActionScript.Terms;

namespace ActionScript.Library;

public struct TermType
{
    public string Name => Term.ValueType;
    public TermKind Kind { get; }
    public TypeLibrary Library { get; }
    public BaseTerm Term { get; }

    public BaseTerm Construct(string name, int line)
    {
        BaseTerm copy = (BaseTerm)Activator.CreateInstance(Term.GetType());
        copy.Name = name;
        copy.Line = line;
        copy.TypeLibrary = Library;
        return copy;
    }

    public TermType(BaseTerm term, TypeLibrary library)
    {
        Term = term;
        Kind = TermKind.Basic;
        Library = library;
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
        if (type.Kind == TermKind.Null)
            return;
        
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