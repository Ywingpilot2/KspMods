using SteelLanguage.Library;
using SteelLanguage.Reflection.Library;
using SteelLanguage.Token.KeyWords.Container;

namespace SteelLanguage.Token.KeyWords.Single;

internal record ImportKeyword : IKeyword
{
    public string Name => "import";
    public void CompileKeyword(string token, SteelCompiler compiler, SteelScript script, ITokenHolder tokenHolder)
    {
        LibraryManager maneger = script.GetLibraryManager();
        maneger.ImportLibrary(compiler.GetLibrary(token.Split(' ')[1].Trim()));
    }
}