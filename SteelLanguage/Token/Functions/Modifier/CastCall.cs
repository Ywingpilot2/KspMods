using System;
using SteelLanguage.Token.Interaction;
using SteelLanguage.Token.Terms;

namespace SteelLanguage.Token.Functions.Modifier;

public class CastCall : TokenCall
{
    private readonly Input _from;
    private readonly string _type;
    
    public CastCall(ITokenHolder container, int line, Input from, string type) : base(container, line)
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
            return ReturnValue.None;
        }
    }

    public override void PostCompilation()
    {
        _from.PostCompilation();
    }
}