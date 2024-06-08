using System.Collections.Generic;
using ActionScript.Functions;
using ActionScript.Terms;

namespace ActionScript.Library;

public interface ILibrary
{
    public IEnumerable<Function> GlobalFunctions { get; }
    public IEnumerable<BaseTerm> GlobalTerms { get; }
    public TypeLibrary TypeLibrary { get; }
}