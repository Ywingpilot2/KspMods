using ActionLanguage.Token.Interaction;
using ActionLanguage.Token.Terms;

namespace ActionLanguage.Token.Functions;

public interface IConditionalCall
{
    public SingleExecutableFunc ExecutableFunc { get; }
    public IConditionalCall Else { get; set; }

    public ReturnValue Call();

    public IConditionalCall GetLastBranch();
}

public class IfCall : TokenCall, IConditionalCall
{
    public SingleExecutableFunc ExecutableFunc { get; }
    public IConditionalCall Else { get; set; }
    private Input _condition;
    
    public IfCall(ITokenHolder script, int line, Input condition) : base(script, line)
    {
        _condition = condition;
        ExecutableFunc = new SingleExecutableFunc(script);
    }

    public override ReturnValue Call()
    {
        if (_condition.GetValue().CastToBool())
        {
            return ExecutableFunc.Execute();
        }
        else if (Else != null)
        {
            return Else.Call();
        }
        
        return new ReturnValue();
    }

    public IConditionalCall GetLastBranch()
    {
        if (Else == null)
            return this;
        else
            return Else.GetLastBranch();
    }
}

public class ElseCall : TokenCall, IConditionalCall
{
    public SingleExecutableFunc ExecutableFunc { get; }
    public IConditionalCall Else { get; set; }
    
    public ElseCall(ITokenHolder script, int line) : base(script, line)
    {
        ExecutableFunc = new SingleExecutableFunc(script);
    }

    public override ReturnValue Call()
    {
        return ExecutableFunc.Execute();
    }

    public IConditionalCall GetLastBranch()
    {
        return this;
    }
}

public class SingleExecutableFunc : BaseExecutable
{
    public SingleExecutableFunc(ITokenHolder holder) : base(holder)
    {
    }

    public override ReturnValue Execute(params BaseTerm[] terms)
    {
        foreach (TokenCall call in Calls)
        {
            // TODO HACK: In order to break when in lower functions, lower functions(e.g ifs) return a break/continue up the chain
            // This is annoying. We should find a better system asap!
            ReturnValue returnValue = call.Call();
            if (returnValue.HasValue)
            {
                if (returnValue.Value is BreakCall)
                    return returnValue;

                if (returnValue.Value is ContinueCall)
                    return returnValue;

                if (returnValue.Value is ReturnCall)
                    return returnValue;
            }
        }

        return new ReturnValue();
    }
}