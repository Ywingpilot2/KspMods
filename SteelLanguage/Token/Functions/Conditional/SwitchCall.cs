﻿using System;
using System.Collections.Generic;
using System.Linq;
using SteelLanguage.Token.Functions.Single;
using SteelLanguage.Token.Interaction;
using SteelLanguage.Token.Terms;

namespace SteelLanguage.Token.Functions.Conditional;

public class SwitchCall : TokenCall
{
    private Input _check;

    private readonly Dictionary<object, int> _cases;
    private readonly object[] _keys;
    
    private readonly SingleExecutableFunc[] _values;
    private readonly SingleExecutableFunc _default;

    public override ReturnValue Call()
    {
        object value = _check.GetValue().GetValue();
        if (!_cases.ContainsKey(value))
        {
            if (_default == null)
                return new ReturnValue();

            return _default.Execute();
        }
        
        for (int i = _cases[value]; i < _keys.Length; i++)
        {
            SingleExecutableFunc func = _values[i];
            
            func.PreExecution();
            ReturnValue returnValue = func.Execute();
            func.PostExecution();
            
            if (returnValue.HasValue)
            {
                if (returnValue.Value is BreakCall)
                {
                    break;
                }
                
                if (returnValue.Value is ContinueCall)
                {
                    continue;
                }

                if (returnValue.Value is ReturnCall)
                    return returnValue;
            }
        }

        if (_default != null)
        {
            _default.PostExecution();
            ReturnValue returnValue = _default.Execute();
            _default.PostExecution();

            if (!returnValue.HasValue) return new ReturnValue();
            
            switch (returnValue.Value)
            {
                case BreakCall:
                case ContinueCall:
                case ReturnCall:
                    return returnValue;
            }
        }

        return new ReturnValue();
    }

    public override void PostCompilation()
    {
        _check.PostCompilation();
    }
    
    public SwitchCall(ITokenHolder container, int line, Input check, IEnumerable<object> cases, IEnumerable<SingleExecutableFunc> funcs, SingleExecutableFunc def) : base(container, line)
    {
        _check = check;
        _default = def;

        _cases = new Dictionary<object, int>();
        _keys = cases.ToArray();
        _values = funcs.ToArray();

        for (var i = 0; i < _keys.Length; i++)
        {
            object key = _keys[i];
            _cases.Add(key, i);
        }
    }
}