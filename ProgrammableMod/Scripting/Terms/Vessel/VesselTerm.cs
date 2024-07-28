using System;
using System.Collections.Generic;
using CommNet;
using ProgrammableMod.Modules.Computers;
using ProgrammableMod.Scripting.Exceptions;
using ProgrammableMod.Scripting.Terms.Vessel.ActionGroups;
using SteelLanguage.Token.Fields;
using SteelLanguage.Token.Functions;
using SteelLanguage.Token.Interaction;
using SteelLanguage.Token.Terms;
using UnityEngine;

namespace ProgrammableMod.Scripting.Terms.Vessel;

public class VesselTerm : BaseComputerTerm
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
        
        yield return new TermField("orbit_velocity", "vec3d", Computer.vessel != null ? Computer.vessel.obt_velocity : null);
        yield return new TermField("surf_velocity", "vec3d", Computer.vessel != null ? Computer.vessel.srf_velocity : null);
        yield return new TermField("acceleration", "vec3d", Computer.vessel != null ? Computer.vessel.acceleration : null);
        yield return new TermField("vertical_speed", "double", Computer.vessel != null ? Computer.vessel.verticalSpeed : 0.0);
        yield return new TermField("horizontal_speed", "double", Computer.vessel != null ? Computer.vessel.horizontalSrfSpeed : 0.0);
        yield return new TermField("total_mass", "double", Computer.vessel != null ? Computer.vessel.totalMass : 0.0);
        yield return new TermField("stage_manager", "staging", _manager);
        yield return new TermField("sas", "autopilot", Computer);
        
        // navigation
        yield return new TermField("throttle", "float", Computer.State.mainThrottle, true);
        yield return new TermField("yaw", "float", Computer.State.yaw, true);
        yield return new TermField("pitch", "float", Computer.State.pitch, true);
        yield return new TermField("roll", "float", Computer.State.roll, true);
        
        // rcs
        yield return new TermField("rcs_x", "float", Computer.State.X, true);
        yield return new TermField("rcs_y", "float", Computer.State.Y, true);
        yield return new TermField("rcs_z", "float", Computer.State.Z, true);

        yield return new TermField("position", "vec3", Computer.vessel.GetTransform().position);
        yield return new TermField("target", "target", Computer.vessel.targetObject);
    }

    public override bool SetField(string name, object value)
    {
        switch (name)
        {
            case "throttle":
            {
                Computer.State.mainThrottle = (float)value;
                return true;
            }
            case "yaw":
            {
                Computer.State.yaw = (float)value;
                return true;
            }
            case "pitch":
            {
                Computer.State.pitch = (float)value;
                return true;
            }
            case "roll":
            {
                Computer.State.roll = (float)value;
                return true;
            }
            case "rcs_x":
            {
                Computer.State.X = (float)value;
                return true;
            }
            case "rcs_y":
            {
                Computer.State.Y = (float)value;
                return true;
            }
            case "rcs_z":
            {
                Computer.State.Z = (float)value;
                return true;
            }
        }

        return false;
    }

    #endregion

    #region Functions

    public override IEnumerable<IFunction> GetFunctions()
    {
        foreach (IFunction function in base.GetFunctions())
        {
            yield return function;
        }
        
        yield return new Function("toggle_action", terms =>
        {
            int idx = terms[0].CastToInt();
            KSPActionGroup group = (KSPActionGroup)KerbinSuperComputer.EnumFromInt(idx + 3, typeof(KSPActionGroup));
            Computer.vessel.ActionGroups.ToggleGroup(group);
        }, "action");
        yield return new Function("set_action",terms =>
        {
            int idx = terms[0].CastToInt();
            bool value = terms[1].CastToBool();
            KSPActionGroup group = (KSPActionGroup)KerbinSuperComputer.EnumFromInt(idx + 3, typeof(KSPActionGroup));
            Computer.vessel.ActionGroups.SetGroup(group, value);
        }, "action", "bool");
        yield return new Function("action_state", "bool", terms =>
        {
            int idx = terms[0].CastToInt();

            return new ReturnValue(Computer.vessel.ActionGroups.groups[idx + 1], "bool");
        }, "action");
        yield return new Function("get_tgt_velocity", "vec3d",
            () =>
            {
                if (Computer.vessel.targetObject != null)
                    return new ReturnValue(Computer.vessel.obt_velocity - Computer.vessel.targetObject.GetObtVelocity(), "vec3d");

                return new ReturnValue(new Vector3d(), "vec3d");
            });
        yield return new Function("get_parts", GetParts, "Part");
    }

    private IEnumerable<ReturnValue> GetParts()
    {
        foreach (Part part in Computer.vessel.Parts)
        {
            yield return new ReturnValue(part, "Part");
        }
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