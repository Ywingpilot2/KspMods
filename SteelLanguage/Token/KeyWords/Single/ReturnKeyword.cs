using System;
using SteelLanguage.Extensions;
using SteelLanguage.Token.Functions;
using SteelLanguage.Token.Functions.Single;
using SteelLanguage.Token.Interaction;
using SteelLanguage.Token.KeyWords.Container;
using SteelLanguage.Utils;

namespace SteelLanguage.Token.KeyWords.Single;

internal record ReturnKeyword : IKeyword
{
    public string Name => "return";

    private SteelCompiler _compiler;
    public void CompileKeyword(string token, SteelCompiler compiler, SteelScript script, ITokenHolder tokenHolder)
    {
        ITokenHolder current = tokenHolder;
        while (true)
        {
            if (current.Container == null || current is UserFunction yay)
            {
                break;
            }

            current = current.Container;
        }
        
        _compiler = compiler;
        tokenHolder.AddCall(ParseReturn(token, tokenHolder, current));
    }
    
    private ReturnCall ParseReturn(string token, ITokenHolder holder, ITokenHolder function)
    {
        string[] split = token.SanitizedSplit(' ', 2, StringSplitOptions.RemoveEmptyEntries);

        string returnType = "void";
        if (function is UserFunction user)
            returnType = user.ReturnType;

        if (returnType == "void")
            return new ReturnCall(holder, _compiler.CurrentLine);
        
        string value = split[1].Trim();

        Input input = CompileUtils.HandleToken(value, returnType, holder, _compiler);
        return new ReturnCall(holder, _compiler.CurrentLine, input);
    }
}