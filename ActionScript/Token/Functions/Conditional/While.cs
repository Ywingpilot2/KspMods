using ActionLanguage.Token.Functions.Single;
using ActionLanguage.Token.Interaction;
using ActionLanguage.Token.Terms;

namespace ActionLanguage.Token.Functions.Conditional;

public class WhileCall : TokenCall
{
    private WhileFunction _while;
    
    public WhileCall(ITokenHolder script, int line, WhileFunction function) : base(script, line)
    {
        _while = function;
    }

    public override ReturnValue Call()
    {
        ReturnValue value = _while.Execute();
        return value;
    }

    public override void PreExecution()
    {
        _while.PreExecution();
    }

    public override void PostExecution()
    {
        _while.PostExecution();
    }

    public override void PostCompilation()
    {
        _while.PostCompilation();
    }
}

public class WhileFunction : BaseExecutable
{
    private Input _condition;

    public override ReturnValue Execute(params BaseTerm[] terms)
    {
        bool shouldBreak = false;
        bool shouldContinue = false;
        while (_condition.GetValue().CastToBool())
        {
            if (shouldBreak)
                break;
            
            if (shouldContinue)
                continue;
            
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
                        return returnValue;
                }
            }
        }

        return new ReturnValue();
    }

    public override void PostCompilation()
    {
        base.PostCompilation();
        _condition.PostCompilation();
    }

    public WhileFunction(Input condition, ITokenHolder holder) : base(holder)
    {
        _condition = condition;
    }
}