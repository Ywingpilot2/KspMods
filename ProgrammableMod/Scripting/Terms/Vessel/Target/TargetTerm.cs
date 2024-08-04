using System.Collections.Generic;
using SteelLanguage.Library.System.Terms.Literal;
using SteelLanguage.Token.Fields;
using SteelLanguage.Token.Terms;

namespace ProgrammableMod.Scripting.Terms.Vessel.Target;

internal class TargetTerm : BaseTerm
{
    public override string ValueType => "target";
    private ITargetable _targetable;

    public override IEnumerable<TermField> GetFields()
    {
        foreach (TermField field in base.GetFields())
        {
            yield return field;
        }

        yield return new TermField("name", "string", _targetable.GetName());
        yield return new TermField("orbit_velocity", "vec3", _targetable.GetObtVelocity());
        yield return new TermField("surface_velocity", "vec3", _targetable.GetSrfVelocity());
        yield return new TermField("forward", "vec3", _targetable.GetFwdVector());
        yield return new TermField("position", "vec3", _targetable.GetTransform().position);
        yield return new TermField("orbit", "Orbit", _targetable.GetOrbit());
    }

    public override bool SetValue(object value)
    {
        if (value is ITargetable targetable)
        {
            _targetable = targetable;
            return true;
        }

        if (value is null)
        {
            _targetable = null;
            return true;
        }

        return false;
    }

    public override bool CopyFrom(BaseTerm term)
    {
        if (term is TargetTerm targetTerm)
        {
            _targetable = targetTerm._targetable;
            return true;
        }

        if (term is NullTerm)
        {
            _targetable = null;
            return true;
        }

        return false;
    }

    public override object GetValue()
    {
        return _targetable;
    }
}