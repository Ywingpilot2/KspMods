﻿using System.Collections.Generic;
using ProgrammableMod.Modules.Computers;
using ProgrammableMod.Scripting.Exceptions;
using ProgrammableMod.Scripting.Terms.KerbNet;
using ProgrammableMod.Scripting.Terms.KerbNet.SolarSystem;
using SteelLanguage;
using SteelLanguage.Library;
using SteelLanguage.Reflection.Library;
using SteelLanguage.Reflection.Type;
using SteelLanguage.Token.Functions;
using SteelLanguage.Token.Interaction;
using SteelLanguage.Token.KeyWords;
using SteelLanguage.Token.KeyWords.Container;
using SteelLanguage.Token.Terms;
using UnityEngine;

namespace ProgrammableMod.Scripting.Library;

/// <summary>
/// Basic ksp library, this contains things mostly used for debugging
/// </summary>
public class KerbalLibrary : ILibrary
{
    public string Name => "kerbnet";

    public IEnumerable<IFunction> GlobalFunctions { get; }
    public IEnumerable<GlobalTerm> GlobalTerms { get; }
    public IEnumerable<IKeyword> Keywords { get; }
    public TypeLibrary TypeLibrary { get; }

    public KerbalLibrary(BaseComputer computer)
    {
        TypeLibrary = new TypeLibrary();
        TermType baseType = SteelCompiler.Library.TypeLibrary.GetTermType("term");
        
        TypeLibrary.AddTermType(new TermType(new KerbNetTerm {Computer = computer}, baseType));
        TypeLibrary.AddTermType(new TermType(new SuperComputerTerm(), baseType));
        
        GlobalTerms = new[]
        {
            new GlobalTerm(new KerbNetTerm
            {
                Kind = TermKind.Class, 
                Computer = computer,
                Name = "KERBNET",
            })
        };
        
        GlobalFunctions = new IFunction[]
        {
            new Function("has_access", "bool", () => new ReturnValue(computer.vessel.Connection.IsConnected, "bool"))
        };
    }
}