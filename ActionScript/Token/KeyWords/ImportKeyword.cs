using ActionLanguage.Library;

namespace ActionLanguage.Token.KeyWords;

public struct ImportKeyword : IKeyword
{
    public string Name => "import";
    public void CompileKeyword(string token, ActionCompiler compiler, ActionScript script, ITokenHolder tokenHolder)
    {
        LibraryManager maneger = script.GetLibraryManager();
        maneger.ImportLibrary(compiler.GetLibrary(token.Split(' ')[1].Trim()));
    }
}