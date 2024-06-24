using System;
using ActionLanguage.Exceptions;
using ActionLanguage.Extensions;
using ActionLanguage.Token.Functions;
using ActionLanguage.Token.Functions.Single;
using ActionLanguage.Token.Interaction;
using ActionLanguage.Utils;

namespace ActionLanguage.Token.KeyWords;

public struct ReturnKeyword : IKeyword
{
    public string Name => "return";

    private ActionCompiler _compiler;
    public void CompileKeyword(string token, ActionCompiler compiler, ActionScript script, ITokenHolder tokenHolder)
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