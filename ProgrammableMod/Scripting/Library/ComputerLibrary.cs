using System.Collections.Generic;
using ActionLanguage;
using ActionLanguage.Library;
using ActionLanguage.Token.Functions;
using ActionLanguage.Token.Interaction;
using ActionLanguage.Token.KeyWords;
using ActionLanguage.Token.Terms;
using ProgrammableMod.Scripting.Terms.Graphmatics;
using ProgrammableMod.Scripting.Terms.Vectors;
using UnityEngine;

namespace ProgrammableMod.Scripting.Library;

public class ComputerLibrary : ILibrary
{
    public IEnumerable<IFunction> GlobalFunctions => new IFunction[]
    {
        new Function("lerp", "float", 
            terms => new ReturnValue(Mathf.Lerp(terms[0].CastToFloat(), terms[1].CastToFloat(), terms[2].CastToFloat()), "float"), 
            "float", "float", "float"),
        new Function("abs", "float", terms => new ReturnValue(Mathf.Abs(terms[0].CastToFloat()),"float")),
    };
    public IEnumerable<BaseTerm> GlobalTerms { get; }
    public IEnumerable<IKeyword> Keywords { get; }
    public TypeLibrary TypeLibrary { get; }

    public ComputerLibrary(ActionLibrary baseLibrary)
    {
        TypeLibrary = new TypeLibrary();

        TermType baseType = baseLibrary.TypeLibrary.GetTermType("term", 0);
        TypeLibrary.AddTermType(new TermType(new PiecewiseCTerm(), TypeLibrary, baseType));
        TypeLibrary.AddTermType(new TermType(new Vec2Term(), TypeLibrary, baseType));
        TypeLibrary.AddTermType(new TermType(new Vec2Term(), TypeLibrary, baseType));
    }
}