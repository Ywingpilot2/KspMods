using System;
using AeroDynamicKerbalInterfaces.Controls.Fields;
using AeroDynamicKerbalInterfaces.Themes;
using UnityEngine;

namespace AeroDynamicKerbalInterfaces.Controls.ContentControl;

public class WindowControl : Control
{
    public override string Style { get; set; } = "WindowBase";
    public string HeaderStyle { get; set; } = "LabelHead";

    private Rect _windowRect;

    public override void Draw()
    {
        _windowRect = GUILayout.Window(new System.Random(GetHashCode()).Next(), _windowRect, DrawWindow, GUIContent.none, GetStyle(), LayoutOptions);
    }

    private void DrawWindow(int id)
    {
        GUI.DragWindow(new Rect(0,0, _windowRect.width, 20));
        GUIStyle style = new GUIStyle(ApplyStyle(ThemesDictionary.GetStyle(HeaderStyle)))
        {
            contentOffset = new Vector2(0, -20)
        };
        GUI.Label(new Rect(0, 0, _windowRect.width, _windowRect.height * 0.05f), Content, style);

        GUILayout.BeginVertical(LayoutOptions);
        
        foreach (Control control in this)
        {
            control.Draw();
        }
        GUILayout.EndVertical();
        
    }

    public WindowControl(int id, GUIContent content, Rect rect, params Control[] children) : base(id, content, children)
    {
        _windowRect = rect;
        
    }

    public WindowControl(int id, Rect rect) : base(id)
    {
        _windowRect = rect;
    }

    public WindowControl(int id, Texture content, Rect rect) : base(id, content)
    {
        _windowRect = rect;
    }

    public WindowControl(int id, string content, Rect rect) : base(id, content)
    {
        _windowRect = rect;
    }
}