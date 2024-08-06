using System;
using UnityEngine;

namespace AeroDynamicKerbalInterfaces.Controls.Fields;

public class TextFieldControl : Control
{
    public string Text
    {
        get => Content.text;
        set => Content.text = value;
    }
    public event EventHandler? OnUpdated;

    public TextFieldControl(int id) : base(id)
    {
    }

    public TextFieldControl(int id, string content, EventHandler? onUpdated = null) : base(id, content)
    {
        OnUpdated = onUpdated;
    }

    protected void Updated() => OnUpdated?.Invoke(this, EventArgs.Empty);

    protected override void Draw()
    {
        string upd = GUILayout.TextField(Content.text, GetStyle(), LayoutOptions);
        if (upd != Content.text)
            OnUpdated?.Invoke(this, EventArgs.Empty);

        Content.text = upd;
    }
}