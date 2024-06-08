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
            new Function("to-string", "string", inputTypes: "term", action: terms => new ReturnValue(terms[0].CastToStr(), "string"))
        };

        public IEnumerable<BaseTerm> GlobalTerms { get; }

        public IEnumerable<TermType> Types { get; }
        public TypeLibrary TypeLibrary { get; }
        
        public ActionLibrary()
        {
            TypeLibrary = new TypeLibrary();
            TypeLibrary.AddTermType(new TermType(new TermI(), TypeLibrary));
            TypeLibrary.AddTermType(new TermType(new TermU(), TypeLibrary));
            TypeLibrary.AddTermType(new TermType(new TermF(), TypeLibrary));
            TypeLibrary.AddTermType(new TermType(new TermD(), TypeLibrary));
            TypeLibrary.AddTermType(new TermType(new StringTerm(), TypeLibrary));
            TypeLibrary.AddTermType(new TermType(new BoolTerm(), TypeLibrary));
        }
    }
}