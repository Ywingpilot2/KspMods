using System;
using AeroDynamicKerbalInterfaces.Themes;
using UnityEngine;

namespace AeroDynamicKerbalInterfaces.Controls.ContentControl;

public class WindowControl : Control
{
    public override string Style { get; set; } = "WindowBase";
    public event EventHandler? OnClose;

    protected Rect WindowRect;

    public override void Draw()
    {
        WindowRect = GUILayout.Window(new System.Random(GetHashCode()).Next(), WindowRect, DrawWindow, GUIContent.none, GetStyle(), LayoutOptions);
    }

    protected virtual void DrawWindow(int id)
    {
    }

    public override void OnDestruction()
    {
        OnClose?.Invoke(this, EventArgs.Empty);
    }

    public WindowControl(int id, GUIContent content, Rect rect, params Control[] children) : base(id, content, children)
    {
        WindowRect = rect;
    }

    public WindowControl(int id, Rect rect) : base(id)
    {
        WindowRect = rect;
    }

    public WindowControl(int id, Texture content, Rect rect) : base(id, content)
    {
        WindowRect = rect;
    }

    public WindowControl(int id, string content, Rect rect) : base(id, content)
    {
        WindowRect = rect;
    }
}