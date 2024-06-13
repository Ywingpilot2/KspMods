using System.Collections.Generic;
using ActionLanguage.Library;
using ActionLanguage.Token.Functions;
using ActionLanguage.Token.Interaction;
using ActionLanguage.Token.KeyWords;
using ActionLanguage.Token.Terms;
using UnityEngine;

namespace ProgrammableMod.Scripting.Library
{
    /// <summary>
    /// Basic ksp library, this contains things mostly used for debugging
    /// </summary>
    public class KerbalLibrary : ILibrary
    {
        public IEnumerable<IFunction> GlobalFunctions => new IFunction[]
        {
            new Function("log", "void", terms =>
            {
                Debug.Log(terms[0].CastToStr());
                return new ReturnValue();
            }, "string")
        };
        public IEnumerable<BaseTerm> GlobalTerms { get; }
        public IEnumerable<IKeyword> Keywords { get; }
        public TypeLibrary TypeLibrary { get; }
    }
}