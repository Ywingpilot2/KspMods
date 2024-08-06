using System;
using ProgrammableMod.Controls;
using UnityEngine;

namespace ProgrammableMod.Modules;

public class PartNameModule : PartModule
{
    [KSPField(groupName = "VesselNaming", groupDisplayName = "#autoLOC_8003391", guiActive = false, guiActiveEditor = false, guiName = "Part Name", isPersistant = true, guiActiveUnfocused = false)]
    public string partName = "";

    private PartRenameControl _renameControl;

    [KSPEvent(groupName = "VesselNaming", groupDisplayName = "#autoLOC_8003391", guiActive = true, guiActiveEditor = true, guiName = "Configure Part Name")]
    public void SetPartName()
    {
        _renameControl.Show(partName);
    }

    public override void OnStart(StartState state)
    {
        base.OnStart(state);

        _renameControl = new PartRenameControl("", OnApplyName);
        if (string.IsNullOrEmpty(partName))
        {
            if (HighLogic.LoadedSceneIsFlight)
                partName = $"{part.partInfo.title} #{vessel.parts.IndexOf(part)}";
            
            if (HighLogic.LoadedSceneIsEditor)
                GameEvents.onEditorShipModified.Add(OnShipModified);
        }
    }

    private void OnDestroy()
    {
        if (HighLogic.LoadedSceneIsEditor)
            GameEvents.onEditorShipModified.Remove(OnShipModified);
    }

    private void OnShipModified(ShipConstruct ship)
    {
        if (string.IsNullOrEmpty(partName) && ship.Contains(part))
            partName = $"{part.partInfo.title} #{ship.parts.IndexOf(part)}";
    }

    private void OnApplyName(string pName)
    {
        partName = pName;
        Fields[nameof(partName)].guiActive = true;
        Fields[nameof(partName)].guiActiveEditor = true;
    }
}