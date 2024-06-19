using System.Collections.Generic;
using ActionLanguage.Token.Functions;
using ActionLanguage.Token.KeyWords;
using ActionLanguage.Token.Terms;

namespace ActionLanguage.Library;

public interface ILibrary
{
    public string Name { get; }
    
    public IEnumerable<IFunction> GlobalFunctions { get; }
    public IEnumerable<BaseTerm> GlobalTerms { get; }
    public IEnumerable<IKeyword> Keywords { get; }
    public TypeLibrary TypeLibrary { get; }
}