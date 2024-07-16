using System;
using System.Collections.Generic;
using SteelLanguage.Exceptions;
using SteelLanguage.Extensions;
using SteelLanguage.Reflection.Type;
using SteelLanguage.Token.Terms;

namespace SteelLanguage.Reflection.Library;

public class TypeLibrary
{
    public Dictionary<string, TermType> Types { get; }

    public bool HasTermType(string name)
    {
        // type container
        if (name.EndsWith(">"))
        {
            string[] split = name.SanitizedSplit('<', 2, StringSplitOptions.RemoveEmptyEntries);
            
            string currentType = split[0].Trim();
            return Types.ContainsKey(currentType);
        }
        
        return Types.ContainsKey(name);
    }

    public TermType GetTermType(string name, int line)
    {
        if (name.EndsWith(">"))
        {
            string[] split = name.SanitizedSplit('<', 2, StringSplitOptions.RemoveEmptyEntries);
            
            string containedType = split[1].Remove(split[1].Length - 1);
            string currentType = split[0].Trim();
            
            if (!Types.ContainsKey(currentType))
                throw new TypeNotExistException(line, currentType);

            TermType example = Types[currentType];
            
            // TODO HACK!!!1 This is a stupid way of ensuring that the BaseTerm gets the correct ContainedType, ensuring the constructor call
            // actually fucking works, this is stupid, pls fix!
            BaseTerm copyTerm = (BaseTerm)Activator.CreateInstance(example.Term.GetType());
            copyTerm.ContainedType = containedType;
            TermType copy = new TermType(copyTerm, example.BaseClass, example.IsAbstract)
            {
                ContainedType = containedType
            };

            return copy;
        }
        
        if (!Types.ContainsKey(name))
            throw new TypeNotExistException(line, name);

        return Types[name];
    }

    public TermType GetByClass(string className)
    {
        foreach (TermType type in EnumerateTypes())
        {
            if (type.Name == className)
                return type;
        }
        
        throw new TypeNotExistException(0, className);
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

    public TypeLibrary()
    {
        Types = new Dictionary<string, TermType>();
    }
}