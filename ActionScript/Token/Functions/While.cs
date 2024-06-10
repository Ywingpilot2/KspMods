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
        return _while.Execute();
    }
}

public class WhileFunction : BaseExecutable
{
    private Input _condition;

    public override ReturnValue Execute(params BaseTerm[] terms)
    {
        bool shouldBreak = false;
        while (_condition.GetValue().CastToBool())
        {
            if (shouldBreak)
                break;
            
            foreach (TokenCall call in Calls)
            {
                if (call is BreakCall)
                {
                    shouldBreak = true;
                    break;
                }
                
                if (call is ContinueCall)
                    break; // This will break the foreach causing us to skip to the next call

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
                        break;

                    if (returnValue.Value is ReturnCall)
                        return returnValue;
                }
            }
        }

        return new ReturnValue();
    }

    public WhileFunction(Input condition, ITokenHolder holder) : base(holder)
    {
        _condition = condition;
    }
}