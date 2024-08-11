using System;
using SteelLanguage.Exceptions;
using SteelLanguage.Extensions;
using SteelLanguage.Reflection.Type;
using SteelLanguage.Token.Functions.Conditional;
using SteelLanguage.Token.Interaction;
using SteelLanguage.Token.Terms;
using SteelLanguage.Utils;

namespace SteelLanguage.Token.KeyWords.Container;

public class ForeachKeyword : ContainerKeyword
{
    public override string Name => "foreach";

    public override void CompileKeyword(string token, SteelCompiler compiler, SteelScript script, ITokenHolder tokenHolder)
    {
        string prms = token.SanitizedSplit('(', 2, StringSplitOptions.RemoveEmptyEntries)[1];
        prms = prms.Remove(prms.Length - 1);

        string[] split = prms.SplitAt(prms.SanitizedIndexOf(" in "), 2, StringSplitOptions.RemoveEmptyEntries);
        string termToken = split[0].Trim();
        string inputToken = split[1].Remove(0, 2).Trim();

        TermType enumerableType = CompileUtils.GetTypeFromToken(inputToken, tokenHolder, CompileUtils.GetTokenKind(inputToken, tokenHolder));
        if (enumerableType.ShortName != "Enumerable" && !enumerableType.IsSubclassOf("Enumerable"))
            throw new InvalidParametersException(compiler.CurrentLine, "Enumerable");
        
        Input input = CompileUtils.HandleToken(inputToken, enumerableType.Name, tokenHolder, compiler);

        string[] typeName = termToken.Split(new []{' '}, 2, StringSplitOptions.RemoveEmptyEntries);
        TermType type = tokenHolder.GetLibraryManager().GetTermType(typeName[0].Trim());
        string name = typeName[1].Trim();
        
        ForeachFunc func = new ForeachFunc(tokenHolder, input, name);

        TermHolder termHolder = new TermHolder(type.Name) { Name = name };
        func.AddHolder(termHolder);

        ParseTokens(func, compiler);
        
        tokenHolder.AddCall(new ForeachCall(tokenHolder, compiler.CurrentLine, func));
    }
}