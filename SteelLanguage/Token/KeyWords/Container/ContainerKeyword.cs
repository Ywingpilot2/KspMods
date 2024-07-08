using SteelLanguage.Exceptions;
using SteelLanguage.Token.Functions;

namespace SteelLanguage.Token.KeyWords.Container;

public abstract class ContainerKeyword : IKeyword
{
    public abstract string Name { get; }
    public abstract void CompileKeyword(string token, SteelCompiler compiler, SteelScript script,
        ITokenHolder tokenHolder);
    
    protected void ParseTokens(BaseExecutable func, SteelCompiler compiler, string token = null)
    {
        string line;
        while (true)
        {
            line = compiler.ReadCleanLine();
            if (line == "")
                continue;
            
            if (line == null)
                throw new FunctionLacksEndException(compiler.CurrentLine, token);
            
            if (line == "{")
                break;
            
            compiler.ParseToken(line, func);
            return;
        }

        line = compiler.ReadCleanLine();
        while (line != "}")
        {
            if (line == null)
                throw new FunctionLacksEndException(compiler.CurrentLine, token);

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