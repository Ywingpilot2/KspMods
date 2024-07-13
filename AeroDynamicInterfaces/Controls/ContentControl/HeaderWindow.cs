using System;
using AeroDynamicKerbalInterfaces.Controls.Fields;
using AeroDynamicKerbalInterfaces.Themes;
using UnityEngine;

namespace AeroDynamicKerbalInterfaces.Controls.ContentControl;

public class HeaderWindow : WindowControl
{
    public virtual string HeaderStyle { get; set; } = "LabelBase";

    protected override void DrawWindow(int id)
    {
        GUI.DragWindow(new Rect(0,0, WindowRect.width, 20));
        GUIStyle style = new GUIStyle(ApplyStyle(ThemesDictionary.GetStyle(HeaderStyle)))
        {
            contentOffset = new Vector2(0, -20)
        };
        GUI.Label(new Rect(0, 0, WindowRect.width, 20), Content, style);

        GUILayout.BeginVertical(LayoutOptions);
        
        DrawContent();
        
        GUILayout.EndVertical();
    }

    protected virtual void DrawContent()
    {
        foreach (Control control in this)
        {
            control.Draw();
        }
    }

    public HeaderWindow(int id, GUIContent content, Rect rect, params Control[] children) : base(id, content, rect, children)
    {
    }

    public HeaderWindow(int id, Rect rect) : base(id, rect)
    {
    }

    public HeaderWindow(int id, Texture content, Rect rect) : base(id, content, rect)
    {
    }

    public HeaderWindow(int id, string content, Rect rect) : base(id, content, rect)
    {
    }
}