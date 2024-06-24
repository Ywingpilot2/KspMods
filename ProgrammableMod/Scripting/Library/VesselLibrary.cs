using System;
using System.Collections.Generic;
using ActionLanguage.Exceptions;
using ActionLanguage.Library;
using ActionLanguage.Token.Functions;
using ActionLanguage.Token.Interaction;
using ActionLanguage.Token.KeyWords;
using ActionLanguage.Token.Terms;
using ActionLanguage.Token.Terms.Literal;
using CommNet;
using ProgrammableMod.Modules.Computers;
using ProgrammableMod.Scripting.Exceptions;
using UnityEngine;

namespace ProgrammableMod.Scripting.Library;

public class VesselLibrary : ILibrary
{
    private BaseComputer _computer;

    public string Name => "vessel";

    public IEnumerable<IFunction> GlobalFunctions => new IFunction[]
    {
        new Function("set_pitch", "void", terms =>
        {
            if (_computer.vessel.Autopilot.Enabled)
            {
                _computer.SetStatus("SAS Enabled, cannot do maneuvers", StatusKind.NotGreat);
                return new ReturnValue();
            }
            _computer.State.pitch = Mathf.Clamp(terms[0].CastToFloat(), -1.0F, 1.0F);
            return new ReturnValue();
        }, "float"),
        new Function("set_yaw", "void", terms =>
        {
            if (_computer.vessel.Autopilot.Enabled)
            {
                _computer.SetStatus("SAS Enabled, cannot do maneuvers", StatusKind.NotGreat);
                return new ReturnValue();
            }
            
            _computer.State.yaw = Mathf.Clamp(terms[0].CastToFloat(), -1.0F, 1.0F);
            return new ReturnValue();
        }, "float"),
        new Function("set_roll", "void", terms =>
        {
            if (_computer.vessel.Autopilot.Enabled)
            {
                _computer.SetStatus("SAS Enabled, cannot do maneuvers", StatusKind.NotGreat);
                return new ReturnValue();
            }
            
            _computer.State.roll = Mathf.Clamp(terms[0].CastToFloat(), -1.0F, 1.0F);
            return new ReturnValue();
        }, "float"),
        new Function("set_throttle", "void", terms =>
        {
            _computer.State.mainThrottle = Mathf.Clamp(terms[0].CastToFloat(), 0.0F, 1.0F);
            return new ReturnValue();
        }, "float"),
        new Function("get_start", "float", _ => new ReturnValue(_computer.runTime, "float")),
        new Function("get_angular_velocity", "vec3", _ => new ReturnValue(_computer.vessel.angularVelocity, "vec3")),
        new Function("get_surf_velocity", "vec3", _ => new ReturnValue(_computer.vessel.srf_velocity, "vec3")),
        new Function("get_accel", "vec3", _ => new ReturnValue(_computer.vessel.acceleration, "vec3")),
        new Function("enable_sas", "void", _ =>
        {
            if (_computer.vessel.Connection.ControlState != VesselControlState.ProbeFull 
                && _computer.vessel.Connection.ControlState != VesselControlState.KerbalFull)
                throw new KerbnetLostException(0);
            
            if (!_computer.vessel.Autopilot.Enabled)
                _computer.vessel.Autopilot.Enable();
            
            return new ReturnValue();
        }),
        new Function("disable_sas", "void", _ =>
        {
            if (_computer.vessel.Autopilot.Enabled)
                _computer.vessel.Autopilot.Disable();
            
            return new ReturnValue();
        }),
        new Function("can_sas", "bool", terms => new ReturnValue(
            _computer.vessel.Autopilot.CanSetMode(
                (VesselAutopilot.AutopilotMode)Enum.Parse(typeof(VesselAutopilot.AutopilotMode),
                    terms[0].CastToStr())), "bool"), "string"),
        new Function("set_sas", "void", terms =>
        {
            string sasType = terms[0].CastToStr();

            // TODO: enums
            VesselAutopilot.AutopilotMode mode = (VesselAutopilot.AutopilotMode)Enum.Parse(typeof(VesselAutopilot.AutopilotMode), sasType);
            if (!_computer.vessel.Autopilot.CanSetMode(mode))
                throw new InvalidActionException(0, $"Cannot set autopilot mode to {sasType} currently");

            // You can only set SAS once, if you spam set it over and over and over it will just do nothing
            // so to prevent this from happening, we just don't set it if its the same
            if (mode != _computer.vessel.Autopilot.Mode)
                _computer.vessel.Autopilot.SetMode(mode);
            
            return new ReturnValue();
        }, "string"),
        new Function("get_vertical_speed", "double", _ => new ReturnValue(_computer.vessel.verticalSpeed, "double")),
        new Function("get_horizontal_speed", "double", _ => new ReturnValue(_computer.vessel.horizontalSrfSpeed, "double"))
    };

    public IEnumerable<GlobalTerm> GlobalTerms { get; }
    public IEnumerable<IKeyword> Keywords { get; }
    public TypeLibrary TypeLibrary { get; }

    public VesselLibrary(BaseComputer computer)
    {
        _computer = computer;
    }
}