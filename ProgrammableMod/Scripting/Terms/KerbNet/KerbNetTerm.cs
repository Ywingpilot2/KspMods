using System.Collections.Generic;
using ProgrammableMod.Scripting.Exceptions;
using SteelLanguage.Exceptions;
using SteelLanguage.Token.Fields;
using SteelLanguage.Token.Functions;
using SteelLanguage.Token.Interaction;
using UnityEngine;

namespace ProgrammableMod.Scripting.Terms.KerbNet;

internal class KerbNetTerm : BaseComputerTerm
{
    public override string ValueType => "commnet";
    private SuperComputerTerm _kerfer;

    public override IEnumerable<TermField> GetFields()
    {
        foreach (TermField field in base.GetFields())
        {
            yield return field;
        }

        if (HighLogic.LoadedSceneIsFlight)
        {
            EstablishConnection();
        }

        yield return new TermField("time", "float", HighLogic.CurrentGame.UniversalTime);
        yield return new TermField("super_computer", "kerfur", _kerfer);
    }

    public override IEnumerable<IFunction> GetFunctions()
    {
        foreach (IFunction function in base.GetFunctions())
        {
            yield return function;
        }

        yield return new Function("enumerate_bodies", EnumerateBodies, "CelestialBody");
        yield return new Function("get_body", "CelestialBody", terms =>
        {
            string name = terms[0].CastToStr();
            foreach (CelestialBody celestialBody in FlightGlobals.Bodies)
            {
                if (celestialBody.GetName() == name)
                    return new ReturnValue(celestialBody, "CelestialBody");
            }

            throw new InvalidActionException(0, $"Could not find body of name {name}");
        }, "string");
    }

    private IEnumerable<ReturnValue> EnumerateBodies()
    {
        foreach (CelestialBody body in FlightGlobals.Bodies)
        {
            yield return new ReturnValue(body, "CelestialBody");
        }
    }

    protected override void ExtraBuilding()
    {
        _kerfer = new SuperComputerTerm();
    }
}