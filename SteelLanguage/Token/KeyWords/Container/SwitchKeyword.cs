using System;
using System.Collections.Generic;
using SteelLanguage.Exceptions;
using SteelLanguage.Extensions;
using SteelLanguage.Reflection;
using SteelLanguage.Token.Functions;
using SteelLanguage.Token.Functions.Conditional;
using SteelLanguage.Token.Interaction;
using SteelLanguage.Token.Terms;
using SteelLanguage.Utils;

namespace SteelLanguage.Token.KeyWords.Container;

public class SwitchKeyword : ContainerKeyword
{
    public override string Name => "switch";
    
    public override void CompileKeyword(string token, SteelCompiler compiler, SteelScript script, ITokenHolder tokenHolder)
    {
        string value = token.SanitizedSplit('(', 2)[1];
        value = value.Remove(value.Length - 1);

        TermType type = CompileUtils.GetTypeFromToken(value, tokenHolder, CompileUtils.GetTokenKind(value, tokenHolder));
        Input check = CompileUtils.HandleToken(value, "term", tokenHolder, compiler);

        TokenCall call = RegisterCases(type.Name, compiler, tokenHolder, check);
        tokenHolder.AddCall(call);
    }

    protected virtual TokenCall RegisterCases(string expectedType, SteelCompiler compiler, ITokenHolder holder, Input check)
    {
        int callLine = compiler.CurrentLine;
        string line = compiler.ReadCleanLine();
        while (line != "{")
        {
            line = compiler.ReadCleanLine();
        }

        line = compiler.ReadCleanLine();

        List<object> cases = new List<object>();
        List<SingleExecutableFunc> funcs = new List<SingleExecutableFunc>();

        SingleExecutableFunc def = null;
        SingleExecutableFunc func = new(holder);
        
        while (line != "}")
        {
            if (line == null)
                throw new FunctionLacksEndException(compiler.CurrentLine, null);

            if (line == "")
            {
                line = compiler.ReadCleanLine();
                continue;
            }

            if (line.StartsWith("default"))
            {
                ParseTokens(func, compiler, line);
                def = func;
            }

            if (line.StartsWith("case"))
            {
                string value = line.SanitizedSplit(' ', 2, StringSplitOptions.RemoveEmptyEntries,
                    ScanDirection.RightToLeft)[1];

                if (!CompileUtils.TokenIsConstant(value))
                    throw new TokenMustBeConstantException(compiler.CurrentLine, value);

                TermType type = CompileUtils.GetTypeFromConstant(value, holder);
                if (type.Name != expectedType && !type.CanImplicitCastTo(holder.GetLibraryManager().GetTermType(expectedType)))
                    throw new FunctionParamsInvalidException(compiler.CurrentLine, value, $"expected type was {expectedType}, instead got {type.Name}");

                BaseTerm term = type.Construct(Guid.NewGuid().ToString(), 0, holder.GetLibraryManager());
                term.Parse(value);

                ParseTokens(func, compiler);

                cases.Add(term.CastToType(expectedType));
                funcs.Add(func);
            }
            
            line = compiler.ReadCleanLine();
            func = new SingleExecutableFunc(holder);
        }
        
        return new SwitchCall(holder, callLine, check, cases, funcs, def);
    }
}