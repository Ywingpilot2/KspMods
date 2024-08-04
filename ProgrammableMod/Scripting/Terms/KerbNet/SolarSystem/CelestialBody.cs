using System.Collections.Generic;
using SteelLanguage.Library.System.Terms.Literal;
using SteelLanguage.Token.Fields;
using SteelLanguage.Token.Terms;

namespace ProgrammableMod.Scripting.Terms.KerbNet.SolarSystem;

internal class CelestialBodyTerm : BaseTerm
{
    public override string ValueType => "CelestialBody";
    private CelestialBody _celestialBody;

    public override IEnumerable<TermField> GetFields()
    {
        foreach (TermField field in base.GetFields())
        {
            yield return field;
        }

        yield return new TermField("name", "string", _celestialBody != null ? _celestialBody.GetName() : null);
        yield return new TermField("has_atmo", "bool", _celestialBody != null ? _celestialBody.atmosphere : null);
        yield return new TermField("has_ocean", "bool", _celestialBody != null ? _celestialBody.ocean : null);
        yield return new TermField("atm_density_asl", "float", _celestialBody != null ? _celestialBody.atmDensityASL : null);
        yield return new TermField("atm_pressure_asl", "float", _celestialBody != null ? _celestialBody.atmPressureASL : null);
        yield return new TermField("soi", "double", _celestialBody != null ? _celestialBody.sphereOfInfluence : null);
        yield return new TermField("orbit", "Orbit", _celestialBody != null ? _celestialBody.orbit : null);
    }

    public override bool SetValue(object value)
    {
        if (value is CelestialBody celestialBody)
        {
            _celestialBody = celestialBody;
            return true;
        }
        
        if (value is null)
        {
            _celestialBody = null;
            return true;
        }
        
        return false;
    }

    public override bool CopyFrom(BaseTerm term)
    {
        if (term is CelestialBodyTerm celest)
        {
            _celestialBody = celest._celestialBody;
            return true;
        }

        if (term is NullTerm)
        {
            _celestialBody = null;
            return true;
        }
        
        return false;
    }

    public override object GetValue()
    {
        return _celestialBody;
    }
}