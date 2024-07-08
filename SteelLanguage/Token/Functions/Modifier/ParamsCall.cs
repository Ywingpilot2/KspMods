using SteelLanguage.Token.Interaction;
using SteelLanguage.Token.Terms.Complex.Enumerators;

namespace SteelLanguage.Token.Functions.Modifier;

/// <summary>
/// Constructs an array from the specified params
/// </summary>
public class ParamsCall : TokenCall
{
    private string _type;
    private Input[] _inputs;
    
    public ParamsCall(ITokenHolder container, int line, string type, Input[] inputs) : base(container, line)
    {
        _type = type;
        _inputs = inputs;
    }

    public override ReturnValue Call()
    {
        TermArray array = new TermArray(_type, _inputs.Length);
        for (var i = 0; i < _inputs.Length; i++)
        {
            Input input = _inputs[i];
            array.SetValue(input.GetValue(), i);
        }

        return new ReturnValue(array, $"array<{_type}>");
    }
}