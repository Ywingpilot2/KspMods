using System;
using UnityEngine;

namespace AeroDynamicKerbalInterfaces.Controls.Buttons;

public class ToggleControl : Control
{
    public bool Toggle { get; set; }
    public event EventHandler<bool>? OnChange; 

    public override string Style { get; set; } = "ToggleBase";

    protected override void Draw()
    {
        bool toggle = GUILayout.Toggle(Toggle, Content, GetStyle(), LayoutOptions);
        if (toggle != Toggle)
        {
            Toggle = toggle;
            OnChange?.Invoke(this, Toggle);
        }
    }

    public ToggleControl(int id) : base(id)
    {
    }

    public ToggleControl(int id, Texture content) : base(id, content)
    {
    }

    public ToggleControl(int id, string content) : base(id, content)
    {
    }
}