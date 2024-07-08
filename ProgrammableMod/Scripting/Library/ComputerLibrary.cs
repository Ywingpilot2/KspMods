using System;
using System.Collections.Generic;
using ProgrammableMod.Modules.Computers;
using ProgrammableMod.Scripting.Terms.Graphmatics;
using ProgrammableMod.Scripting.Terms.Vectors;
using SteelLanguage.Library;
using SteelLanguage.Reflection;
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
        new Function("abs", "float", terms => new ReturnValue(Mathf.Abs(terms[0].CastToFloat()),"float"), 
            "float"),
        new Function("log", "void", terms =>
        {
            _computer.Log(terms[0].CastToStr());
            return new ReturnValue();
        }, "string"),
        new Function("display", "void", terms =>
        {
            ScreenMessages.PostScreenMessage(terms[0].CastToStr(), 0.5f,
                ScreenMessageStyle.UPPER_LEFT);
            return new ReturnValue();
        }, "string"),
        new Function("get_start", "float", _ => new ReturnValue(_computer.runTime, "float")),
    };
    public IEnumerable<GlobalTerm> GlobalTerms { get; }
    public IEnumerable<IKeyword> Keywords { get; }
    public TypeLibrary TypeLibrary { get; }

    public ComputerLibrary(SteelLibrary baseLibrary, BaseComputer computer)
    {
        _computer = computer;
        TypeLibrary = new TypeLibrary();
        
        TermType baseType = baseLibrary.TypeLibrary.GetTermType("term", 0);
        TypeLibrary.AddTermType(new TermType(new PiecewiseCTerm(), baseType));
        TypeLibrary.AddTermType(new TermType(new Vec2Term(), baseType));
        TypeLibrary.AddTermType(new TermType(new Vec3Term(), baseType));
    }
}