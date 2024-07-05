using System.Collections.Generic;
using System.Linq;
using SteelLanguage.Token.Interaction;
using SteelLanguage.Token.Terms;

namespace SteelLanguage.Token.Functions;

public abstract class TokenCall : BaseToken
{
    public abstract ReturnValue Call();

    #region Pre/Post events

    public virtual void PreExecution()
    {
    }

    public virtual void PostExecution()
    {
    }

    public virtual void PostCompilation()
    {
    }

    #endregion

    protected TokenCall(ITokenHolder container, int line) : base(container, line)
    {
    }
}
    
public class FunctionCall : TokenCall
{
    private Input[] Inputs;
    private IFunction Function { get; }

    public override ReturnValue Call()
    {
        BaseTerm[] terms = new BaseTerm[Inputs.Length];
        for (var i = 0; i < Inputs.Length; i++)
        {
            terms.SetValue(Inputs[i].GetValue(), i);
        }

        return Function.Execute(terms);
    }
        
    #region Pre/Post events

    public override void PreExecution()
    {
        Function.PreExecution();
    }

    public override void PostExecution()
    {
        Function.PostExecution();
    }

    public override void PostCompilation()
    {
        Function.PostCompilation();
        foreach (Input input in Inputs)
        {
            input.PostCompilation();
        }
    }

    #endregion

    public FunctionCall(ITokenHolder container, IFunction function, IEnumerable<Input> inputs, int line) : base(container, line)
    {
        Function = function;
        Inputs = inputs.ToArray();
    }

    public FunctionCall(ITokenHolder container, IFunction function, int line, params Input[] inputs) : base(container, line)
    {
        Function = function;
        Inputs = inputs;
    }
}