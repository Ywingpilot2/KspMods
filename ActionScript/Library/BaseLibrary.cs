﻿using System;
using System.Collections.Generic;
using System.Globalization;
using ActionScript.Token.Functions;
using ActionScript.Token.Interaction;
using ActionScript.Token.KeyWords;
using ActionScript.Token.Terms;

namespace ActionScript.Library
{
    public class ActionLibrary : ILibrary
    {
        static Random _rng = new();
        
        public IEnumerable<IFunction> GlobalFunctions { get; } = new IFunction[]
        {
            new Function("to-string", "string", inputTypes: "term", action: terms =>
            {
                try
                {
                    return new ReturnValue(terms[0].CastToStr(), "string");
                }
                catch (Exception e)
                {
                    return new ReturnValue(terms[0].GetTermType().Name, "string");
                }
            }),
            new Function("equals", "bool", inputTypes:new []{"term", "term"}, action: terms =>
            {
                object a = terms[0].GetValue();
                object b = terms[0].GetValue();
                return new ReturnValue(a == b, "bool");
            }),
            new Function("random", "int", _ => new ReturnValue(_rng.Next(), "int")),
            new Function("randon-max", "int",inputTypes:new []{"int"}, action:terms => new ReturnValue(_rng.Next(terms[0].CastToInt()), "int")),
            new Function("random-interval", "int", inputTypes:new []{"int","int"}, action:terms => new ReturnValue(_rng.Next(terms[0].CastToInt(), terms[1].CastToInt()), "int")),
            new Function("not", "bool", inputTypes:new []{"bool"}, action: terms => new ReturnValue(!terms[0].CastToBool(), "bool")),
            new Function("and", "bool", inputTypes:new []{"bool","bool"}, action: terms => new ReturnValue(terms[0].CastToBool() && terms[1].CastToBool(), "bool")),
            new Function("or", "bool", inputTypes:new []{"bool","bool"}, action: terms => new ReturnValue(terms[0].CastToBool() || terms[1].CastToBool(), "bool"))
        };

        public IEnumerable<BaseTerm> GlobalTerms { get; }

        public IEnumerable<IKeyword> Keywords => new IKeyword[]
        {
            new FuncKeyword(),
            new WhileKeyword(),
            new BreakKeyword(),
            new ContinueKeyword()
        };
        public TypeLibrary TypeLibrary { get; }
        
        public ActionLibrary()
        {
            TypeLibrary = new TypeLibrary();

            TermType baseType = new TermType(new Term(), TypeLibrary, isAbstract:true);
            TermType numberType = new TermType(new NumberTerm(), TypeLibrary, baseType, true);
            TypeLibrary.AddTermType(baseType);
            TypeLibrary.AddTermType(numberType);
            TypeLibrary.AddTermType(new TermType(new VoidTerm(), TypeLibrary, isAbstract:true));
            TypeLibrary.AddTermType(new TermType(new TermI(), TypeLibrary, numberType));
            TypeLibrary.AddTermType(new TermType(new TermU(), TypeLibrary, numberType));
            TypeLibrary.AddTermType(new TermType(new TermF(), TypeLibrary, numberType));
            TypeLibrary.AddTermType(new TermType(new TermD(), TypeLibrary, numberType));
            TypeLibrary.AddTermType(new TermType(new StringTerm(), TypeLibrary, baseType));
            TypeLibrary.AddTermType(new TermType(new BoolTerm(), TypeLibrary, baseType));
        }
    }
}