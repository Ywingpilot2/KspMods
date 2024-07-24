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
using UnityEngine;
using Random = System.Random;

namespace ProgrammableMod.Controls;

public sealed class LogControl : DragWindow
{
    public List<string> LogText { get; }
    private readonly TextAreaControl _textControl;

    public override void Draw()
    {
        UpdateControl();
        base.Draw();
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
        AeroInterfaceManager.RemoveControl(Id);
    }

    private void UpdateControl()
    {
        _textControl.Content.text = string.Join("\n", LogText);
    }

    public LogControl(int id) : base(id, new(Screen.width / 2, Screen.height / 2, 600, 450))
    {
        LogText = new List<string>();

        Random rng = new Random(GetHashCode());

        RowControl rows = new RowControl(rng.Next());
        ScrollViewControl scroll = new ScrollViewControl(rng.Next());
        _textControl = new TextAreaControl(rng.Next(), "");
        _textControl.LayoutOptions = new[] { GUILayout.ExpandWidth(true), GUILayout.ExpandHeight(true) };
        _textControl.RichText = true;

        scroll.Add(_textControl);
        rows.Add(scroll);

        ColumnControl columns = new ColumnControl(rng.Next());
        columns.Add(new ButtonControl(rng.Next(), "Clear", (_,_) => ClearLog()));
        columns.Add(new ButtonControl(rng.Next(), "Close", (_,_) => Close()));
        
        rows.Add(columns);
        Add(rows);
    }
}