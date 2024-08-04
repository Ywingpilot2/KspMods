using System;
using System.Collections.Generic;
using ProgrammableMod.Modules.Computers;
using ProgrammableMod.Scripting.Terms.Graphmatics;
using ProgrammableMod.Scripting.Terms.Graphmatics.Curves;
using ProgrammableMod.Scripting.Terms.KerbNet.SolarSystem;
using ProgrammableMod.Scripting.Terms.Vectors;
using SteelLanguage;
using SteelLanguage.Library;
using SteelLanguage.Reflection.Library;
using SteelLanguage.Reflection.Type;
using SteelLanguage.Token.Functions;
using SteelLanguage.Token.Interaction;
using SteelLanguage.Token.KeyWords;
using SteelLanguage.Token.KeyWords.Container;
using UnityEngine;

namespace ProgrammableMod.Scripting.Library;

public class ComputerLibrary : ILibrary
{
    public string Name => "computing";
    private readonly BaseComputer _computer;

    public IEnumerable<IFunction> GlobalFunctions => new IFunction[]
    {
        new Function("lerp", "float", 
            terms => new ReturnValue(Mathf.Lerp(terms[0].CastToFloat(), terms[1].CastToFloat(), terms[2].CastToFloat()), "float"), 
            "float", "float", "float"),
        new Function("log", terms =>
        {
            _computer.Log(terms[0].CastToStr());
        }, "string"),
        new Function("display",terms =>
        {
            ScreenMessages.PostScreenMessage(terms[0].CastToStr(), 0.5f,
                ScreenMessageStyle.UPPER_LEFT);
        }, "string"),
        new Function("get_start", "float", () => new ReturnValue(_computer.runTime, "float")),
    };
    public IEnumerable<GlobalTerm> GlobalTerms { get; }
    public IEnumerable<IKeyword> Keywords { get; }
    public TypeLibrary TypeLibrary { get; }

    public ComputerLibrary(BaseComputer computer) : this()
    {
        _computer = computer;
    }

    public ComputerLibrary()
    {
        TypeLibrary = new TypeLibrary();
        
        TermType baseType = SteelCompiler.Library.TypeLibrary.GetTermType("term");
        TypeLibrary.AddTermType(new TermType(new PiecewiseCTerm(), baseType));
        TypeLibrary.AddTermType(new TermType(new Vec2Term(), baseType));
        TypeLibrary.AddTermType(new TermType(new Vec3Term(), baseType));
        TypeLibrary.AddTermType(new TermType(new Vec3dTerm(), baseType));
        TypeLibrary.AddTermType(new TermType(new OrbitTerm(), baseType, isNullable:true));
        TypeLibrary.AddTermType(new TermType(new CelestialBodyTerm(), baseType, isNullable:true));
    }
}