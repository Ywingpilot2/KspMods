using System;
using System.Collections.Generic;
using ActionLanguage.Exceptions;
using ActionLanguage.Token.Functions;
using ActionLanguage.Token.Interaction;
using CommNet;
using ProgrammableMod.Scripting.Exceptions;

namespace ProgrammableMod.Scripting.Terms.Vessel;

public class SASTerm : BaseVesselTerm
{
    public override string ValueType => "autopilot";

    public override IEnumerable<IFunction> GetFunctions()
    {
        foreach (IFunction function in base.GetFunctions())
        {
            yield return function;
        }
        
        yield return new Function("enable_sas", "void", _ =>
        {
            if (Computer.vessel.Connection.ControlState != VesselControlState.ProbeFull
                && Computer.vessel.Connection.ControlState != VesselControlState.KerbalFull)
                throw new ControlLostException(0);

            if (!Computer.vessel.Autopilot.Enabled)
                Computer.vessel.Autopilot.Enable();

            return new ReturnValue();
        });
        yield return new Function("disable_sas", "void", _ =>
        {
            if (Computer.vessel.Autopilot.Enabled)
                Computer.vessel.Autopilot.Disable();

            return new ReturnValue();
        });
        yield return new Function("can_sas", "bool", terms => new ReturnValue(
            Computer.vessel.Autopilot.CanSetMode(
                (VesselAutopilot.AutopilotMode)Enum.Parse(typeof(VesselAutopilot.AutopilotMode),
                    terms[0].CastToStr())), "bool"), "string");
        yield return new Function("set_sas", "void", terms =>
        {
            string sasType = terms[0].CastToStr();

            // TODO: enums
            VesselAutopilot.AutopilotMode mode =
                (VesselAutopilot.AutopilotMode)Enum.Parse(typeof(VesselAutopilot.AutopilotMode), sasType);
            if (!Computer.vessel.Autopilot.CanSetMode(mode))
                throw new InvalidActionException(0, $"Cannot set autopilot mode to {sasType} currently");

            // You can only set SAS once, if you spam set it over and over and over it will just do nothing
            // so to prevent this from happening, we just don't set it if its the same
            if (mode != Computer.vessel.Autopilot.Mode)
                Computer.vessel.Autopilot.SetMode(mode);

            return new ReturnValue();
        }, "string");
    }
}