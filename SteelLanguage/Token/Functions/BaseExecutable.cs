using System;
using System.Collections.Generic;
using SteelLanguage.Exceptions;
using SteelLanguage.Library;
using SteelLanguage.Reflection.Library;
using SteelLanguage.Token.Interaction;
using SteelLanguage.Token.Terms;

namespace SteelLanguage.Token.Functions;

/// <summary>
/// Base class for an <see cref="T:SteelLanguage.Token.Functions.IExecutable" /> <see cref="T:SteelLanguage.Token.ITokenHolder" />. 
/// </summary>
public abstract record BaseExecutable : ITokenHolder, IExecutable
{
    public ITokenHolder Container { get; }

    protected readonly Dictionary<string, TermHolder> BaseTerms;
    protected readonly List<TokenCall> Calls;

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

        foreach (TermHolder term in BaseTerms.Values)
        {
            yield return term.GetTerm();
        }
    }

    public IFunction GetFunction(string name) => Container.GetFunction(name);

    public bool HasFunction(string name) => Container.HasFunction(name);

    public virtual BaseTerm GetTerm(string name)
    {
        if (!BaseTerms.ContainsKey(name))
            return Container.GetTerm(name);

        return BaseTerms[name].GetTerm();
    }

    public virtual TermHolder GetHolder(string name)
    {
        if (!BaseTerms.ContainsKey(name))
            return Container.GetHolder(name);

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

        TermHolder holder = new TermHolder(term.GetTermType().Name)
        {
            Name = term.Name
        };
        holder.SetTerm(term);
        BaseTerms.Add(term.Name, holder);
    }

    public void AddHolder(TermHolder holder)
    {
        if (HasTerm(holder.Name))
            throw new TermAlreadyExistsException(0, holder.Name);
        
        BaseTerms.Add(holder.Name, holder);
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
    }

    #endregion

    protected BaseExecutable(ITokenHolder holder)
    {
        BaseTerms = new Dictionary<string, TermHolder>();
        Calls = new List<TokenCall>();
        Container = holder;
    }
}