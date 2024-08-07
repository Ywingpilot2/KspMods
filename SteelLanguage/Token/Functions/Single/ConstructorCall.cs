﻿using System.Collections.Generic;
using System.Linq;
using SteelLanguage.Reflection.Type;
using SteelLanguage.Token.Interaction;
using SteelLanguage.Token.Terms;

namespace SteelLanguage.Token.Functions.Single;

public class ConstructorCall : TokenCall
{
    private readonly Input[] _inputs;
    private readonly TermType _type;
    private readonly string _sig;
    
    public ConstructorCall(ITokenHolder container, int line, string sig, TermType type, IEnumerable<Input> inputs) : base(container, line)
    {
        _type = type;
        _inputs = inputs.ToArray();
        _sig = sig;
    }

    public override ReturnValue Call()
    {
        BaseTerm[] terms = new BaseTerm[_inputs.Length];
        for (int i = 0; i < _inputs.Length; i++)
        {
            terms.SetValue(_inputs[i].GetValue(), i);
        }

        return _type.Construct(_sig, terms);
    }
}