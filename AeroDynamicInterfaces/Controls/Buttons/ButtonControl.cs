using System;
using AeroDynamicKerbalInterfaces.Themes;
using UnityEngine;

namespace AeroDynamicKerbalInterfaces.Controls.Buttons;

public class ButtonControl : Control
{
    public override string Style { get; set; } = "ButtonBase";
    public event EventHandler? OnPressed;

    public override void Draw()
    {
        if (GUILayout.Button(Content, GetStyle(), LayoutOptions))
        {
            OnPressed?.Invoke(this, EventArgs.Empty);
            
        }
    }

    public ButtonControl(int id, GUIContent content, EventHandler? onPressed = null, params GUILayoutOption[] options) : base(id, content)
    {
        OnPressed += onPressed;
        LayoutOptions = options;
    }

    public ButtonControl(int id, EventHandler? onPressed = null) : base(id)
    {
        OnPressed = onPressed;
    }

    public ButtonControl(int id, Texture content, EventHandler? onPressed = null) : base(id, content)
    {
        OnPressed = onPressed;
    }

    public ButtonControl(int id, string content, EventHandler? onPressed = null) : base(id, content)
    {
        OnPressed = onPressed;
    }
}