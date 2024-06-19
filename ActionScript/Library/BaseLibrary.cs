﻿using System;
using System.Collections.Generic;
using ActionLanguage.Token.Functions;
using ActionLanguage.Token.Interaction;
using ActionLanguage.Token.KeyWords;
using ActionLanguage.Token.Terms;
using ActionLanguage.Token.Terms.Complex;
using ActionLanguage.Token.Terms.Complex.Enumerators;
using ActionLanguage.Token.Terms.Literal;
using ActionLanguage.Token.Terms.Technical;

namespace ActionLanguage.Library
{
    public class ActionLibrary : ILibrary
    {
        public string Name => "system";
        static Random _rng = new();

        public IEnumerable<IFunction> GlobalFunctions { get; } = new IFunction[]
        {
            new Function("to_string", "string", inputTypes: "term", action: terms =>
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
            new Function("equal", "bool", inputTypes:new []{"term", "term"}, action: terms =>
            {
                object a = terms[0].GetValue();
                object b = terms[1].GetValue();
                
                if (a == null && b == null) // both values are null therefore technically equal
                {
                    return new ReturnValue(true, "bool");
                }
                return new ReturnValue(Equals(a, b), "bool");
            }),
            new Function("not_equal", "bool", inputTypes:new []{"term","term"}, action: terms =>
            {
                object a = terms[0].GetValue();
                object b = terms[1].GetValue();

                if (a == null && b == null) // both values are null therefore technically equal
                {
                    return new ReturnValue(false, "bool");
                }
                return new ReturnValue(!Equals(a, b), "bool");
            }),
            new Function("greater", "bool", terms =>
            {
                // We use doubles since those are most likely to give us accurate results(everything can cast to them without losing data)
                double a = terms[0].CastToDouble();
                double b = terms[1].CastToDouble();
                return new ReturnValue(a > b, "bool");
            }, "number_term","number_term"),
            new Function("greater_equal", "bool", terms =>
            {
                // We use doubles since those are most likely to give us accurate results(everything can cast to them without losing data)
                double a = terms[0].CastToDouble();
                double b = terms[1].CastToDouble();
                return new ReturnValue(a >= b, "bool");
            }, "number_term","number_term"),
            new Function("lesser", "bool", terms =>
            {
                // We use doubles since those are most likely to give us accurate results(everything can cast to them without losing data)
                double a = terms[0].CastToDouble();
                double b = terms[1].CastToDouble();
                return new ReturnValue(a < b, "bool");
            }, "number_term","number_term"),
            new Function("lesser_equal", "bool", terms =>
            {
                // We use doubles since those are most likely to give us accurate results(everything can cast to them without losing data)
                double a = terms[0].CastToDouble();
                double b = terms[1].CastToDouble();
                return new ReturnValue(a <= b, "bool");
            }, "number_term","number_term"),
            new Function("random", "int", _ => new ReturnValue(_rng.Next(), "int")),
            new Function("random_max", "int",inputTypes:new []{"int"}, action:terms => new ReturnValue(_rng.Next(terms[0].CastToInt()), "int")),
            new Function("random_interval", "int", inputTypes:new []{"int","int"}, action:terms => new ReturnValue(_rng.Next(terms[0].CastToInt(), terms[1].CastToInt()), "int")),
            new Function("not", "bool", inputTypes:new []{"bool"}, action: terms => new ReturnValue(!terms[0].CastToBool(), "bool")),
            new Function("and", "bool", inputTypes:new []{"bool","bool"}, action: terms => new ReturnValue(terms[0].CastToBool() && terms[1].CastToBool(), "bool")),
            new Function("or", "bool", inputTypes:new []{"bool","bool"}, action: terms => new ReturnValue(terms[0].CastToBool() || terms[1].CastToBool(), "bool")),
            new Function("is_null", "bool", terms =>
            {
                BaseTerm term = terms[0];
                return term.Kind == TermKind.Null ? new ReturnValue(term.GetValue() == null, "bool") : new ReturnValue(false, "bool");
            }),
        };

        public IEnumerable<BaseTerm> GlobalTerms => new[]
        {
            new NullTerm
            {
                Name = "null",
                Kind = TermKind.Null,
                Line = 0
            }
        };

        public IEnumerable<IKeyword> Keywords => new IKeyword[]
        {
            new FuncKeyword(),
            new WhileKeyword(),
            new BreakKeyword(),
            new ContinueKeyword(),
            new ReturnKeyword(),
            new ThrowKeyword(),
            new IfKeyword(),
            new ElseIfKeyword(),
            new ElseKeyword(),
            new ForeachKeyword(),
            new ImportKeyword()
        };
        public TypeLibrary TypeLibrary { get; }
        
        public ActionLibrary()
        {
            TypeLibrary = new TypeLibrary();

            TermType baseType = new TermType(new Term(), TypeLibrary, isAbstract:true);
            TermType numberType = new TermType(new NumberTerm(), TypeLibrary, baseType, true);
            TermType enumerableType = new TermType(new EnumeratorTerm(), TypeLibrary, baseType, true);
            TypeLibrary.AddTermType(baseType);
            TypeLibrary.AddTermType(numberType);
            TypeLibrary.AddTermType(enumerableType);
            TypeLibrary.AddTermType(new TermType(new VoidTerm(), TypeLibrary, isAbstract:true));
            TypeLibrary.AddTermType(new TermType(new NullTerm(), TypeLibrary, baseType, isAbstract:true));
            TypeLibrary.AddTermType(new TermType(new TermI(), TypeLibrary, numberType));
            TypeLibrary.AddTermType(new TermType(new TermU(), TypeLibrary, numberType));
            TypeLibrary.AddTermType(new TermType(new TermF(), TypeLibrary, numberType));
            TypeLibrary.AddTermType(new TermType(new TermD(), TypeLibrary, numberType));
            TypeLibrary.AddTermType(new TermType(new StringTerm(), TypeLibrary, baseType));
            TypeLibrary.AddTermType(new TermType(new BoolTerm(), TypeLibrary, baseType));
            TypeLibrary.AddTermType(new TermType(new ArrayTerm(), TypeLibrary, enumerableType));
        }
    }
}