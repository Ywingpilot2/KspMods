using System.Collections.Generic;
using SteelLanguage.Library.System.Terms.Literal;
using SteelLanguage.Token.Fields;
using SteelLanguage.Token.Functions;
using SteelLanguage.Token.Interaction;
using SteelLanguage.Token.Terms;

namespace ProgrammableMod.Scripting.Terms.KerbNet.SolarSystem;

internal class OrbitTerm : BaseTerm
{
    public override string ValueType => "Orbit";
    private Orbit _orbit;

    public override IEnumerable<TermField> GetFields()
    {
        foreach (TermField field in base.GetFields())
        {
            yield return field;
        }

        yield return new TermField("altitude", "double", _orbit?.altitude);
        yield return new TermField("speed", "double", _orbit?.orbitalSpeed);
        yield return new TermField("percent", "double", _orbit?.orbitPercent);
        yield return new TermField("prev_patch", "Orbit", _orbit?.previousPatch);
        yield return new TermField("next_patch", "Orbit", _orbit != null && _orbit.nextPatch.activePatch ? _orbit.nextPatch : null);
        yield return new TermField("orbiting_body", "CelestialBody", _orbit?.referenceBody);
        yield return new TermField("closest_body", "CelestialBody", _orbit?.closestEncounterBody);
        yield return new TermField("closest_patch", "Orbit", _orbit?.closestEncounterPatch);
        yield return new TermField("apoapsis", "double", _orbit?.ApR);
        yield return new TermField("periapsis", "double", _orbit?.PeR);
        yield return new TermField("velocity", "vec3d", _orbit?.vel);
        yield return new TermField("pe_arg", "double", _orbit?.argumentOfPeriapsis);
        yield return new TermField("eccentricity", "double", _orbit?.eccentricity);
        yield return new TermField("time_since_pe", "double", _orbit?.ObT);
        yield return new TermField("time_to_pe", "double", _orbit?.timeToPe);
        yield return new TermField("time_to_ap", "double", _orbit?.timeToAp);
        yield return new TermField("inclination", "double", _orbit?.inclination);
    }

    public override IEnumerable<IFunction> GetFunctions()
    {
        foreach (IFunction function in base.GetFunctions())
        {
            yield return function;
        }

        yield return new Function("speed_when", "double",
            terms => new ReturnValue(_orbit.getOrbitalSpeedAt(terms[0].CastToDouble()), "double"));
        yield return new Function("velocity_when", "vec3d",
            terms => new ReturnValue(_orbit.getOrbitalVelocityAtUT(terms[0].CastToDouble()), "double"));
    }

    public override bool SetValue(object value)
    {
        if (value is Orbit orbit)
        {
            _orbit = orbit;
            return true;
        }
        
        if (value is null)
        {
            _orbit = null;
            return true;
        }
        
        return false;
    }

    public override bool CopyFrom(BaseTerm term)
    {
        if (term is OrbitTerm orbit)
        {
            _orbit = orbit._orbit;
            return true;
        }

        if (term is NullTerm)
        {
            _orbit = null;
            return true;
        }
        
        return false;
    }

    public override object GetValue()
    {
        return _orbit;
    }
}