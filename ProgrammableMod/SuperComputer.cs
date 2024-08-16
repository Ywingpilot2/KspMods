using System;
using System.Collections.Generic;
using KSP.UI.Screens;
using ProgrammableMod.Extensions;
using ProgrammableMod.Scripting.Config.ScriptLibrary;
using ProgrammableMod.Scripting.Config.ValueStasher;
using ProgrammableMod.Scripting.Library;
using ProgrammableMod.Scripting.Terms;
using SteelLanguage.Exceptions;
using SteelLanguage.Library;
using SteelLanguage.Reflection.Library;
using SteelLanguage.Token.Interaction;
using SteelLanguage.Token.Terms;
using UnityEngine;
using UnityEngine.Serialization;

namespace ProgrammableMod;

/// <summary>
/// Behold! The kerbin super computer!
/// </summary>
[KSPAddon(KSPAddon.Startup.Flight, false)]
public class KerbinSuperComputer : MonoBehaviour
{
    public ValueStasher stasher;
    public static readonly ScriptLibrary Library = new();

    public static ValueStasher CurrentStasher => Instance.stasher;
    public static KerbinSuperComputer Instance { get; private set; }

    private void Awake()
    {
        stasher = new ValueStasher();
        
        GameEvents.onGameStateLoad.Add(Load);
        GameEvents.onGameStateSave.Add(Save);
    }

    private void Start()
    {
        Instance = this;
    }
    
    private void Load(ConfigNode data)
    {
        if (!data.HasNode("kerfers_stash"))
            return;
        
        stasher.Load(data.GetNode("kerfers_stash"));
    }

    private void Save(ConfigNode data)
    {
        if (data.HasNode("kerfers_stash"))
            data.RemoveNode("kerfers_stash");
        
        ConfigNode configNode = new("kerfers_stash");
        stasher.Save(configNode);
        data.AddNode(configNode);
    }

    #region Utility

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

    #endregion
}