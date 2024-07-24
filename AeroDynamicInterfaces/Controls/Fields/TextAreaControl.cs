using System;
using UnityEngine;

namespace AeroDynamicKerbalInterfaces.Controls.Fields;

public class TextAreaControl : Control
{
    public string Text
    {
        get => Content.text;
        set => Content.text = value;
    }
    public bool ReadOnly { get; set; }
    public event EventHandler? OnUpdated;

    public TextAreaControl(int id) : base(id)
    {
    }

    public TextAreaControl(int id, string content, EventHandler? onUpdated = null) : base(id, content)
    {
        OnUpdated = onUpdated;
    }
    
    protected void Updated() => OnUpdated?.Invoke(this, EventArgs.Empty);

    public override void Draw()
    {
        if (ReadOnly)
        {
            GUILayout.TextArea(Content.text, GetStyle(), LayoutOptions);
            return;
        }
        
        string upd = GUILayout.TextArea(Content.text, GetStyle(), LayoutOptions);

        if (upd != Content.text)
            OnUpdated?.Invoke(this, EventArgs.Empty);

        Content.text = upd;
    }
}