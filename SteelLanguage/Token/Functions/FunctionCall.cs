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
    // NOTICE: this isn't a real cache.
    // Dynamic and static inputs are mixed together in a muddy way, and the compiler is not setup to recognize the difference
    // this is instead meant to prevent us from creating lots and lots of arrays in memory, since we only need 1 array of a fixed length
    private readonly BaseTerm[] _cache;
    private readonly Input[] _inputs;

    private IFunction Function { get; }

    public override ReturnValue Call()
    {
        if (_inputs.Length == 0)
            return Function.Execute();
        
        for (var i = 0; i < _inputs.Length; i++)
        {
            _cache.SetValue(_inputs[i].GetValue(), i);
        }

        return Function.Execute(_cache);
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
        foreach (Input input in _inputs)
        {
            input.PostCompilation();
        }
    }

    #endregion

    public FunctionCall(ITokenHolder container, IFunction function, IEnumerable<Input> inputs, int line) : base(container, line)
    {
        Function = function;
        _inputs = inputs.ToArray();
        _cache = new BaseTerm[_inputs.Length];
    }

    public FunctionCall(ITokenHolder container, IFunction function, int line, params Input[] inputs) : base(container, line)
    {
        Function = function;
        _inputs = inputs;
        _cache = new BaseTerm[_inputs.Length];
    }
}