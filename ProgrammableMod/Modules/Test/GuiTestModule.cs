using System;
using UnityEngine;

namespace ProgrammableMod.Modules.Test;

public class GuiTestModule : PartModule
{
    private bool _show;

    private void OnGUI()
    {
        if (HighLogic.LoadedSceneIsFlight && _show)
        {
            GUI.Box(new Rect(10,10,100,90), "test", HighLogic.Skin.box);
			
            if (GUI.Button(new Rect(Screen.width / 2,Screen.height / 2,80,20), "test button"))
            {
                Debug.Log("fcuk you baltimore");
            }
        }
    }

    [KSPEvent(active = true, guiActive = true, guiName = "Open UI")]
    public void StartExecute()
    {
        _show = true;
    }
    
    [KSPEvent(active = true, guiActive = true, guiName = "Close UI")]
    public void Stop()
    {
        _show = false;
    }
}