using UnityEngine;

namespace AeroDynamicKerbalInterfaces.Controls.ContentControl.Organization;

public class RowControl : Control
{
    public RowControl(int id, GUIContent content, params Control[] children) : base(id, content, children)
    {
    }

    public RowControl(int id, params Control[] children) : base(id)
    {
        AddRange(children);
    }

    public RowControl(int id, Texture content) : base(id, content)
    {
    }

    public RowControl(int id, string content) : base(id, content)
    {
    }

    public override void Draw()
    {
        GUILayout.BeginVertical(LayoutOptions);

        foreach (Control control in this)
        {
            control.Draw();
        }
        
        GUILayout.EndVertical();
    }
}