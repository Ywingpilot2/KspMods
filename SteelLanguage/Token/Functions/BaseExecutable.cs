using System;
using System.Collections.Generic;
using SteelLanguage.Exceptions;
using SteelLanguage.Library;
using SteelLanguage.Token.Interaction;
using SteelLanguage.Token.Terms;

namespace SteelLanguage.Token.Functions;

public abstract class BaseExecutable : ITokenHolder, IExecutable
{
    public ITokenHolder Container { get; }

    protected Dictionary<string, BaseTerm> BaseTerms;
    private Dictionary<string, object> _compiledValues;
    protected List<TokenCall> Calls;

    #region Token Holder
    
    public virtual IEnumerable<TokenCall> EnumerateCalls()
    {
        foreach (TokenCall call in Calls)
        {
            yield return call;
        }
    }

    public virtual IEnumerable<BaseTerm> EnumerateTerms()
    {
        foreach (BaseTerm term in Container.EnumerateTerms())
        {
            yield return term;
        }

        foreach (BaseTerm term in BaseTerms.Values)
        {
            yield return term;
        }
    }

    public IFunction GetFunction(string name) => Container.GetFunction(name);

    public bool HasFunction(string name) => Container.HasFunction(name);

    public virtual BaseTerm GetTerm(string name)
    {
        if (!BaseTerms.ContainsKey(name))
            return Container.GetTerm(name);

        return BaseTerms[name];
    }

    public virtual bool HasTerm(string name)
    {
        if (!BaseTerms.ContainsKey(name))
            return Container.HasTerm(name);
        
        return true;
    }

    public void AddCall(TokenCall call)
    {
        Calls.Add(call);
    }

    public void AddTerm(BaseTerm term)
    {
        if (HasTerm(term.Name))
            throw new TermAlreadyExistsException(0, term.Name);
        
        BaseTerms.Add(term.Name, term);
    }

    public LibraryManager GetLibraryManager() => Container.GetLibraryManager();

    public void AddFunc(IFunction function)
    {
        throw new InvalidCompilationException(0, "Cannot declare a function here");
    }

    #endregion

    public virtual ReturnValue Execute(params BaseTerm[] terms)
    {
        throw new NotImplementedException();
    }

    public virtual ReturnValue Execute()
    {
        throw new NotImplementedException();
    }
    
    #region Pre/Post events

    public virtual void PreExecution()
    {
        foreach (BaseTerm term in BaseTerms.Values)
        {
            term.SetValue(_compiledValues[term.Name]);
        }
    }

    public virtual void PostExecution()
    {
    }

    protected bool HasCompiled;
    public virtual void PostCompilation()
    {
        if (HasCompiled)
            return;
        HasCompiled = true;
        
        foreach (TokenCall call in Calls)
        {
            call.PostCompilation();
        }
        
        foreach (BaseTerm term in BaseTerms.Values)
        {
            if (_compiledValues.ContainsKey(term.Name))
                continue;
            
            _compiledValues.Add(term.Name, term.GetValue());
        }
    }

    #endregion

    public BaseExecutable(ITokenHolder holder)
    {
        BaseTerms = new Dictionary<string, BaseTerm>();
        _compiledValues = new Dictionary<string, object>();
        Calls = new List<TokenCall>();
        Container = holder;
    }
}