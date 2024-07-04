using System;
using UnityEngine;

namespace AeroDynamicKerbalInterfaces.Controls.Fields;

public class TextAreaControl : Control
{
    public string Text => Content.text;
    private readonly EventHandler? _onUpdated;

    public TextAreaControl(int id) : base(id)
    {
    }

    public TextAreaControl(int id, string content, EventHandler? onUpdated = null) : base(id, content)
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