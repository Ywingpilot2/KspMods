using AeroDynamicKerbalInterfaces.Themes;
using UnityEngine;

namespace AeroDynamicKerbalInterfaces.Controls.ContentControl;

public class DragWindow : WindowControl
{
    protected override void DrawWindow(int id)
    {

        GUIStyle style = ApplyStyle(ThemesDictionary.GetStyle("LabelBase")!);
        style.contentOffset = new Vector2(0, -20);
        GUI.Label(new Rect(0, 0, WindowRect.width, 20), Content, style);

        GUILayout.BeginVertical(LayoutOptions);
        
        DrawContent();
        
        GUILayout.EndVertical();
        
        GUI.DragWindow();
    }

    protected virtual void DrawContent()
    {
        foreach (Control control in this)
        {
            control.Draw();
        }
    }
    
    public DragWindow(int id, GUIContent content, Rect rect, params Control[] children) : base(id, content, rect, children)
    {
    }

    public DragWindow(int id, Rect rect) : base(id, rect)
    {
    }

    public DragWindow(int id, Texture content, Rect rect) : base(id, content, rect)
    {
    }

    public DragWindow(int id, string content, Rect rect) : base(id, content, rect)
    {
    }
}