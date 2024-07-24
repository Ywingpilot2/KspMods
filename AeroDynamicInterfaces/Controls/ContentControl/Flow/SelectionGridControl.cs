using System;
using UnityEngine;

namespace AeroDynamicKerbalInterfaces.Controls.ContentControl.Flow;

public class SelectionGridControl : Control
{
    public override string Style => "ButtonBase";
    public int Selected { get; set; }
    public int HorizontalCount { get; set; }
    public GUIContent[] Contents { get; set; }
    public event EventHandler? OnChanged;
    protected void Changed() => OnChanged?.Invoke(this, EventArgs.Empty);

    public SelectionGridControl(int id, int horizontalCount) : base(id)
    {
        HorizontalCount = horizontalCount;
        Contents = new GUIContent[0];
    }

    public SelectionGridControl(int id, int horizontalCount, params GUIContent[] contents) : this(id, horizontalCount)
    {
        Contents = contents;
    }
    
    public SelectionGridControl(int id, int horizontalCount, params string[] contents) : this(id, horizontalCount)
    {
        Contents = new GUIContent[contents.Length];
        for (int i = 0; i < contents.Length; i++)
        {
            Contents.SetValue(new GUIContent(contents[i]), i);
        }
    }
    
    public SelectionGridControl(int id, int horizontalCount, params Texture[] contents) : this(id, horizontalCount)
    {
        Contents = new GUIContent[contents.Length];
        for (int i = 0; i < contents.Length; i++)
        {
            Contents.SetValue(new GUIContent(contents[i]), i);
        }
    }

    public override void Draw()
    {
        int previous = Selected;
        Selected = GUILayout.SelectionGrid(Selected, Contents, HorizontalCount, GetStyle(), LayoutOptions);
        
        if (Selected != previous)
            OnChanged?.Invoke(this, EventArgs.Empty);
    }
}