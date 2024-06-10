using System.Collections.Generic;
using ActionScript.Exceptions;
using ActionScript.Library;
using ActionScript.Token.Interaction;
using ActionScript.Token.KeyWords;
using ActionScript.Token.Terms;

namespace ActionScript.Token.Functions;

public abstract class BaseExecutable : ITokenHolder, IExecutable
{
    public ITokenHolder Container { get; }

    protected Dictionary<string, BaseTerm> BaseTerms;
    protected List<TokenCall> Calls;

    #region Token Holder
    
    public IEnumerable<TokenCall> EnumerateCalls()
    {
        foreach (TokenCall call in Container.EnumerateCalls())
        {
            yield return call;
        }

        foreach (TokenCall call in Calls)
        {
            yield return call;
        }
    }

    public IEnumerable<BaseTerm> EnumerateTerms()
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

    public BaseTerm GetTerm(string name)
    {
        if (!BaseTerms.ContainsKey(name))
            return Container.GetTerm(name);

        return BaseTerms[name];
    }

    public bool HasTerm(string name)
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

    public void AddFunc(IFunction function)
    {
        throw new InvalidCompilationException(0, "Cannot declare a function within a while statement");
    }

    public bool TermTypeExists(string name) => Container.TermTypeExists(name);

    public TermType GetTermType(string name) => Container.GetTermType(name);

    public bool HasKeyword(string name) => Container.HasKeyword(name);

    public IKeyword GetKeyword(string name) => Container.GetKeyword(name);

    #endregion

    public abstract ReturnValue Execute(params BaseTerm[] terms);

    public BaseExecutable(ITokenHolder holder)
    {
        BaseTerms = new Dictionary<string, BaseTerm>();
        Calls = new List<TokenCall>();
        Container = holder;
    }
}