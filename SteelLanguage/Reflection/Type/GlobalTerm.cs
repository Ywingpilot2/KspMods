using SteelLanguage.Token.Terms;

namespace SteelLanguage.Reflection.Type;

public readonly record struct GlobalTerm(string Name, string TypeName)
{
    public BaseTerm Source { get; }
    public string Name { get; } = Name;
    public string TypeName { get; } = TypeName;

    public GlobalTerm(BaseTerm source) : this(source.Name, source.ValueType)
    {
        Source = source;
    }
}