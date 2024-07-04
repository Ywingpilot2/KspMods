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
    private readonly EventHandler? _onUpdated;

    public TextFieldControl(int id) : base(id)
    {
    }

    public TextFieldControl(int id, string content, EventHandler? onUpdated = null) : base(id, content)
    {
        _onUpdated = onUpdated;
    }

    public override void Draw()
    {
        string upd = GUILayout.TextArea(Content.text, GetStyle(), LayoutOptions);
        if (upd != Content.text)
            _onUpdated?.Invoke(this, EventArgs.Empty);

        Content.text = upd;
    }
}