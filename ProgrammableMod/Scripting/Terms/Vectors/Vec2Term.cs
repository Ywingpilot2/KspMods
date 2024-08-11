using System.Collections.Generic;
using SteelLanguage.Exceptions;
using SteelLanguage.Reflection.Type;
using SteelLanguage.Token.Fields;
using SteelLanguage.Token.Functions.Operator;
using SteelLanguage.Token.Functions.Single;
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
    }

    public override IEnumerable<TermField> GetFields()
    {
        foreach (TermField field in base.GetFields())
        {
            yield return field;
        }

        yield return new TermField("x", "float", _value.x);
        yield return new TermField("y", "float", _value.y);
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
                        return _value + new Vector2(i, i);
                    }
                    case float i:
                    {
                        return _value + new Vector2(i, i);
                    }
                    case Vector3 i:
                    {
                        return _value + (Vector2)i;
                    }
                    case Vector2 i:
                    {
                        return _value + i;
                    }
                    default:
                        throw new InvalidActionException(0, $"Cannot conduct math operation between {ValueType} and {subject.ValueType}");
                }
            }
            case MathOperatorKind.Subtract:
            {
                object value = subject.GetValue();
                switch (value)
                {
                    case int i:
                    {
                        return _value - new Vector2(i, i);
                    }
                    case float i:
                    {
                        return _value - new Vector2(i, i);
                    }
                    case Vector3 i:
                    {
                        return _value - (Vector2)i;
                    }
                    case Vector2 i:
                    {
                        return _value - i;
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

    public override object CastToType(string name)
    {
        if (name == "vec3")
            return (Vector3)_value;

        return base.CastToType(name);
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

        _value = ConfigNode.ParseVector2(node.GetValue("value"));
        return true;
    }
}