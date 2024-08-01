using System.Collections.Generic;
using System.Linq;
using SteelLanguage.Token.Interaction;
using SteelLanguage.Token.Terms;

namespace SteelLanguage.Token.Functions;

public class LocalCall : TokenCall
{
    private readonly Input _input;
    private readonly string _funcName;
    private readonly Input[] _inputs;
    
    public override ReturnValue Call()
    {
        if (_inputs.Length == 0)
            return _input.GetValue().GetFunction(_funcName).Execute();
        
        BaseTerm[] terms = new BaseTerm[_inputs.Length];
        for (int i = 0; i < _inputs.Length; i++)
        {
            terms.SetValue(_inputs[i].GetValue(), i);
        }

        return _input.GetValue().GetFunction(_funcName).Execute(terms);
    }

    // TODO: Cheap pre/post execution
    public override void PostCompilation()
    {
        _input.PostCompilation();
        foreach (Input input in _inputs)
        {
            input.PostCompilation();
        }
    }

    public LocalCall(ITokenHolder container, string function, IEnumerable<Input> inputs, int line, Input term) : base(container, line)
    {
        _funcName = function;
        _input = term;
        _inputs = inputs.ToArray();
    }
}