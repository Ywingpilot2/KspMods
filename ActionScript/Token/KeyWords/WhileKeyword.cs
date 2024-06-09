using System;
using ActionScript.Exceptions;
using ActionScript.Extensions;
using ActionScript.Library;
using ActionScript.Token.Functions;
using ActionScript.Token.Interaction;
using ActionScript.Token.Terms;

namespace ActionScript.Token.KeyWords;

public struct WhileKeyword : IKeyword
{
    public string Name => "while";

    private ActionCompiler _compiler;
    private ActionScript _script;

    public void CompileKeyword(string token, ActionCompiler compiler, ActionScript script, ITokenHolder holder)
    {
        _compiler = compiler;
        _script = script;

        string[] split = token.SmartSplit('(', 2);
        string prms = split[1].Trim();
        prms = prms.Remove(prms.Length - 1);

        Input condition;
        if (prms.EndsWith(")"))
        {
            FunctionCall functionCall = _compiler.ParseFunctionCall(prms, holder);
            condition = new Input(holder, functionCall);
        }
        else if (holder.HasTerm(prms))
        {
            BaseTerm term = holder.GetTerm(prms);
            condition = new Input(term);
        }
        else
        {
            TermType boolType = _script.GetTermType("bool");
            BaseTerm term = boolType.Construct(Guid.NewGuid().ToString(), compiler.CurrentLine);
            if (!term.Parse(prms))
                throw new InvalidCompilationException(_compiler.CurrentLine, $"Condition in while statement is not a valid bool");
            condition = new Input(term);
        }

        WhileFunction whileFunction = new WhileFunction(condition, holder);
        ParseWhileTokens(token, whileFunction);
        
        holder.AddCall(new WhileCall(holder, compiler.CurrentLine, whileFunction));
    }

    private void ParseWhileTokens(string whileToken, WhileFunction whileFunction)
    {
        string line = _compiler.ReadCleanLine();
        while (line != "{")
        {
            line = _compiler.ReadCleanLine();
            if (line == null)
                throw new FunctionLacksEndException(_compiler.CurrentLine, whileToken);
        }

        line = _compiler.ReadCleanLine();
        while (line != "}")
        {
            if (line == null)
                throw new FunctionLacksEndException(_compiler.CurrentLine, whileToken);

            if (line == "")
            {
                line = _compiler.ReadCleanLine();
                continue;
            }

            if (line.StartsWith("break"))
            {
                whileFunction.AddCall(new BreakCall(whileFunction, _compiler.CurrentLine));
            }
            else if (line.StartsWith("continue"))
            {
                whileFunction.AddCall(new ContinueCall(whileFunction, _compiler.CurrentLine));
            }
            else
            {
                _compiler.ParseToken(line, whileFunction);
            }
            
            line = _compiler.ReadCleanLine();
        }
    }
}