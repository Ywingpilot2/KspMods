using System.Linq;
using SteelLanguage.Exceptions;
using SteelLanguage.Extensions;
using SteelLanguage.Token.Functions.Conditional;
using SteelLanguage.Token.Interaction;
using SteelLanguage.Utils;

namespace SteelLanguage.Token.KeyWords.Container;

public class IfKeyword : ContainerKeyword
{
    public override string Name => "if";
    
    public override void CompileKeyword(string token, SteelCompiler compiler, SteelScript script, ITokenHolder tokenHolder)
    {
        string value = token.SanitizedSplit('(', 2)[1];
        value = value.Remove(value.Length - 1);
        Input input = CompileUtils.HandleToken(value, "bool", tokenHolder, compiler);

        IfCall ifCall = new IfCall(tokenHolder, compiler.CurrentLine, input);
        ParseTokens(ifCall.ExecutableFunc, compiler);
        tokenHolder.AddCall(ifCall);
    }
}

public class ElseIfKeyword : IfKeyword
{
    public override string Name => "elif";

    public override void CompileKeyword(string token, SteelCompiler compiler, SteelScript script, ITokenHolder tokenHolder)
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
    
    public override void CompileKeyword(string token, SteelCompiler compiler, SteelScript script, ITokenHolder tokenHolder)
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