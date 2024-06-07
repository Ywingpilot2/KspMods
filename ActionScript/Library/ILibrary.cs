using System.Collections.Generic;
using ActionScript.Functions;
using ActionScript.Terms;

namespace ActionScript.Library;

public interface ILibrary
{
    public IEnumerable<Function> Functions { get; }
    public IEnumerable<Term> GlobalTerms { get; }
}