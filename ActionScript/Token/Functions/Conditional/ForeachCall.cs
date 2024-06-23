using System.Collections;
using ActionLanguage.Token.Functions.Single;
using ActionLanguage.Token.Interaction;
using ActionLanguage.Token.Terms;
using ActionLanguage.Token.Terms.Complex;

namespace ActionLanguage.Token.Functions.Conditional;

public class ForeachCall : TokenCall
{
    private ForeachFunc _foreach;
    
    public ForeachCall(ITokenHolder script, int line, ForeachFunc function) : base(script, line)
    {
        _foreach = function;
    }

    public override ReturnValue Call()
    {
        ReturnValue value = _foreach.Execute();
        return value;
    }

    public override void PreExecution()
    {
        _foreach.PreExecution();
    }

    public override void PostExecution()
    {
        _foreach.PostExecution();
    }

    public override void PostCompilation()
    {
        _foreach.PostCompilation();
    }
}

public class ForeachFunc : BaseExecutable
{
    private Input _enumerator;
    private string _term;

    public override ReturnValue Execute(params BaseTerm[] terms)
    {
        bool shouldBreak = false;
        bool shouldContinue = false;
        EnumeratorTerm term = (EnumeratorTerm)_enumerator.GetValue();

        IEnumerator enumerator = term.Value.GetEnumerator();
        while (enumerator.MoveNext())
        {
            if (shouldBreak)
            {
                enumerator.Reset();
                break;
            }
            
            if (shouldContinue)
                continue;

            GetTerm(_term).CopyFrom((BaseTerm)enumerator.Current);
            foreach (TokenCall call in Calls)
            {
                if (call is BreakCall)
                {
                    shouldBreak = true;
                    break;
                }
                
                if (call is ContinueCall)
                {
                    shouldContinue = true;
                    break;
                }

                if (call is ReturnCall)
                    return new ReturnValue(call, "return");
                
                // TODO HACK: In order to break when in lower functions, lower functions(e.g ifs) return a break/continue up the chain
                // This is annoying. We should find a better system asap!
                ReturnValue returnValue = call.Call();
                if (returnValue.HasValue)
                {
                    if (returnValue.Value is BreakCall)
                    {
                        shouldBreak = true;
                        break;
                    }
                
                    if (returnValue.Value is ContinueCall)
                    {
                        shouldContinue = true;
                        break;
                    }

                    if (returnValue.Value is ReturnCall)
                    {
                        enumerator.Reset();
                        return returnValue;
                    }
                }
            }
        }
        
        enumerator.Reset();

        return new ReturnValue();
    }

    public override void PostCompilation()
    {
        base.PostCompilation();
        _enumerator.PostCompilation();
    }

    public ForeachFunc(ITokenHolder holder, Input enumerator, string enumTerm) : base(holder)
    {
        _enumerator = enumerator;
        _term = enumTerm;
    }
}