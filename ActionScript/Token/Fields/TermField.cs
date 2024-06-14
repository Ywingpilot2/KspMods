using ActionLanguage.Token.Interaction;

namespace ActionLanguage.Token.Fields;

public struct TermField
{
    public string Name { get; }
    public ReturnValue Value { get; }

    public TermField(string name, string type, object value)
    {
        Name = name;
        Value = new ReturnValue(value, type);
    }
}