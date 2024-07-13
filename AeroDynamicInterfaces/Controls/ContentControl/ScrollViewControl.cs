using System;
using System.Collections.Generic;
using AeroDynamicKerbalInterfaces.Themes;
using UnityEngine;

namespace AeroDynamicKerbalInterfaces.Controls.ContentControl;

public class ScrollViewControl : Control
{
    public override string Style { get; set; } = "ScrollViewBase";
    public string VerticalStyle { get; set; } = "VerticalScrollbar";
    public string HorizontalStyle { get; set; } = "HorizontalScrollbar";

    public Vector2 ScrollPosition { get; set; }
    public bool AutoScrollBottom { get; set; }
    
    public override void Draw()
    {
        ScrollPosition = GUILayout.BeginScrollView(ScrollPosition, false, false,
            ThemesDictionary.GetStyle(HorizontalStyle), 
            ThemesDictionary.GetStyle(VerticalStyle),  
            ThemesDictionary.GetStyle(Style),
            LayoutOptions);

        float height = 0;
        foreach (Control childControl in this)
        {
            childControl.Draw();
            height += GUILayoutUtility.GetLastRect().height;
        }
        
        GUILayout.EndScrollView();

        if (AutoScrollBottom)
        {
            // TODO: Implement a system for measuring control height in our own classes
            float scrollBottom = (height - GUILayoutUtility.GetLastRect().height) + (height / Count);
            
            // check if we should scroll to the bottom
            // this check works by seeing if we are close to the bottom, if we are close to the bottom it will automatically scroll us all the way down 
            if (Math.Abs(scrollBottom - ScrollPosition.y) < 100)
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