using UnityEngine;

namespace AeroDynamicKerbalInterfaces.Controls.Fields;

public class LabelControl : Control
{
    public override string Style { get; set; } = "LabelBase";

    public LabelControl(int id, GUIContent content, params GUILayoutOption[] options) : base(id, content)
    {
        LayoutOptions = options;
    }

    public LabelControl(int id) : base(id)
    {
    }

    public LabelControl(int id, Texture content) : base(id, content)
    {
    }

    public LabelControl(int id, string content) : base(id, content)
    {
    }

    protected override void Draw()
    {
        GUILayout.Label(Content, GetStyle(), LayoutOptions);
    }
}