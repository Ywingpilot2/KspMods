using System;
using System.Collections.Generic;
using CommNet;
using ProgrammableMod.Modules.Computers;
using ProgrammableMod.Scripting.Exceptions;
using SteelLanguage.Token.Fields;
using SteelLanguage.Token.Functions;
using SteelLanguage.Token.Interaction;
using SteelLanguage.Token.Terms;
using UnityEngine;

namespace ProgrammableMod.Scripting.Terms.Vessel;

public class VesselTerm : BaseVesselTerm
{
    public override string ValueType => "vessel";
    private MylStagingManager _manager;

    #region Fields

    public override IEnumerable<TermField> GetFields()
    {
        foreach (TermField field in base.GetFields())
        {
            yield return field;
        }

        yield return new TermField("angular_velocity", "vec3", Computer.runTime);
        yield return new TermField("surf_velocity", "vec3", Computer.vessel != null ? Computer.vessel.srf_velocity : null);
        yield return new TermField("acceleration", "vec3", Computer.vessel != null ? Computer.vessel.acceleration : null);
        yield return new TermField("vertical_speed", "double", Computer.vessel != null ? Computer.vessel.verticalSpeed : 0.0);
        yield return new TermField("horizontal_speed", "double", Computer.vessel != null ? Computer.vessel.horizontalSrfSpeed : 0.0);
        yield return new TermField("total_mass", "double", Computer.vessel != null ? Computer.vessel.totalMass : 0.0);
        yield return new TermField("stage_manager", "staging", _manager);
        yield return new TermField("sas", "autopilot", Computer);
    }

    #endregion

    #region Functions

    public override IEnumerable<IFunction> GetFunctions()
    {
        foreach (IFunction function in base.GetFunctions())
        {
            yield return function;
        }
        
        yield return new Function("set_pitch", "void", terms =>
        {
            Computer.State.pitch = Mathf.Clamp(terms[0].CastToFloat(), -1.0F, 1.0F);
            return new ReturnValue();
        }, "float");
        yield return new Function("set_yaw", "void", terms =>
        {
            Computer.State.yaw = Mathf.Clamp(terms[0].CastToFloat(), -1.0F, 1.0F);
            return new ReturnValue();
        }, "float");
        yield return new Function("set_roll", "void", terms =>
        {
            Computer.State.roll = Mathf.Clamp(terms[0].CastToFloat(), -1.0F, 1.0F);
            return new ReturnValue();
        }, "float");
        yield return new Function("set_throttle", "void", terms =>
        {
            Computer.State.mainThrottle = Mathf.Clamp(terms[0].CastToFloat(), 0.0F, 1.0F);
            return new ReturnValue();
        }, "float");
        yield return new Function("toggle_action", "void", terms =>
        {
            int idx = terms[0].CastToInt();
            KSPActionGroup group = (KSPActionGroup)KerbinSuperComputer.EnumFromInt(idx + 3, typeof(KSPActionGroup));
            Computer.vessel.ActionGroups.ToggleGroup(group);

            return new ReturnValue();
        }, "action");
        yield return new Function("set_action", "void", terms =>
        {
            int idx = terms[0].CastToInt();
            bool value = terms[1].CastToBool();
            KSPActionGroup group = (KSPActionGroup)KerbinSuperComputer.EnumFromInt(idx + 3, typeof(KSPActionGroup));
            Computer.vessel.ActionGroups.SetGroup(group, value);

            return new ReturnValue();
        }, "action", "bool");
        yield return new Function("action_state", "bool", terms =>
        {
            int idx = terms[0].CastToInt();

            return new ReturnValue(Computer.vessel.ActionGroups.groups[idx + 1], "bool");
        }, "action");
    }

    #endregion

    public override bool CopyFrom(BaseTerm term)
    {
        bool t = base.CopyFrom(term);
        if (t)
        {
            _manager = new MylStagingManager(Computer);
        }

        return t;
    }
}