using System.Collections.Generic;
using ProgrammableMod.Scripting.Exceptions;
using SteelLanguage.Token.Fields;
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

        yield return new TermField("time", "float", Time.fixedTime);
        yield return new TermField("has_access", "bool", Computer.vessel != null && Computer.vessel.Connection.IsConnected);
        yield return new TermField("super_computer", "kerfur", _kerfer);
    }

    protected override void ExtraBuilding()
    {
        _kerfer = new SuperComputerTerm();
    }

    /// <summary>
    /// Connect to the <see cref="KerbinSuperComputer"/>!
    /// Warranty void if connection unstable.
    /// </summary>
    private void EstablishConnection()
    {
        if (!Computer.vessel.Connection.IsConnected && !Computer.compiling)
            throw new KerbnetLostException(0);
    }
}