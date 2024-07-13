namespace ProgrammableMod.Scripting.ValueStasher;

public struct ProtoStash
{
    public string Name { get; }
    public string ValueType { get; }
    public ConfigNode Node { get; }

    public ProtoStash(string name, string valueType, ConfigNode node)
    {
        Name = name;
        ValueType = valueType;
        Node = node;
    }
}