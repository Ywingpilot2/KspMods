using System;

namespace SteelLanguage.Token.Interaction;

public readonly record struct ReturnValue
{
    public bool HasValue => Value != null;
    public string Type { get; }
    public object Value { get; }

    public static readonly ReturnValue None = new();
    
    [Obsolete(message:"Please use ReturnValue.None instead")]
    public ReturnValue()
    {
    }

    public ReturnValue(object value, string type)
    {
        Value = value;
        Type = type;
    }
}