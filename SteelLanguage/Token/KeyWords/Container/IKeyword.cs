﻿namespace SteelLanguage.Token.KeyWords.Container;

public interface IKeyword
{
    public string Name { get; }

    public void CompileKeyword(string token, SteelCompiler compiler, SteelScript script, ITokenHolder tokenHolder);
}