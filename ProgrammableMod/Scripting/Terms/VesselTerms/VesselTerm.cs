﻿using System.Collections.Generic;
using ProgrammableMod.Modules;
using ProgrammableMod.Scripting.Exceptions;
using ProgrammableMod.Scripting.Terms.VesselTerms.ActionGroups;
using SteelLanguage.Token.Fields;
using SteelLanguage.Token.Functions;
using SteelLanguage.Token.Interaction;
using SteelLanguage.Token.Terms;
using UnityEngine;

namespace ProgrammableMod.Scripting.Terms.VesselTerms;

internal class VesselTerm : BaseComputerTerm
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
        
        // wheels
        yield return new TermField("wheels_steer", "float", Computer.State.wheelSteer, true);
        yield return new TermField("wheels_gas", "float", Computer.State.wheelThrottle, true);

        yield return new TermField("position", "vec3", Computer.vessel != null ? Computer.vessel.GetTransform().position : null);
        yield return new TermField("target", "target", Computer.vessel != null ? Computer.vessel.targetObject : null);
        
        yield return new TermField("altitude", "double", Computer.vessel != null ? Computer.vessel.altitude : 0.0);
        yield return new TermField("ground_dist", "float", Computer.vessel != null ? Computer.vessel.heightFromTerrain : 0.0f);
        yield return new TermField("atmo_density", "double", Computer.vessel != null ? Computer.vessel.atmDensity : 0.0);
    }

    public override bool SetField(string name, object value)
    {
        switch (name)
        {
            case "throttle":
            {
                float clamped = Mathf.Clamp((float)value, 0, 1);
                Computer.State.mainThrottle = clamped;
                return true;
            }
            case "yaw":
            {
                float clamped = Mathf.Clamp((float)value, -1, 1);
                Computer.State.yaw = clamped;
                return true;
            }
            case "pitch":
            {
                float clamped = Mathf.Clamp((float)value, -1, 1);
                Computer.State.pitch = clamped;
                return true;
            }
            case "roll":
            {
                float clamped = Mathf.Clamp((float)value, -1, 1);
                Computer.State.roll = clamped;
                return true;
            }
            case "rcs_x":
            {
                float clamped = Mathf.Clamp((float)value, -1, 1);
                Computer.State.X = clamped;
                return true;
            }
            case "rcs_y":
            {
                float clamped = Mathf.Clamp((float)value, -1, 1);
                Computer.State.Y = clamped;
                return true;
            }
            case "rcs_z":
            {
                float clamped = Mathf.Clamp((float)value, -1, 1);
                Computer.State.Z = clamped;
                return true;
            }
            case "wheels_steer":
            {
                float clamped = Mathf.Clamp((float)value, -1, 1);
                Computer.State.wheelSteer = clamped;
                return true;
            }
            case "wheels_gas":
            {
                float clamped = Mathf.Clamp((float)value, -1, 1);
                Computer.State.wheelThrottle = clamped;
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
        yield return new Function("get_part", "Part", terms => new ReturnValue(GetPart(terms[0].CastToStr()), "Part"), "string");
        yield return new Function("get_orbit", "Orbit", () =>
        {
            EstablishConnection();
            return new ReturnValue(Computer.vessel.orbit, "Orbit");
        });
        yield return new Function("get_body", "CelestialBody", () =>
        {
            EstablishConnection();
            return new ReturnValue(Computer.vessel.mainBody, "CelestialBody");
        });
    }

    private IEnumerable<ReturnValue> GetParts()
    {
        foreach (Part part in Computer.vessel.Parts)
        {
            yield return new ReturnValue(part, "Part");
        }
    }

    private Part GetPart(string name)
    {
        foreach (Part part in Computer.vessel.Parts)
        {
            if (!part.HasModuleImplementing<PartNameModule>())
                continue;

            PartNameModule nameModule = part.FindModuleImplementing<PartNameModule>();
            if (nameModule.partName == name)
                return part;
        }

        throw new PartNotFoundException(0, name);
    }

    #endregion

    public override bool CopyFrom(BaseTerm term)
    {
        bool t = base.CopyFrom(term);
        if (t)
        {
            _manager = new MylStagingManager(Computer.vessel);
        }

        return t;
    }
}