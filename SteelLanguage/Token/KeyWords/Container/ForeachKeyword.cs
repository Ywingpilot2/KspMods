using System;
using SteelLanguage.Extensions;
using SteelLanguage.Reflection;
using SteelLanguage.Token.Functions.Conditional;
using SteelLanguage.Token.Interaction;
using SteelLanguage.Token.Terms;
using SteelLanguage.Utils;

namespace SteelLanguage.Token.KeyWords.Container;

public class ForeachKeyword : ContainerKeyword
{
    public override string Name => "foreach";

    private SteelCompiler _compiler;
    public override void CompileKeyword(string token, SteelCompiler compiler, SteelScript script, ITokenHolder tokenHolder)
    {
        _compiler = compiler;

        string prms = token.SanitizedSplit('(', 2, StringSplitOptions.RemoveEmptyEntries)[1];
        prms = prms.Remove(prms.Length - 1);

        string[] split = prms.SplitAt(prms.SanitizedIndexOf(" in "), 2, StringSplitOptions.RemoveEmptyEntries);
        string termToken = split[0].Trim();
        string inputToken = split[1].Remove(0, 2).Trim();
        
        Input input = CompileUtils.HandleToken(inputToken, "enumerable", tokenHolder, compiler);

        string[] typeName = termToken.Split(new []{' '}, 2, StringSplitOptions.RemoveEmptyEntries);
        TermType type = tokenHolder.GetLibraryManager().GetTermType(typeName[0].Trim());
        string name = typeName[1].Trim();
        
        ForeachFunc func = new ForeachFunc(tokenHolder, input, name);

        BaseTerm term = type.Construct(name, compiler.CurrentLine, script.GetLibraryManager());
        func.AddTerm(term);

        ParseTokens(func, compiler);
        
        tokenHolder.AddCall(new ForeachCall(tokenHolder, compiler.CurrentLine, func));
    }
}