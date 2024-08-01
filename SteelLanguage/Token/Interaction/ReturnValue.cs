using System;

namespace SteelLanguage.Token.Interaction;

public readonly record struct ReturnValue
{
    public bool HasValue => Value != null;
    public string Type { get; }
    public object Value { get; }
    
    public ReturnValue()
    {
    }

    public ReturnValue(object value, string type)
    {
        Value = value;
        Type = type;
    }
}