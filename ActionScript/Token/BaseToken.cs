using ActionScript.Exceptions;
using ActionScript.Functions;
using ActionScript.Terms;

namespace ActionScript.Token;

public class BaseToken : IToken
{
    protected ActionScript Script;
    public int Line { get; set; }
    
    public BaseTerm GetTerm(string name)
    {
        if (!Script.Terms.ContainsKey(name))
            return null;
        
        return Script.Terms[name];
    }

    public Function GetFunc(string name) => Script.GetFunction(name);

    public BaseToken(ActionScript script, int line)
    {
        Script = script;
        Line = line;
    }
}