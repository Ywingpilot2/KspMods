using System.Collections.Generic;
using SteelLanguage.Reflection.Type;
using SteelLanguage.Token.Functions;
using SteelLanguage.Token.KeyWords.Container;

namespace SteelLanguage.Reflection.Library;

public interface ILibrary
{
    public string Name { get; }
    
    public IEnumerable<IFunction> GlobalFunctions { get; }
    public IEnumerable<GlobalTerm> GlobalTerms { get; }
    public IEnumerable<IKeyword> Keywords { get; }
    public TypeLibrary TypeLibrary { get; }
}