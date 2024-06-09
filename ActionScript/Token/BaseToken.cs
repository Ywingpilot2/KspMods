using ActionScript.Exceptions;
using ActionScript.Token.Functions;
using ActionScript.Token.Terms;

namespace ActionScript.Token;

public class BaseToken : IToken
{
    protected ITokenHolder Script;
    public int Line { get; set; }

    public BaseTerm GetTerm(string name) => Script.GetTerm(name);

    public IFunction GetFunc(string name) => Script.GetFunction(name);

    public BaseToken(ITokenHolder script, int line)
    {
        Script = script;
        Line = line;
    }
}