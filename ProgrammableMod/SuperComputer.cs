using System;
using System.Collections.Generic;
using UnityEngine;

namespace ProgrammableMod;

/// <summary>
/// Behold! The kerbin super computer!
/// </summary>
[KSPAddon(KSPAddon.Startup.Flight, true)]
public class KerbinSuperComputer : MonoBehaviour
{
    private void Awake()
    {
       // TODO: Find a way to handle stashing of values to this central class
       // the issue I am encountering right now is we need to be able to add any value of any type to the save
       // ...or we could add restrictions to the stasher and only let basic types(e.g ints or vecs) through
       // that would be annoying though, complex classes such as piecewise or arrays wouldn't be stashable then
    }

    /// <summary>
    /// Gets an enum value at the specified index of the enum
    /// </summary>
    /// <param name="idx">The index to get the value from</param>
    /// <returns>The enum value</returns>
    public static Enum EnumFromInt(int idx, Type enumType)
    {
        Array values = enumType.GetEnumValues();
        object value = values.GetValue(idx);
        return (Enum)Enum.ToObject(enumType, value);
    }
}

public class ValueStasher : IConfigNode
{
    private readonly Dictionary<string, object> _stashedValues;

    public void StashValue(string name, object value)
    {
        if (HasValue(name))
        {
            _stashedValues.Remove(name);
        }
        
        _stashedValues.Add(name, value);
    }

    public object GetValue(string name)
    {
        if (!HasValue(name))
            return null;

        return _stashedValues[name];
    }

    public bool HasValue(string name) => _stashedValues.ContainsKey(name);

    public void Load(ConfigNode node)
    {
        throw new NotImplementedException();
    }

    public void Save(ConfigNode node)
    {
        throw new NotImplementedException();
    }

    public ValueStasher()
    {
        _stashedValues = new Dictionary<string, object>();
    }
}