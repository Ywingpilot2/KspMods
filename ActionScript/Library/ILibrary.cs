using System.Collections.Generic;
using ActionScript.Token.Functions;
using ActionScript.Token.KeyWords;
using ActionScript.Token.Terms;

namespace ActionScript.Library;

public interface ILibrary
{
    public IEnumerable<IFunction> GlobalFunctions { get; }
    public IEnumerable<BaseTerm> GlobalTerms { get; }
    public IEnumerable<IKeyword> Keywords { get; }
    public TypeLibrary TypeLibrary { get; }
}