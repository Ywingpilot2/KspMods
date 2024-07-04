using System;
using AeroDynamicKerbalInterfaces.Themes;
using UnityEngine;

namespace AeroDynamicKerbalInterfaces.Controls.Buttons;

public class ButtonControl : Control
{
    public override string Style { get; set; } = "ButtonBase";

    protected readonly Action<ButtonControl> OnPressed;
    
    public override void Draw()
    {
        if (GUILayout.Button(Content, GetStyle(), LayoutOptions))
        {
            OnPressed.Invoke(this);
        }
    }

    public ButtonControl(int id, GUIContent content, Action<ButtonControl> onPressed, params Control[] children) : base(id, content, children)
    {
        OnPressed = onPressed;
    }

    public ButtonControl(int id, Action<ButtonControl> onPressed) : base(id)
    {
        OnPressed = onPressed;
    }

    public ButtonControl(int id, Texture content, Action<ButtonControl> onPressed) : base(id, content)
    {
        OnPressed = onPressed;
    }

    public ButtonControl(int id, string content, Action<ButtonControl> onPressed) : base(id, content)
    {
        OnPressed = onPressed;
    }
}