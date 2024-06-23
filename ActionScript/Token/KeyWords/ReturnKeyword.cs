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
        UserFunction function;
        ITokenHolder current = tokenHolder;
        while (true)
        {
            if (current == null)
                throw new InvalidCompilationException(_compiler.CurrentLine, $"Return is not valid at line {_compiler.CurrentLine}");
            
            if (current is UserFunction yay)
            {
                function = yay;
                break;
            }

            current = current.Container;
        }
        
        _compiler = compiler;
        tokenHolder.AddCall(ParseReturn(token, tokenHolder, function));
    }
    
    private ReturnCall ParseReturn(string token, ITokenHolder holder, UserFunction function)
    {
        string[] split = token.SanitizedSplit(' ', 2, StringSplitOptions.RemoveEmptyEntries);
        string value = split[1].Trim();

        Input input = CompileUtils.HandleToken(value, function.ReturnType, holder, _compiler);
        return new ReturnCall(holder, _compiler.CurrentLine, input);
    }
}