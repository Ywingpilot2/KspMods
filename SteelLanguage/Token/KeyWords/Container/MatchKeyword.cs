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

public class MatchKeyword : SwitchKeyword
{
    public override string Name => "match";
    
    protected override TokenCall RegisterCases(string expectedType, SteelCompiler compiler, ITokenHolder holder, Input check)
    {
        int callLine = compiler.CurrentLine;
        string line = compiler.ReadCleanLine();
        while (line != "{")
        {
            line = compiler.ReadCleanLine();
        }

        line = compiler.ReadCleanLine();

        List<Input> cases = new List<Input>();
        List<SingleExecutableFunc> funcs = new List<SingleExecutableFunc>();

        SingleExecutableFunc def = null;
        SingleExecutableFunc func = new SingleExecutableFunc(holder);
        
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

                Input input = CompileUtils.HandleToken(value, expectedType, holder, compiler);

                ParseTokens(func, compiler);

                cases.Add(input);
                funcs.Add(func);
            }
            
            line = compiler.ReadCleanLine();
            func = new SingleExecutableFunc(holder);
        }

        return new MatchCall(holder, callLine, check, cases, funcs, def);
    }
}