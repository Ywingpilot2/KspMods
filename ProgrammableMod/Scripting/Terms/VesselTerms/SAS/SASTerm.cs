﻿using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using SteelLanguage.Exceptions;
using SteelLanguage.Token.Functions;
using SteelLanguage.Token.Interaction;

namespace ProgrammableMod.Scripting.Terms.VesselTerms.SAS;

internal class SASTerm : BaseComputerTerm
{
    public override string ValueType => "autopilot";
    
    [SuppressMessage("ReSharper", "HeapView.BoxingAllocation")]
    public override IEnumerable<IFunction> GetFunctions()
    {
        foreach (IFunction function in base.GetFunctions())
        {
            yield return function;
        }
        
        yield return new Function("enable_sas", _ =>
        {
            if (!Computer.vessel.Autopilot.Enabled)
                Computer.vessel.ActionGroups.SetGroup(KSPActionGroup.SAS, true);
        });
        yield return new Function("disable_sas" ,_ =>
        {
            if (Computer.vessel.Autopilot.Enabled)
                Computer.vessel.ActionGroups.SetGroup(KSPActionGroup.SAS, false);
        });
        yield return new Function("can_sas", "bool", terms =>
        {
            VesselAutopilot.AutopilotMode mode =
                (VesselAutopilot.AutopilotMode)KerbinSuperComputer.EnumFromInt(terms[0].CastToInt(),
                    typeof(VesselAutopilot.AutopilotMode));
            mode = CorrectMode(mode);

            return new ReturnValue(Computer.vessel.Autopilot.CanSetMode(mode), "bool");
        }, "sas_mode");
        yield return new Function("set_sas", terms =>
        {
            string sasType = terms[0].CastToStr();

            VesselAutopilot.AutopilotMode mode =
                (VesselAutopilot.AutopilotMode)KerbinSuperComputer.EnumFromInt(terms[0].CastToInt(),
                    typeof(VesselAutopilot.AutopilotMode));
            mode = CorrectMode(mode);

            if (!Computer.vessel.Autopilot.CanSetMode(mode))
                throw new InvalidActionException(0, $"Cannot set autopilot mode to {sasType} currently");

            // You can only set SAS once, if you spam set it over and over and over it will just do nothing
            // so to prevent this from happening, we just don't set it if its the same
            if (mode != Computer.vessel.Autopilot.Mode)
                Computer.vessel.Autopilot.SetMode(mode);
        }, "sas_mode");
    }

    /// <summary>
    /// This game is fucking stupid and has RadialIn and RadialOut flipped around
    /// </summary>
    private VesselAutopilot.AutopilotMode CorrectMode(VesselAutopilot.AutopilotMode mode)
    {
        if (mode == VesselAutopilot.AutopilotMode.RadialIn)
            return VesselAutopilot.AutopilotMode.RadialOut;

        if (mode == VesselAutopilot.AutopilotMode.RadialOut)
            return VesselAutopilot.AutopilotMode.RadialIn;

        return mode;
    }
}