using System;
using System.Collections.Generic;
using CommNet;
using KSP.UI.Screens;
using ProgrammableMod.Modules.Computers;
using ProgrammableMod.Scripting.Exceptions;
using ProgrammableMod.Scripting.Terms.VesselTerms;
using ProgrammableMod.Scripting.Terms.VesselTerms.ActionGroups;
using ProgrammableMod.Scripting.Terms.VesselTerms.Parts;
using ProgrammableMod.Scripting.Terms.VesselTerms.SAS;
using ProgrammableMod.Scripting.Terms.VesselTerms.Target;
using SteelLanguage;
using SteelLanguage.Library;
using SteelLanguage.Reflection.Library;
using SteelLanguage.Reflection.Type;
using SteelLanguage.Token.Functions;
using SteelLanguage.Token.KeyWords;
using SteelLanguage.Token.KeyWords.Container;
using SteelLanguage.Token.Terms;
using UnityEngine;

namespace ProgrammableMod.Scripting.Library;

public class VesselLibrary : ILibrary
{
    public string Name => "vessel";
    
    public IEnumerable<IFunction> GlobalFunctions { get; }

    public IEnumerable<GlobalTerm> GlobalTerms { get; }
    public IEnumerable<IKeyword> Keywords { get; }
    public TypeLibrary TypeLibrary { get; }

    public VesselLibrary(BaseComputer computer) : this()
    {
        GlobalTerms = new[]
        {
            new GlobalTerm(new VesselTerm { Name = "VESSEL", Computer = computer, Kind = TermKind.Class })
        };
    }

    public VesselLibrary()
    {
        TypeLibrary = new TypeLibrary();

        TermType baseType = SteelCompiler.Library.TypeLibrary.GetTermType("term");
        TermType enumType = SteelCompiler.Library.TypeLibrary.GetTermType("enum");
        
        TypeLibrary.AddTermType(new TermType(new VesselTerm(), baseType));
        TypeLibrary.AddTermType(new TermType(new StagingTerm(), baseType));
        TypeLibrary.AddTermType(new TermType(new StageInfoTerm(), baseType));
        TypeLibrary.AddTermType(new TermType(new SASTerm(), baseType));
        TypeLibrary.AddTermType(new TermType(new SASTypeTerm(), enumType));
        TypeLibrary.AddTermType(new TermType(new ActionGroupTerm(), enumType));
        TypeLibrary.AddTermType(new TermType(new TargetTerm(), baseType, isNullable:true));
        TypeLibrary.AddTermType(new TermType(new PartTerm(), baseType, isNullable:true));
    }
}