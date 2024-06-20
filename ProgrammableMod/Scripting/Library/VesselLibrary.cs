using System.Collections.Generic;
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
            _computer.State.pitch = Mathf.Clamp(terms[0].CastToFloat(), -1.0F, 1.0F);
            return new ReturnValue();
        }, "float"),
        new Function("set_yaw", "void", terms =>
        {
            _computer.State.yaw = Mathf.Clamp(terms[0].CastToFloat(), -1.0F, 1.0F);
            return new ReturnValue();
        }, "float"),
        new Function("set_roll", "void", terms =>
        {
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
            if (_computer.vessel.Connection.ControlState != VesselControlState.ProbeFull)
                throw new KerbnetLostException(0);
            
            _computer.vessel.Autopilot.Enable();
            return new ReturnValue();
        }),
        new Function("disable_sas", "void", _ =>
        {
            _computer.vessel.Autopilot.Disable();
            return new ReturnValue();
        }),
    };

    public IEnumerable<BaseTerm> GlobalTerms { get; }
    public IEnumerable<IKeyword> Keywords { get; }
    public TypeLibrary TypeLibrary { get; }

    public VesselLibrary(BaseComputer computer)
    {
        _computer = computer;
    }
}