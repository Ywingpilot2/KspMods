using System.Collections.Generic;
using ActionLanguage.Exceptions;
using ActionLanguage.Reflection;
using ActionLanguage.Token.Functions;
using ActionLanguage.Token.KeyWords;
using ActionLanguage.Token.Terms;

namespace ActionLanguage.Library;

/// <summary>
/// Centralized class for handling <see cref="ILibrary"/>s
/// </summary>
public class LibraryManager
{
    private Dictionary<string, ILibrary> _libraries;
    private Dictionary<string, IKeyword> _keywords;
    private Dictionary<string, IFunction> _functions;
    private Dictionary<string, BaseTerm> _globals;
    
    public bool HasGlobalTerm(string name) => _globals.ContainsKey(name);

    public BaseTerm GetGlobalTerm(string name)
    {
        if (!HasGlobalTerm(name))
            throw new TermNotExistException(0, name);

        return _globals[name];
    }

    public IEnumerable<BaseTerm> EnumerateGlobalTerms()
    {
        foreach (BaseTerm globalsValue in _globals.Values)
        {
            yield return globalsValue;
        }
    }

    public bool HasFunction(string name) => _functions.ContainsKey(name);

    public IFunction GetFunction(string name)
    {
        if (!HasFunction(name))
            throw new FunctionNotExistException(0, name);

        return _functions[name];
    }

    public IKeyword GetKeyword(string name)
    {
        if (!HasKeyword(name))
            throw new InvalidCompilationException(0, $"Keyword {name} does not exist");
        
        return _keywords[name];
    }

    public bool HasKeyword(string name) => _keywords.ContainsKey(name);

    public IEnumerable<ILibrary> EnumerateLibraries()
    {
        foreach (ILibrary library in _libraries.Values)
        {
            yield return library;
        }
    }
    
    public bool HasTermType(string name)
    {
        foreach (ILibrary library in EnumerateLibraries())
        {
            if (library.TypeLibrary == null)
                continue;
            
            if (library.TypeLibrary.HasTermType(name))
                return true;
        }

        return false;
    }

    public TermType GetTermType(string name)
    {
        foreach (ILibrary library in EnumerateLibraries())
        {
            if (library.TypeLibrary == null)
                continue;
            
            if (library.TypeLibrary.HasTermType(name))
                return library.TypeLibrary.GetTermType(name, 0);
        }
        
        throw new TypeNotExistException(0, name);
    }

    public void ImportLibrary(ILibrary library)
    {
        _libraries.Add(library.Name, library);

        if (library.Keywords != null)
        {
            foreach (IKeyword keyword in library.Keywords)
            {
                _keywords.Add(keyword.Name, keyword);
            }
        }

        if (library.GlobalFunctions != null)
        {
            foreach (IFunction function in library.GlobalFunctions)
            {
                _functions.Add(function.Name, function);
            }
        }

        if (library.GlobalTerms != null)
        {
            foreach (GlobalTerm globalTerm in library.GlobalTerms)
            {
                BaseTerm term = GetTermType(globalTerm.TypeName).Construct(globalTerm.Name, 0, this);
                if (globalTerm.Source != null)
                    term.CopyFrom(globalTerm.Source);
                
                _globals.Add(globalTerm.Name, term);
            }
        }
    }

    public ILibrary GetLibrary(string name)
    {
        // TODO: Throw CompilationException if not found
        return _libraries[name];
    }

    public bool HasLibrary(string name) => _libraries.ContainsKey(name);
    
    public LibraryManager()
    {
        _libraries = new Dictionary<string, ILibrary>();
        _keywords = new Dictionary<string, IKeyword>();
        _functions = new Dictionary<string, IFunction>();
        _globals = new Dictionary<string, BaseTerm>();
    }
}