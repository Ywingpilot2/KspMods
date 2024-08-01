using SteelLanguage.Token.Terms;

namespace SteelLanguage.Reflection.Type;

public record struct GlobalTerm(string Name, string TypeName)
{
    public BaseTerm Source { get; }

    public GlobalTerm(BaseTerm source) : this(source.Name, source.ValueType)
    {
        Source = source;
    }
}