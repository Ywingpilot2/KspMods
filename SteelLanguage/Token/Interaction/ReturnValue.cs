namespace SteelLanguage.Token.Interaction;

public struct ReturnValue
{
    public bool HasValue => Value != null;
    public string Type { get; }
    public object Value { get; }

    public ReturnValue(object value, string type)
    {
        Value = value;
        Type = type;
    }
}