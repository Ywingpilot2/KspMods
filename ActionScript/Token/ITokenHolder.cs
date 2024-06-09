using ActionScript.Functions;
using ActionScript.Library;
using ActionScript.Terms;

namespace ActionScript.Token;

/// <summary>
/// Base implementation of an interface capable of handling tokens and their types
/// </summary>
public interface ITokenHolder
{
    public IFunction GetFunction(string name);
    public bool HasFunction(string name);
    
    public BaseTerm GetTerm(string name);
    public bool HasTerm(string name);

    public void AddCall(TokenCall call);
    public void AddTerm(BaseTerm term);
    public void AddFunc(IFunction function);

    public bool TermTypeExists(string name);
    public TermType GetTermType(string name);
}