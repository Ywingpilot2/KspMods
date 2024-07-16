﻿using System.Collections.Generic;
using SteelLanguage.Token.Fields;
using SteelLanguage.Token.Functions.Single;
using SteelLanguage.Token.Interaction;
using SteelLanguage.Token.Terms;
using UnityEngine;

namespace ProgrammableMod.Scripting.Terms.Vectors;

public class Vec3Term : BaseTerm, IStashableTerm
{
    public override string ValueType => "vec3";
    
    public override IEnumerable<TermConstructor> GetConstructors()
    {
        foreach (TermConstructor constructor in base.GetConstructors())
        {
            yield return constructor;
        }

        yield return new TermConstructor(terms => new ReturnValue(new Vector3(terms[0].CastToFloat(), terms[1].CastToFloat(), terms[2].CastToFloat()), "vec3"), 
            "float", "float", "float");
        yield return new TermConstructor(terms => new ReturnValue(new Vector3(terms[0].CastToFloat(), terms[1].CastToFloat(), terms[2].CastToFloat()), "vec3"), 
            "double", "double", "double");
    }

    public override IEnumerable<TermField> GetFields()
    {
        foreach (TermField field in base.GetFields())
        {
            yield return field;
        }

        yield return new TermField("x", "float", _value.x);
        yield return new TermField("y", "float", _value.y);
        yield return new TermField("z", "float", _value.z);
    }

    private Vector3 _value;
    public override bool SetValue(object value)
    {
        if (value is Vector3 vector3)
        {
            _value = vector3;
            return true;
        }

        return false;
    }

    public override bool CopyFrom(BaseTerm term)
    {
        if (term is Vec3Term vec3Term)
        {
            _value = vec3Term._value;
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