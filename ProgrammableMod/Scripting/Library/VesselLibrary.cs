using System.Collections.Generic;
using ActionLanguage.Library;
using ActionLanguage.Token.Functions;
using ActionLanguage.Token.Interaction;
using ActionLanguage.Token.KeyWords;
using ActionLanguage.Token.Terms;
using ProgrammableMod.Modules.Computers;
using UnityEngine;

namespace ProgrammableMod.Scripting.Library;

public class VesselLibrary : ILibrary
{
    private BaseComputer _computer;
    
    public IEnumerable<IFunction> GlobalFunctions => new IFunction[]
    {
        new Function("set_pitch", "void", terms =>
        {
            _computer._state.pitch = Mathf.Clamp(terms[0].CastToFloat(), -1.0F, 1.0F);
            return new ReturnValue();
        }, "float"),
        new Function("set_yaw", "void", terms =>
        {
            _computer._state.yaw = Mathf.Clamp(terms[0].CastToFloat(), -1.0F, 1.0F);
            return new ReturnValue();
        }, "float"),
        new Function("set_roll", "void", terms =>
        {
            _computer._state.roll = Mathf.Clamp(terms[0].CastToFloat(), -1.0F, 1.0F);
            return new ReturnValue();
        }, "float"),
        new Function("set_throttle", "void", terms =>
        {
            _computer._state.mainThrottle = Mathf.Clamp(terms[0].CastToFloat(), 0.0F, 1.0F);
            return new ReturnValue();
        }, "float")
    };
    public IEnumerable<BaseTerm> GlobalTerms { get; }
    public IEnumerable<IKeyword> Keywords { get; }
    public TypeLibrary TypeLibrary { get; }

    public VesselLibrary(BaseComputer computer)
    {
        _computer = computer;
    }
}