using System;
using System.Collections.Generic;
using AeroDynamicKerbalInterfaces;
using AeroDynamicKerbalInterfaces.Controls;
using AeroDynamicKerbalInterfaces.Controls.Buttons;
using AeroDynamicKerbalInterfaces.Controls.ContentControl;
using AeroDynamicKerbalInterfaces.Controls.ContentControl.Organization;
using AeroDynamicKerbalInterfaces.Controls.Fields;
using UnityEngine;
using Random = System.Random;

namespace ProgrammableMod.Modules.Test;

public class GuiTestModule : PartModule
{
    private bool _isOpen = false;
    private int _winId;
    private TextAreaControl _textControl;
    private string _text = "this is a text";
    
    [KSPEvent(active = true, guiActive = true, guiName = "Open UI", guiActiveEditor = true)]
    public void StartExecute()
    {
        if (_isOpen)
            return;
        
        Random random = new Random();
        _winId = random.Next();

        _textControl = new TextAreaControl(random.Next(), _text)
        {
            LayoutOptions = new []{GUILayout.ExpandWidth(true),GUILayout.ExpandHeight(true)}
        };

        WindowControl control = new WindowControl(_winId, new GUIContent("I am gonna touch you"),
            new(Screen.width / 2, Screen.height / 2, 600, 450),
            new ScrollViewControl(random.Next(), _textControl))
            {
                FontSize = 18,
            };

        ColumnControl columnControl = new ColumnControl(random.Next(),
            new ButtonControl(random.Next(), "Cancel", (_,_) => Stop()),
            new ButtonControl(random.Next(), "Save", Save));
        control.Add(columnControl);

        AeroInterfaceManager.AddControl(control);
        _isOpen = true;
    }

    private void Save(object sender, EventArgs e)
    {
        _text = _textControl.Text;
        Debug.Log(_text);
        Stop();
    }

    [KSPEvent(active = true, guiActive = true, guiName = "Close UI", guiActiveEditor = true)]
    public void Stop()
    {
        AeroInterfaceManager.RemoveControl(_winId);
        _isOpen = false;
    }
}