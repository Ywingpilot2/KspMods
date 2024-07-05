using SteelLanguage.Token.Functions.Single;
using SteelLanguage.Token.Interaction;
using SteelLanguage.Token.Terms;

namespace SteelLanguage.Token.Functions.Conditional;

public interface IConditionalCall
{
    public SingleExecutableFunc ExecutableFunc { get; }
    public IConditionalCall Else { get; set; }

    public ReturnValue Call();

    public IConditionalCall GetLastBranch();
    public void PostCompilation();
}

public class IfCall : TokenCall, IConditionalCall
{
    public SingleExecutableFunc ExecutableFunc { get; }
    public IConditionalCall Else { get; set; }
    private Input _condition;
    
    public IfCall(ITokenHolder container, int line, Input condition) : base(container, line)
    {
        _condition = condition;
        ExecutableFunc = new SingleExecutableFunc(container);
    }

    public override ReturnValue Call()
    {
        ReturnValue value = new ReturnValue();
        if (_condition.GetValue().CastToBool())
        {
            value = ExecutableFunc.Execute();
        }
        else if (Else != null)
        {
            value = Else.Call();
        }

        return value;
    }

    public override void PreExecution()
    {
        ExecutableFunc.PreExecution();
    }

    public override void PostExecution()
    {
        ExecutableFunc.PostExecution();
    }

    public override void PostCompilation()
    {
        ExecutableFunc.PostCompilation();
        Else?.PostCompilation();
        _condition.PostCompilation();
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
    
    public ElseCall(ITokenHolder container, int line) : base(container, line)
    {
        ExecutableFunc = new SingleExecutableFunc(container);
    }

    public override ReturnValue Call()
    {
        return ExecutableFunc.Execute();
    }
    
    public override void PreExecution()
    {
        ExecutableFunc.PreExecution();
    }

    public override void PostExecution()
    {
        ExecutableFunc.PostExecution();
    }

    public override void PostCompilation()
    {
        ExecutableFunc.PostCompilation();
        Else?.PostCompilation();
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