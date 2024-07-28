using System.Collections.Generic;
using SteelLanguage.Exceptions;
using SteelLanguage.Reflection.Type;
using SteelLanguage.Token.Fields;
using SteelLanguage.Token.Functions;
using SteelLanguage.Token.Functions.Operator;
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

    public override bool CanImplicitCastToType(TermType type)
    {
        if (type.Name == "vec3d")
            return true;
        
        return base.CanImplicitCastToType(type);
    }

    public override object CastToType(string name)
    {
        if (name == "vec3d")
            return (Vector3d)_value;

        if (name == "vec2")
            return (Vector2)_value;

        if (name == "vec2d")
            return (Vector2d)_value;
        
        return base.CastToType(name);
    }

    public override MathOperatorKind[] AllowedMathOps => new[]
    {
        MathOperatorKind.Add,
        MathOperatorKind.Subtract,
        MathOperatorKind.Multiply,
        MathOperatorKind.Divide
    };

    public override object ConductMath(MathOperatorKind kind, BaseTerm subject)
    {
        switch (kind)
        {
            case MathOperatorKind.Add:
            {
                object value = subject.GetValue();
                switch (value)
                {
                    case int i:
                    {
                        return _value + new Vector3(i, i, i);
                    }
                    case float i:
                    {
                        return _value + new Vector3(i, i, i);
                    }
                    case Vector3 i:
                    {
                        return _value + i;
                    }
                    case Vector2 i:
                    {
                        return _value + (Vector3)i;
                    }
                    default:
                        throw new InvalidActionException(0, $"Cannot conduct math operation between {ValueType} and {subject.ValueType}");
                }
            } break;
            case MathOperatorKind.Subtract:
            {
                object value = subject.GetValue();
                switch (value)
                {
                    case int i:
                    {
                        return _value - new Vector3(i, i, i);
                    }
                    case float i:
                    {
                        return _value - new Vector3(i, i, i);
                    }
                    case Vector3 i:
                    {
                        return _value - i;
                    }
                    case Vector2 i:
                    {
                        return _value - (Vector3)i;
                    }
                    default:
                        throw new InvalidActionException(0, $"Cannot conduct math operation between {ValueType} and {subject.ValueType}");
                }
            } break;
            case MathOperatorKind.Multiply:
            {
                object value = subject.GetValue();
                switch (value)
                {
                    case int i:
                    {
                        return _value * i;
                    }
                    case float i:
                    {
                        return _value * i;
                    }
                    default:
                        throw new InvalidActionException(0, $"Cannot conduct math operation between {ValueType} and {subject.ValueType}");
                }
            }
            case MathOperatorKind.Divide:
            {
                object value = subject.GetValue();
                switch (value)
                {
                    case int i:
                    {
                        return _value / i;
                    }
                    case float i:
                    {
                        return _value / i;
                    }
                    default:
                        throw new InvalidActionException(0, $"Cannot conduct math operation between {ValueType} and {subject.ValueType}");
                }
            }
        }
        
        throw new InvalidActionException(0, $"{kind} is not a supported math operator for {ValueType}");
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