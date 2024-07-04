using System;
using UnityEngine;

namespace AeroDynamicKerbalInterfaces.Controls.Fields;

public class TextAreaControl : Control
{
    public string Text => Content.text;
    private readonly Action<TextAreaControl>? _onUpdated;

    public TextAreaControl(int id) : base(id)
    {
    }

    public TextAreaControl(int id, string content, Action<TextAreaControl>? onUpdated = null) : base(id, content)
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