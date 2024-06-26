﻿using System.Collections.Generic;
using ActionLanguage.Library;
using ActionLanguage.Reflection;
using ActionLanguage.Token.Functions;
using ActionLanguage.Token.Interaction;
using ActionLanguage.Token.KeyWords;
using ActionLanguage.Token.Terms;
using ProgrammableMod.Modules.Computers;
using ProgrammableMod.Scripting.Exceptions;
using ProgrammableMod.Scripting.Terms.KerbNet;
using UnityEngine;

namespace ProgrammableMod.Scripting.Library;

/// <summary>
/// Basic ksp library, this contains things mostly used for debugging
/// </summary>
public class KerbalLibrary : ILibrary
{
    public string Name => "kerbnet";

    public IEnumerable<IFunction> GlobalFunctions { get; }
    /*{
        new Function("get_time", "float", _ =>
        {
            EstablishConnection();
            return new ReturnValue(Time.fixedTime, "float");
        }),
        new Function("has_access", "bool", _ => new ReturnValue(_computer.vessel.Connection.IsConnected, "bool")),
        new Function("get_altitude", "double", _ =>
        {
            EstablishConnection();
            return new ReturnValue(_computer.vessel.altitude, "double");
        }),
        new Function("get_ground_altitude", "double", _ =>
        {
            EstablishConnection();
            return new ReturnValue(_computer.vessel.terrainAltitude, "double");
        }),
        new Function("get_density", "double", _ =>
        {
            EstablishConnection();
            return new ReturnValue(_computer.vessel.atmDensity, "double");
        }),
        new Function("get_surf_dist", "float", _ =>
        {
            EstablishConnection();
            return new ReturnValue(_computer.vessel.heightFromTerrain, "float");
        }),
        new Function("get_mission_time", "double", _ =>
        {
            EstablishConnection();
            return new ReturnValue(_computer.vessel.missionTime, "double");
        })
    };*/
    public IEnumerable<GlobalTerm> GlobalTerms { get; }
    public IEnumerable<IKeyword> Keywords { get; }
    public TypeLibrary TypeLibrary { get; }

    public KerbalLibrary(BaseComputer computer, ActionLibrary library)
    {
        TypeLibrary = new TypeLibrary();
        KerbNetTerm kerb = new KerbNetTerm
        {
            Kind = TermKind.Class, 
            Computer = computer,
            Name = "KERBNET",
            
        };
        TypeLibrary.AddTermType(new TermType(kerb, library.TypeLibrary.GetTermType("term", 0)));
        
        GlobalTerms = new[]
        {
            new GlobalTerm(kerb)
        };
    }
}