using UnityEngine;

namespace AeroDynamicKerbalInterfaces.Controls.ContentControl.Organization;

public class ColumnControl : Control
{
    public ColumnControl(int id, GUIContent content, params Control[] children) : base(id, content, children)
    {
    }

    public ColumnControl(int id, params Control[] children) : base(id)
    {
        AddRange(children);
    }

    public ColumnControl(int id, Texture content) : base(id, content)
    {
    }

    public ColumnControl(int id, string content) : base(id, content)
    {
    }

    public override void Draw()
    {
        GUILayout.BeginHorizontal(LayoutOptions);
        
        foreach (Control childControl in this)
        {
            childControl.Draw();
        }
        
        GUILayout.EndHorizontal();
    }
}