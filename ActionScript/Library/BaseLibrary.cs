using System;
using System.Collections.Generic;
using System.Globalization;
using ActionScript.Functions;
using ActionScript.Terms;

namespace ActionScript.Library
{
    public class ActionLibrary : ILibrary
    {
        public IEnumerable<Function> GlobalFunctions { get; } = new[]
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
            })
        };

        public IEnumerable<BaseTerm> GlobalTerms { get; }
        public TypeLibrary TypeLibrary { get; }
        
        public ActionLibrary()
        {
            TypeLibrary = new TypeLibrary();

            TermType baseType = new TermType(new Term(), TypeLibrary, isAbstract:true);
            TermType numberType = new TermType(new NumberTerm(), TypeLibrary, baseType, true);
            TypeLibrary.AddTermType(baseType);
            TypeLibrary.AddTermType(numberType);
            TypeLibrary.AddTermType(new TermType(new TermI(), TypeLibrary, numberType));
            TypeLibrary.AddTermType(new TermType(new TermU(), TypeLibrary, numberType));
            TypeLibrary.AddTermType(new TermType(new TermF(), TypeLibrary, numberType));
            TypeLibrary.AddTermType(new TermType(new TermD(), TypeLibrary, numberType));
            TypeLibrary.AddTermType(new TermType(new StringTerm(), TypeLibrary, baseType));
            TypeLibrary.AddTermType(new TermType(new BoolTerm(), TypeLibrary, baseType));
        }
    }
}