﻿using SteelLanguage.Exceptions;
using SteelLanguage.Token.Interaction;

namespace SteelLanguage.Token.Functions.Single;

public class ThrowCall : TokenCall
{
    private Input _input;
    
    public ThrowCall(ITokenHolder container, int line, Input exception) : base(container, line)
    {
        _input = exception;
    }

    public override ReturnValue Call()
    {
        throw new InvalidActionException(Line, _input.GetValue().CastToStr());
    }

    public override void PostCompilation()
    {
        _input.PostCompilation();
    }
}