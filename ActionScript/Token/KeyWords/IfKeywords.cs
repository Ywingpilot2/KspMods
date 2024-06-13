using System.Linq;
using ActionLanguage.Exceptions;
using ActionLanguage.Extensions;
using ActionLanguage.Token.Functions;
using ActionLanguage.Token.Interaction;
using ActionLanguage.Utils;

namespace ActionLanguage.Token.KeyWords;

public class IfKeyword : IKeyword
{
    public virtual string Name => "if";
    
    public virtual void CompileKeyword(string token, ActionCompiler compiler, ActionScript script, ITokenHolder tokenHolder)
    {
        string value = token.SanitizedSplit('(', 2)[1];
        value = value.Remove(value.Length - 1);
        Input input = CompileUtils.HandleToken(value, "bool", tokenHolder, compiler);

        IfCall ifCall = new IfCall(tokenHolder, compiler.CurrentLine, input);
        ParseTokens(ifCall.ExecutableFunc, compiler);
        tokenHolder.AddCall(ifCall);
    }

    protected void ParseTokens(SingleExecutableFunc func, ActionCompiler compiler)
    {
        string line = compiler.ReadCleanLine();
        while (line != "{")
        {
            line = compiler.ReadCleanLine();
            if (line == null)
                throw new FunctionLacksEndException(compiler.CurrentLine, null);
        }

        line = compiler.ReadCleanLine();
        while (line != "}")
        {
            if (line == null)
                throw new FunctionLacksEndException(compiler.CurrentLine, null);

            if (line == "")
            {
                line = compiler.ReadCleanLine();
                continue;
            }

            compiler.ParseToken(line, func);
            
            line = compiler.ReadCleanLine();
        }
    }
}

public class ElseIfKeyword : IfKeyword
{
    public override string Name => "elif";

    public override void CompileKeyword(string token, ActionCompiler compiler, ActionScript script, ITokenHolder tokenHolder)
    {
        if (tokenHolder.EnumerateCalls().LastOrDefault() is not IConditionalCall call) // TODO: This is a really slow way of doing this lol
            throw new ElseBranchMissingRootException(compiler.CurrentLine);
        
        string value = token.SanitizedSplit('(', 2)[1];
        value = value.Remove(value.Length - 1);
        Input input = CompileUtils.HandleToken(value, "bool", tokenHolder, compiler);

        IfCall ifCall = new IfCall(tokenHolder, compiler.CurrentLine, input);
        ParseTokens(ifCall.ExecutableFunc, compiler);
        call.GetLastBranch().Else = ifCall;
    }
}

public class ElseKeyword : IfKeyword
{
    public override string Name => "else";
    
    public override void CompileKeyword(string token, ActionCompiler compiler, ActionScript script, ITokenHolder tokenHolder)
    {
        if (tokenHolder.EnumerateCalls().LastOrDefault() is not IConditionalCall call) // TODO: This is a really slow way of doing this lol
            throw new ElseBranchMissingRootException(compiler.CurrentLine);

        ElseCall elseCall = new ElseCall(tokenHolder, compiler.CurrentLine);
        ParseTokens(elseCall.ExecutableFunc, compiler);

        if (call.GetLastBranch() is ElseCall)
            throw new AlreadyHasElseBranchException(compiler.CurrentLine);

        call.GetLastBranch().Else = elseCall;
    }
}