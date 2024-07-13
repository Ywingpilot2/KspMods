using System.Collections.Generic;
using UnityEngine;

namespace AeroDynamicKerbalInterfaces.Themes;

public static class ThemesDictionary
{
    private static readonly Dictionary<string, GUIStyle> Styles;
    public static readonly GUISkin Skin;

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

        #region Custom Style

        foreach (GUIStyle customStyle in HighLogic.Skin.customStyles)
        {
            AddStyle(customStyle.name.Replace(" ", ""), customStyle);
        }

        #endregion

        #endregion

        Skin = ScriptableObject.CreateInstance<GUISkin>();
        Skin.box = HighLogic.Skin.box;
        Skin.button = HighLogic.Skin.button;
        Skin.label = HighLogic.Skin.label;
        Skin.textField = HighLogic.Skin.textField;
        Skin.toggle = HighLogic.Skin.toggle;
        
        Skin.scrollView = HighLogic.Skin.scrollView;
        Skin.textArea = HighLogic.Skin.textArea;
        Skin.window = HighLogic.Skin.window;

        Skin.horizontalScrollbar = HighLogic.Skin.horizontalScrollbar;
        Skin.horizontalScrollbarThumb = HighLogic.Skin.horizontalScrollbarThumb;
        Skin.horizontalScrollbarLeftButton = HighLogic.Skin.horizontalScrollbarLeftButton;
        Skin.horizontalScrollbarRightButton = HighLogic.Skin.horizontalScrollbarRightButton;

        Skin.verticalScrollbar = HighLogic.Skin.verticalScrollbar;
        Skin.verticalScrollbarDownButton = HighLogic.Skin.verticalScrollbarDownButton;
        Skin.verticalScrollbarThumb = HighLogic.Skin.verticalScrollbarThumb;
        Skin.verticalScrollbarUpButton = HighLogic.Skin.verticalScrollbarDownButton;
        Skin.font = HighLogic.UISkin.font;
    }
}