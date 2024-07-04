using System;
using UnityEngine;

namespace AeroDynamicKerbalInterfaces.Controls.Fields;

public class TextFieldControl : Control
{
    public string Text => Content.text;
    private readonly Action<TextFieldControl>? _onUpdated;

    public TextFieldControl(int id) : base(id)
    {
    }

    public TextFieldControl(int id, string content, Action<TextFieldControl>? onUpdated = null) : base(id, content)
    {
        _onUpdated = onUpdated;
    }

    public override void Draw()
    {
        string upd = GUILayout.TextArea(Content.text, GetStyle(), LayoutOptions);
        if (upd != Content.text)
            _onUpdated?.Invoke(this);

        Content.text = upd;
    }
}