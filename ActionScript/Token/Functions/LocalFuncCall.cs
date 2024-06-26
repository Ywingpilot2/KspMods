﻿using System.Collections.Generic;
using System.Linq;
using ActionLanguage.Token.Interaction;
using ActionLanguage.Token.Terms;

namespace ActionLanguage.Token.Functions;

public class LocalCall : TokenCall
{
    private Input _input;
    private string _funcName;
    private Input[] _inputs;
    
    public override ReturnValue Call()
    {
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

    public LocalCall(ITokenHolder script, string function, IEnumerable<Input> inputs, int line, Input term) : base(script, line)
    {
        _funcName = function;
        _input = term;
        _inputs = inputs.ToArray();
    }
}