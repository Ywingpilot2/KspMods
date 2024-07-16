using SteelLanguage.Exceptions;
using SteelLanguage.Reflection.Type;
using SteelLanguage.Token.Terms;

namespace SteelLanguage.Library.System.Terms.Literal;

public class NullTerm : BaseTerm
{
    public override string ValueType => "null-type";
    public override bool Parse(string value)
    {
        throw new TypeNotConstructableException(0, "null-type");
    }

    public override bool CanImplicitCastToStr => true;

    public override bool CanImplicitCastToType(TermType type)
    {
        if (type.IsNullable)
        {
            return true;
        }
        
        return base.CanImplicitCastToType(type);
    }

    public override bool SetValue(object value)
    {
        return true;
    }

    public override bool CopyFrom(BaseTerm term)
    {
        return true;
    }

    public override object GetValue()
    {
        return null;
    }
}