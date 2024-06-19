using System.Collections.Generic;
using ActionLanguage.Library;
using ActionLanguage.Token.Interaction;
using ActionLanguage.Token.Terms;
using UnityEngine;

namespace ProgrammableMod.Scripting.Terms.Vectors;

public class Vec2Term : BaseTerm
{
    public override string ValueType => "vec2";

    public override IEnumerable<TermConstructor> GetConstructors()
    {
        foreach (TermConstructor constructor in base.GetConstructors())
        {
            yield return constructor;
        }

        yield return new TermConstructor(terms => new ReturnValue(new Vector2(terms[0].CastToFloat(), terms[1].CastToFloat()), "vec2"), 
            "float", "float");
        yield return new TermConstructor(terms => new ReturnValue(new Vector2(terms[0].CastToFloat(), terms[1].CastToFloat()), "vec2"), 
            "double", "double");
    }

    private Vector2 _value;
    public override bool SetValue(object value)
    {
        if (value is Vector2 vector2)
        {
            _value = vector2;
            return true;
        }

        return false;
    }

    public override bool CopyFrom(BaseTerm term)
    {
        if (term is Vec2Term vec2Term)
        {
            _value = vec2Term._value;
            return true;
        }

        return false;
    }

    public override object GetValue()
    {
        return _value;
    }
}