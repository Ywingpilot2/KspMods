using System;
using UnityEngine;

namespace AeroDynamicKerbalInterfaces.Controls.ContentControl.Flow;

public class ScrollViewControl : Control
{
    public override string Style { get; set; } = "ScrollViewBase";

    public Vector2 ScrollPosition { get; set; }
    public bool AutoScrollBottom { get; set; }

    private float _height;
    public override void Draw()
    {
        ScrollPosition = GUILayout.BeginScrollView(ScrollPosition, GetStyle(), LayoutOptions);

        if (Event.current.type == EventType.Repaint)
            _height = 0;
        
        foreach (Control childControl in this)
        {
            childControl.Draw();
            
            if (Event.current.type == EventType.Repaint)
                _height += GUILayoutUtility.GetLastRect().height;
        }
        
        GUILayout.EndScrollView();

        if (AutoScrollBottom)
        {
            // TODO: Implement a system for measuring control height in our own classes
            GUILayoutUtility.GetLastRect(); // for some reason if we dont do this it breaks lol
            float scrollBottom = _height - GUILayoutUtility.GetLastRect().height;
            
            // check if we should scroll to the bottom
            // this check works by seeing if we are close to the bottom, if we are close to the bottom it will automatically scroll us all the way down 
            if (Math.Abs(scrollBottom - ScrollPosition.y) < 25)
            {
                ScrollPosition = new Vector2(ScrollPosition.x, scrollBottom);
            }
        }
    }

    public ScrollViewControl(int id, params Control[] children) : base(id)
    {
        ScrollPosition = Vector2.zero;
        AutoScrollBottom = true;
        AddRange(children);
    }
}