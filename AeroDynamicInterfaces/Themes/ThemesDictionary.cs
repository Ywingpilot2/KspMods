﻿using System.Collections.Generic;
using UnityEngine;

namespace AeroDynamicKerbalInterfaces.Themes;

public static class ThemesDictionary
{
    private static readonly Dictionary<string, GUIStyle> Styles;

    #region Getting Styles

    public static bool HasStyle(string name) => Styles.ContainsKey(name);

    public static GUIStyle? GetStyle(string name)
    {
        if (!HasStyle(name))
            return null;

        return Styles[name];
    }

    public static bool AddStyle(string name, GUIStyle style)
    {
        if (HasStyle(name))
            return false;
        
        Styles.Add(name, style);
        return true;
    }

    public static bool RemoveStyle(string name)
    {
        if (!HasStyle(name))
            return false;

        Styles.Remove(name);
        return true;
    }

    public static void ReplaceStyle(string name, GUIStyle style)
    {
        RemoveStyle(name);
        AddStyle(name, style);
    }

    #endregion

    public static Color ColorFromTexture(Texture2D texture, int sourceX = 0, int sourceY = 0)
    {
        return texture.GetPixel(sourceX, sourceY);
    }

    public static Texture2D TextureFromColor(Color color, int width, int height)
    {
        Color[] pix = new Color[width * height];
        for( int i = 0; i < pix.Length; ++i )
        {
            pix.SetValue(color, i);
        }
        Texture2D result = new Texture2D(width, height);
        result.SetPixels(pix);
        result.Apply();
        return result;
    }

    static ThemesDictionary()
    {
        #region Styles

        Styles = new Dictionary<string, GUIStyle>();
        AddStyle("None", GUIStyle.none);

        #region Content Control stuff

        AddStyle("AreaBase", HighLogic.Skin.textArea);
        AddStyle("WindowBase", HighLogic.Skin.window);
        AddStyle("ScrollViewBase", HighLogic.Skin.scrollView);

        #endregion

        #region Base Controls

        AddStyle("ButtonBase", HighLogic.Skin.button);
        AddStyle("FieldBase", HighLogic.Skin.textField);
        AddStyle("BoxBase", HighLogic.Skin.box);
        AddStyle("LabelBase", HighLogic.Skin.label);
        AddStyle("ToggleBase", HighLogic.Skin.toggle);

        #endregion

        #region scrollbar

        AddStyle("HorizontalScrollbarThumb", HighLogic.Skin.horizontalScrollbarThumb);
        AddStyle("HorizontalScrollbar", HighLogic.Skin.horizontalScrollbar);
        AddStyle("HorizontalScrollbarLeft", HighLogic.Skin.horizontalScrollbarLeftButton);
        AddStyle("HorizontalScrollbarRight", HighLogic.Skin.horizontalScrollbarRightButton);
        AddStyle("HorizontalSlider", HighLogic.Skin.horizontalSlider);
        AddStyle("HorizontalSliderThumb", HighLogic.Skin.horizontalSliderThumb);
        
        AddStyle("VerticalScrollbarThumb", HighLogic.Skin.verticalScrollbarThumb);
        AddStyle("VerticalScrollbar", HighLogic.Skin.verticalScrollbar);
        AddStyle("VerticalScrollbarLeft", HighLogic.Skin.verticalScrollbarUpButton);
        AddStyle("VerticalScrollbarRight", HighLogic.Skin.verticalScrollbarDownButton);
        AddStyle("VerticalSlider", HighLogic.Skin.verticalSlider);
        AddStyle("VerticalSliderThumb", HighLogic.Skin.verticalSliderThumb);

        #endregion

        #region Extra styles

        GUIStyle style = new GUIStyle(HighLogic.Skin.label)
        {
            fontSize = 16,
            alignment = TextAnchor.MiddleCenter,
            fontStyle = FontStyle.Bold,
            stretchWidth = true
        };
        AddStyle("LabelHead", style);

        style = new GUIStyle(HighLogic.Skin.label)
        {
            fontSize = 16,
            alignment = TextAnchor.MiddleLeft,
            fontStyle = FontStyle.Bold,
            stretchWidth = true,
            normal = new GUIStyleState{textColor = Color.white, background = TextureFromColor(new Color(229,130,0,170), 2,2)}
        };
        AddStyle("LabelTitle", style); // TODO: this is broken, investigate why

        #endregion

        #endregion
    }
}