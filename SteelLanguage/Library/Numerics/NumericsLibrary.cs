using System;
using System.Collections.Generic;
using SteelLanguage.Reflection.Library;
using SteelLanguage.Reflection.Type;
using SteelLanguage.Token.Functions;
using SteelLanguage.Token.Interaction;
using SteelLanguage.Token.KeyWords.Container;
using SteelLanguage.Utils;

namespace SteelLanguage.Library.Numerics;

public class NumericsLibrary : ILibrary
{
    private static readonly Random Rng = new();
    
    public string Name => "system.numerics";

    public IEnumerable<IFunction> GlobalFunctions => new IFunction[]
    {
        new Function("random", "int", () => new ReturnValue(Rng.Next(), "int")),
        new Function("random_max", "int", inputTypes: new[] { "int" },
            action: terms => new ReturnValue(Rng.Next(terms[0].CastToInt()), "int")),
        new Function("random_interval", "int", inputTypes: new[] { "int", "int" },
            action: terms => new ReturnValue(Rng.Next(terms[0].CastToInt(), terms[1].CastToInt()), "int")),
        new Function("random_double", "double", () => new ReturnValue(Rng.NextDouble(), "double")),
        new Function("abs", "double", terms => new ReturnValue(Math.Abs(terms[0].CastToDouble()), "double"), "number"),
    };
    public IEnumerable<GlobalTerm> GlobalTerms { get; }
    public IEnumerable<IKeyword> Keywords { get; }
    public TypeLibrary TypeLibrary { get; }
}