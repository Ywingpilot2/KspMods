using System.Collections.Generic;
using ProgrammableMod.Modules.Computers;
using ProgrammableMod.Scripting.Exceptions;
using ProgrammableMod.Scripting.Terms.KerbNet;
using SteelLanguage.Library;
using SteelLanguage.Reflection;
using SteelLanguage.Token.Functions;
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

    public KerbalLibrary(BaseComputer computer, SteelLibrary library)
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