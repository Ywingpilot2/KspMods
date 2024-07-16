using System.Collections.Generic;
using System.Linq;
using SteelLanguage.Token.Functions.Modifier;
using SteelLanguage.Token.Functions.Single;
using SteelLanguage.Token.Interaction;
using SteelLanguage.Token.Terms;
using SteelLanguage.Utils;

namespace SteelLanguage.Token.Functions.Conditional;

public class MatchCall : TokenCall
{
    
    
    private Input _check;
    private readonly Input[] _keys;
    
    private readonly SingleExecutableFunc[] _values;
    private readonly SingleExecutableFunc _default;

    public override ReturnValue Call()
    {
        BaseTerm value = _check.GetValue();
        bool hasBroken = false;
        
        for (int i = 0; i < _keys.Length; i++)
        {
            Input key = _keys[i];
            if (!key.GetValue().ConductComparison(ComparisonOperatorKind.Equal, value))
                continue;

            SingleExecutableFunc func = _values[i];
            
            func.PreExecution();
            ReturnValue returnValue = func.Execute();
            func.PostExecution();
            
            if (returnValue.HasValue)
            {
                if (returnValue.Value is BreakCall)
                {
                    hasBroken = true;
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

        if (!hasBroken && _default != null)
        {
            _default.PreExecution();
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

        foreach (SingleExecutableFunc func in _values)
        {
            func.PostCompilation();
        }
        
        _default?.PostCompilation();
    }
    
    public MatchCall(ITokenHolder container, int line, Input check, IEnumerable<Input> cases, IEnumerable<SingleExecutableFunc> funcs, SingleExecutableFunc def) : base(container, line)
    {
        _check = check;
        _default = def;
        
        _keys = cases.ToArray();
        _values = funcs.ToArray();
    }
}