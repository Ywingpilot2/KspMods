using ActionScript.Exceptions;

namespace ActionScript.Token.Terms;

public class NullTerm : BaseTerm
{
    public override string ValueType => "null-type";
    public override bool Parse(string value)
    {
        throw new TypeNotConstructableException(0, "null-type");
    }

    public override bool CanImplicitCastToStr => true;

    public override bool SetValue(object value)
    {
        throw new System.NotImplementedException();
    }

    public override bool CopyFrom(BaseTerm term)
    {
        throw new System.NotImplementedException();
    }

    public override object GetValue()
    {
        return null;
    }
}