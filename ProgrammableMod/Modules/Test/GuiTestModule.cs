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
using ProgrammableMod.Controls;
using UnityEngine;
using Random = System.Random;

namespace ProgrammableMod.Modules.Test;

public class GuiTestModule : PartModule
{
    private bool _isOpen = false;
    private int _winId;

    [KSPEvent(active = true, guiActive = true, guiName = "Open UI", guiActiveEditor = true)]
    public void StartExecute()
    {
        if (_isOpen)
            return;

        _winId = new System.Random().Next();
        CodeLibraryControl libraryControl = new CodeLibraryControl(_winId);
        AeroInterfaceManager.AddControl(libraryControl);
    }

    [KSPEvent(active = true, guiActive = true, guiName = "Close UI", guiActiveEditor = true)]
    public void Stop()
    {
        AeroInterfaceManager.RemoveControl(_winId);
        _isOpen = false;
    }
}