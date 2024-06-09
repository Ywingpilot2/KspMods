using ActionScript.Library;
using ActionScript.Token.Functions;
using ActionScript.Token.KeyWords;
using ActionScript.Token.Terms;

namespace ActionScript.Token;

public interface IFunctionHolder
{
    public IFunction GetFunction(string name);
    public bool HasFunction(string name);
    public void AddFunc(IFunction function);
}

/// <summary>
/// Base implementation of an interface capable of handling tokens and their types
/// </summary>
public interface ITokenHolder : IFunctionHolder
{
    public BaseTerm GetTerm(string name);
    public bool HasTerm(string name);

    public void AddCall(TokenCall call);
    public void AddTerm(BaseTerm term);

    public bool TermTypeExists(string name);
    public TermType GetTermType(string name);

    public bool HasKeyword(string name);
    public IKeyword GetKeyword(string name);
}