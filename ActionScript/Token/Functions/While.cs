using System.Collections.Generic;
using ActionScript.Exceptions;
using ActionScript.Library;
using ActionScript.Token.Interaction;
using ActionScript.Token.KeyWords;
using ActionScript.Token.Terms;

namespace ActionScript.Token.Functions;

public class WhileCall : TokenCall
{
    private WhileFunction _while;
    
    public WhileCall(ITokenHolder script, int line, WhileFunction function) : base(script, line)
    {
        _while = function;
    }

    public override ReturnValue Call()
    {
        _while.Execute();
        
        return new ReturnValue(); // TODO: this sucks lol
    }
}

public class WhileFunction : ITokenHolder
{
    private ITokenHolder _holder;
    private Input _condition;
    
    private Dictionary<string, BaseTerm> _baseTerms;
    private List<TokenCall> _calls;
    
    public void Execute()
    {
        bool shouldBreak = false;
        while (_condition.GetValue().CastToBool())
        {
            if (shouldBreak)
                break;
            
            foreach (TokenCall call in _calls)
            {
                if (call is BreakCall)
                {
                    shouldBreak = true;
                    break;
                }
                
                if (call is ContinueCall)
                    break; // This will break the foreach causing us to skip to the next call
                
                // TODO HACK: In order to break when in lower functions, lower functions(e.g ifs) return a break/continue up the chain
                // This is annoying. We should find a better system asap!
                ReturnValue returnValue = call.Call();
                if (returnValue.HasValue && returnValue.Value is BreakCall)
                {
                    shouldBreak = true;
                    break;
                }
                
                if (returnValue.HasValue && returnValue.Value is ContinueCall)
                    break;
            }
        }
    }

    public WhileFunction(Input condition, ITokenHolder holder)
    {
        _condition = condition;
        _holder = holder;
        _baseTerms = new Dictionary<string, BaseTerm>();
        _calls = new List<TokenCall>();
    }

    public IFunction GetFunction(string name) => _holder.GetFunction(name);

    public bool HasFunction(string name) => _holder.HasFunction(name);

    public BaseTerm GetTerm(string name)
    {
        if (!_baseTerms.ContainsKey(name))
            return _holder.GetTerm(name);

        return _baseTerms[name];
    }

    public bool HasTerm(string name)
    {
        if (!_baseTerms.ContainsKey(name))
            return _holder.HasTerm(name);
        
        return true;
    }

    public void AddCall(TokenCall call)
    {
        _calls.Add(call);
    }

    public void AddTerm(BaseTerm term)
    {
        if (HasTerm(term.Name))
            throw new TermAlreadyExistsException(0, term.Name);
        
        _baseTerms.Add(term.Name, term);
    }

    public void AddFunc(IFunction function)
    {
        throw new InvalidCompilationException(0, "Cannot declare a function within a while statement");
    }

    public bool TermTypeExists(string name) => _holder.TermTypeExists(name);

    public TermType GetTermType(string name) => _holder.GetTermType(name);

    public bool HasKeyword(string name) => _holder.HasKeyword(name);

    public IKeyword GetKeyword(string name) => _holder.GetKeyword(name);
}