using System;
using ActionLanguage.Token.Interaction;
using ActionLanguage.Token.Terms;

namespace ActionLanguage.Token.Functions;

public class CastCall : TokenCall
{
    private Input _from;
    private string _type;
    
    public CastCall(ITokenHolder script, int line, Input from, string type) : base(script, line)
    {
        _from = from;
        _type = type;
    }

    public override ReturnValue Call()
    {
        try
        {
            BaseTerm term = _from.GetValue();
            return new ReturnValue(term.CastToType(_type), _type);
        }
        catch (Exception e)
        {
            return new ReturnValue();
        }
    }
}