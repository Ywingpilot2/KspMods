using SteelLanguage.Library.System.Terms.Complex.Enumerators;
using SteelLanguage.Token.Interaction;

namespace SteelLanguage.Token.Functions.Modifier;

/// <summary>
/// Constructs an array from the specified params
/// </summary>
public class ParamsCall : TokenCall
{
    private readonly string _type;
    private readonly Input[] _inputs;
    
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

        return new ReturnValue(array, $"Array<{_type}>");
    }
}