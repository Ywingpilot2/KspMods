using SteelLanguage.Exceptions;
using SteelLanguage.Extensions;
using SteelLanguage.Token.Functions.Conditional;
using SteelLanguage.Token.Interaction;
using SteelLanguage.Utils;

namespace SteelLanguage.Token.KeyWords.Container;

public class WhileKeyword : ContainerKeyword
{
    public override string Name => "while";

    public override void CompileKeyword(string token, SteelCompiler compiler, SteelScript script, ITokenHolder holder)
    {
        string[] split = token.SanitizedSplit('(', 2);
        string prms = split[1].Trim();
        prms = prms.Remove(prms.Length - 1);

        Input condition = CompileUtils.HandleToken(prms, "bool", holder, compiler);

        WhileFunction whileFunction = new WhileFunction(condition, holder);
        ParseTokens(whileFunction, compiler);
        
        holder.AddCall(new WhileCall(holder, compiler.CurrentLine, whileFunction));
    }
}