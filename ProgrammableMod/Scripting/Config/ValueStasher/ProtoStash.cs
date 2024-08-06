using System.Globalization;
using ProgrammableMod.Extensions;
using SteelLanguage.Exceptions;

namespace ProgrammableMod.Scripting.Config.ValueStasher;

public readonly record struct ProtoStash(string Name, string ValueType, ConfigNode Node)
{
    public string Name { get; } = Name;
    public string ValueType { get; } = ValueType;
    public ConfigNode Node { get; } = Node;

    public bool IsBasic => bool.Parse(Node.GetValue("basic_type"));

    private readonly NumberFormatInfo _info = new();

    public object GetBasicValue()
    {
        switch (ValueType)
        {
            case "int": return GetAsInt();
            case "uint": return GetAsUint();
            case "float": return GetAsFloat();
            case "double": return GetAsDouble();
            case "bool": return GetAsBool();
            // annoyding
            case "string": return Node.GetValue("value").ConfigDirty();
        }
        
        throw new InvalidActionException(0, $"Stashed value {Name} of type {ValueType} was not a basic type(int, string, float)");
    }
    
    public int GetAsInt()
    {
        if (int.TryParse(Node.GetValue("value"), NumberStyles.Integer & NumberStyles.AllowLeadingSign, _info, out int i))
        {
            return i;
        }

        return 0;
    }
    
    public float GetAsFloat()
    {
        if (float.TryParse(Node.GetValue("value"), NumberStyles.Float, _info, out float i))
        {
            return i;
        }

        return 0;
    }
    
    public uint GetAsUint()
    {
        if (uint.TryParse(Node.GetValue("value"), NumberStyles.Integer & NumberStyles.AllowLeadingSign, _info, out uint i))
        {
            return i;
        }

        return 0;
    }
    
    public double GetAsDouble()
    {
        if (double.TryParse(Node.GetValue("value"), NumberStyles.Float, _info, out double i))
        {
            return i;
        }

        return 0;
    }

    public bool GetAsBool()
    {
        if (bool.TryParse(Node.GetValue("value"), out bool b))
        {
            return b;
        }
        
        return false;
    }
}