namespace ActionLanguage.Library;

public struct GlobalTerm
{
    public string Name { get; }
    public string TypeName { get; }

    public GlobalTerm(string name, string typeName)
    {
        Name = name;
        TypeName = typeName;
    }
}