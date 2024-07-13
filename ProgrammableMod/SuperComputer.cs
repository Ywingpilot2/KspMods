using System;
using System.Collections.Generic;
using KSP.UI.Screens;
using ProgrammableMod.Scripting.Library;
using ProgrammableMod.Scripting.Terms;
using ProgrammableMod.Scripting.ValueStasher;
using SteelLanguage.Exceptions;
using SteelLanguage.Library;
using SteelLanguage.Reflection;
using SteelLanguage.Token.Interaction;
using SteelLanguage.Token.Terms;
using UnityEngine;

namespace ProgrammableMod;

/// <summary>
/// Behold! The kerbin super computer!
/// </summary>
[KSPAddon(KSPAddon.Startup.Flight, true)]
public class KerbinSuperComputer : MonoBehaviour
{
    [KSPField]
    private ValueStasher _stasher;

    public static ValueStasher CurrentStasher { get; private set; }
    internal static ILibrary[] Libraries { get; private set; }

    private void Awake()
    {
        Libraries = new ILibrary[]
        {
            new KerbalLibrary(),
            new VesselLibrary(),
            new ComputerLibrary()
        };
        
        _stasher = new ValueStasher();
        CurrentStasher = _stasher;
    }

    /// <summary>
    /// Gets an enum value at the specified index of the enum
    /// </summary>
    /// <param name="idx">The index to get the value from</param>
    /// <param name="enumType"></param>
    /// <returns>The enum value</returns>
    internal static Enum EnumFromInt(int idx, Type enumType)
    {
        Array values = enumType.GetEnumValues();
        object value = values.GetValue(idx);
        return (Enum)Enum.ToObject(enumType, value);
    }
}