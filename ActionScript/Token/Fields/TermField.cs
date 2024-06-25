using ActionLanguage.Token.Interaction;

namespace ActionLanguage.Token.Fields;

public struct TermField
{
    public string Name { get; }
    public bool Set { get; }
    public ReturnValue Value { get; }

    public TermField(string name, string type, object value, bool set = false)
    {
        Name = name;
        Value = new ReturnValue(value, type);
        Set = set;
    }
}