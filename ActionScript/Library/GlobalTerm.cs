using ActionLanguage.Token.Terms;

namespace ActionLanguage.Library;

public struct GlobalTerm
{
    public string Name { get; }
    public string TypeName { get; }
    public BaseTerm Source { get; }

    public GlobalTerm(string name, string typeName)
    {
        Name = name;
        TypeName = typeName;
    }

    public GlobalTerm(BaseTerm source)
    {
        Name = source.Name;
        TypeName = source.ValueType;
        Source = source;
    }
}