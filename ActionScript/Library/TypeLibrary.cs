using System;
using System.Collections.Generic;
using ActionLanguage.Exceptions;
using ActionLanguage.Extensions;
using ActionLanguage.Token;
using ActionLanguage.Token.Fields;
using ActionLanguage.Token.Functions;
using ActionLanguage.Token.Interaction;
using ActionLanguage.Token.Terms;
using ActionLanguage.Utils;

namespace ActionLanguage.Library;

public enum ConstructorKind
{
    Empty = 0,
    Filled = 1
}

public struct TermConstructor : IExecutable
{
    public string[] Inputs { get; }
    public ConstructorKind Kind { get; }
    
    private Func<BaseTerm[], ReturnValue> _action;

    public ReturnValue Execute(params BaseTerm[] terms)
    {
        return _action.Invoke(terms);
    }

    public string GetSig()
    {
        if (Kind == ConstructorKind.Empty)
        {
            return "";
        }

        return string.Join(" ", Inputs);
    }

    public TermConstructor()
    {
        Inputs = new string[0];
    }

    public TermConstructor(Func<BaseTerm[], ReturnValue> action, params string[] inputs)
    {
        _action = action;
        Inputs = inputs;
        Kind = ConstructorKind.Filled;
    }

    public void PreExecution()
    {
    }

    public void PostExecution()
    {
    }

    public void PostCompilation()
    {
    }
}

public class TermType
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
    
    public OperatorKind[] AllowedOperators => Term.AllowedOperators;
    
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
}