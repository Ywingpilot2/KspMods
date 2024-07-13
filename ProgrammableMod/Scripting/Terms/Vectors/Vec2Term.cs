using System.Collections.Generic;
using SteelLanguage.Reflection;
using SteelLanguage.Token.Interaction;
using SteelLanguage.Token.Terms;
using UnityEngine;

namespace ProgrammableMod.Scripting.Terms.Vectors;

public class Vec2Term : BaseTerm, IStashableTerm
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

    public bool Save(ConfigNode node)
    {
        node.AddValue("value", ConfigNode.WriteVector(_value));
        return true;
    }

    public bool Load(ConfigNode node)
    {
        if (node.HasValue("value"))
            return false;

        _value = ConfigNode.ParseVector3(node.GetValue("value"));
        return true;
    }
}