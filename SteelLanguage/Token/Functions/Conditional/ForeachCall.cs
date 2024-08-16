using System;
using System.Collections;
using SteelLanguage.Library.System.Terms.Complex;
using SteelLanguage.Token.Functions.Single;
using SteelLanguage.Token.Interaction;
using SteelLanguage.Token.Terms;

namespace SteelLanguage.Token.Functions.Conditional;

public class ForeachCall : TokenCall
{
    private readonly ForeachFunc _foreach;
    
    public ForeachCall(ITokenHolder container, int line, ForeachFunc function) : base(container, line)
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

public record ForeachFunc : BaseExecutable
{
    private readonly Input _enumerator;
    private readonly string _term;

    public override ReturnValue Execute()
    {
        bool shouldBreak = false;
        EnumeratorTerm term = (EnumeratorTerm)_enumerator.GetValue();

        IEnumerator enumerator = term.GetEnumerableValue().GetEnumerator();
        while (enumerator.MoveNext())
        {
            if (shouldBreak)
            {
                enumerator.Reset();
                break;
            }

            GetHolder(_term).SetTerm((BaseTerm)enumerator.Current);
            foreach (TokenCall call in Calls)
            {
                if (call is BreakCall)
                {
                    shouldBreak = true;
                    break;
                }
                
                if (call is ContinueCall)
                {
                    break;
                }

                if (call is ReturnCall)
                    return new ReturnValue(call, "return");
                
                // TODO HACK: In order to break when in lower functions, lower functions(e.g ifs) return a break/continue up the chain
                // This is annoying. We should find a better system asap!
                call.PreExecution();
                ReturnValue returnValue = call.Call();
                call.PostExecution();
                if (returnValue.HasValue)
                {
                    if (returnValue.Value is BreakCall)
                    {
                        shouldBreak = true;
                        break;
                    }
                
                    if (returnValue.Value is ContinueCall)
                    {
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

        return ReturnValue.None;
    }

    public override void PostCompilation()
    {
        if (!HasCompiled)
            _enumerator.PostCompilation();
        
        base.PostCompilation();
    }

    public ForeachFunc(ITokenHolder holder, Input enumerator, string enumTerm) : base(holder)
    {
        _enumerator = enumerator;
        _term = enumTerm;
    }
}