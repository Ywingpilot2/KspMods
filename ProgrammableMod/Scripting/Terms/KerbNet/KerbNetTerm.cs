using System.Collections.Generic;
using ProgrammableMod.Scripting.Exceptions;
using SteelLanguage.Token.Fields;
using UnityEngine;

namespace ProgrammableMod.Scripting.Terms.KerbNet;

public class KerbNetTerm : BaseVesselTerm
{
    public override string ValueType => "commnet";

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

        yield return new TermField("time", "float", Time.fixedTime);
        yield return new TermField("altitude", "double", Computer.vessel != null ? Computer.vessel.altitude : 0.0);
        yield return new TermField("ground_dist", "float", Computer.vessel != null ? Computer.vessel.heightFromTerrain : 0.0f);
        yield return new TermField("atmo_density", "double", Computer.vessel != null ? Computer.vessel.atmDensity : 0.0);
        yield return new TermField("has_access", "bool", Computer.vessel != null && Computer.vessel.Connection.IsConnected);
    }

    /// <summary>
    /// Connect to the <see cref="KerbinSuperComputer"/>!
    /// Warranty void if connection unstable.
    /// </summary>
    private void EstablishConnection()
    {
        if (!Computer.vessel.Connection.IsConnected && !Computer.Compiling)
            throw new KerbnetLostException(0);
    }
}