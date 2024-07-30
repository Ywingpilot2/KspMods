using System;
using System.Collections.Generic;
using AeroDynamicKerbalInterfaces;
using AeroDynamicKerbalInterfaces.Controls;
using AeroDynamicKerbalInterfaces.Controls.Buttons;
using AeroDynamicKerbalInterfaces.Controls.ContentControl;
using AeroDynamicKerbalInterfaces.Controls.ContentControl.Flow;
using AeroDynamicKerbalInterfaces.Controls.ContentControl.Organization;
using AeroDynamicKerbalInterfaces.Controls.ContentControl.Windows;
using AeroDynamicKerbalInterfaces.Controls.Fields;
using JetBrains.Annotations;
using ProgrammableMod.Controls;
using UnityEngine;
using Random = System.Random;

namespace ProgrammableMod.Modules.Test;

public class GuiTestModule : PartModule
{
    [UI_Toggle(scene = UI_Scene.All, controlEnabled = true, enabledText = "yes", disabledText = "no", tipText = "fuck you")]
    [KSPField(guiActive = true, guiActiveEditor = true)]
    [UsedImplicitly]
    public bool boolean = false;

    [UI_FloatRange(minValue = 0, maxValue = 100, controlEnabled = true, scene = UI_Scene.All, stepIncrement = 1f)]
    [KSPField(guiActiveEditor = true, guiActive = true, guiName = "number", groupName = "test", groupDisplayName = "#autoLOC_8003391")]
    public float number = 0;

    public float Number
    {
        get => number;
        set => number = value;
    }

    private void FixedUpdate()
    {
        if (boolean)
        {
            Number = new Random().Next(0, 100);
        }
    }

    [KSPEvent(guiActive = true, guiActiveEditor = true, groupName = "test", groupDisplayName = "#autoLOC_8003391", guiName = "test event")]
    public void TestFunc()
    {
        
    }
}