using System;
using System.Collections.Generic;
using AeroDynamicKerbalInterfaces;
using AeroDynamicKerbalInterfaces.Controls;
using AeroDynamicKerbalInterfaces.Controls.Buttons;
using AeroDynamicKerbalInterfaces.Controls.ContentControl;
using AeroDynamicKerbalInterfaces.Controls.ContentControl.Organization;
using AeroDynamicKerbalInterfaces.Controls.Fields;
using JetBrains.Annotations;
using UnityEngine;
using Random = System.Random;

namespace ProgrammableMod.Controls;

public sealed class LogControl : Control
{
    public List<string> LogText { get; }
    private readonly TextAreaControl _textControl;
    private Action<LogControl> _onClose;

    public override void Draw()
    {
        UpdateControl();
        foreach (Control control in this)
        {
            control.Draw();
        }
    }

    public void ClearLog()
    {
        LogText.Clear();
        UpdateControl();
    }

    public void Log(string log)
    {
        if (LogText.Count == 999)
            LogText.Clear();
        
        LogText.Add(log);
    }

    public void Close()
    {
        _onClose.Invoke(this);
        AeroInterfaceManager.RemoveControl(Id);
    }

    private void UpdateControl()
    {
        _textControl.Content.text = string.Join("\n", LogText);
    }

    public LogControl(int id, Action<LogControl> onClose) : base(id)
    {
        LogText = new List<string>();
        _onClose = onClose;

        Random rng = new Random(GetHashCode());
        WindowControl windowControl = new WindowControl(rng.Next(), new(Screen.width / 2, Screen.height / 2, 600, 450));

        RowControl rows = new RowControl(rng.Next());
        ScrollViewControl scroll = new ScrollViewControl(rng.Next())
        {
            AutoScrollBottom = true
        };
        _textControl = new TextAreaControl(rng.Next(), "");
        _textControl.LayoutOptions = new[] { GUILayout.ExpandWidth(true), GUILayout.ExpandHeight(true) };

        scroll.Add(_textControl);
        rows.Add(scroll);

        ColumnControl columns = new ColumnControl(rng.Next());
        columns.Add(new ButtonControl(rng.Next(), "Clear", _ => ClearLog()));
        columns.Add(new ButtonControl(rng.Next(), "Close", _ => Close()));
        
        rows.Add(columns);
        windowControl.Add(rows);
        Add(windowControl);
    }
}