using ActionScript.Exceptions;
using ActionScript.Token.Interaction;

namespace ActionScript.Token.Functions;

public class ThrowCall : TokenCall
{
    private Input _input;
    
    public ThrowCall(ITokenHolder script, int line, Input exception) : base(script, line)
    {
        _input = exception;
    }

    public override ReturnValue Call()
    {
        throw new InvalidActionException(Line, _input.GetValue().CastToStr());
    }
}