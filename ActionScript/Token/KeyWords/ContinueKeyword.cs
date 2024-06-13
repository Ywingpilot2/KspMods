﻿using ActionLanguage.Token.Functions;

namespace ActionLanguage.Token.KeyWords;

public struct ContinueKeyword : IKeyword
{
    public string Name => "continue";
    public void CompileKeyword(string token, ActionCompiler compiler, ActionScript script, ITokenHolder tokenHolder)
    {
        tokenHolder.AddCall(new ContinueCall(tokenHolder, compiler.CurrentLine));
    }
}