using SteelLanguage.Reflection.Type;
using SteelLanguage.Token.Functions;
using SteelLanguage.Token.Terms;

namespace SteelLanguage.Token;

public abstract class BaseToken : IToken
{
    protected readonly ITokenHolder Container;
    public int Line { get; set; }

    public BaseTerm GetTerm(string name) => Container.GetTerm(name);
    public TermType GetTermType(string name) => Container.GetLibraryManager().GetTermType(name);
    public IFunction GetFunc(string name) => Container.GetFunction(name);

    protected BaseToken(ITokenHolder container, int line)
    {
        Container = container;
        Line = line;
    }
}