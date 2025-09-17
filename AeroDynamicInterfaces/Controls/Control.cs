using System.Collections;
using System.Collections.Generic;
using AeroDynamicKerbalInterfaces.Exceptions;
using AeroDynamicKerbalInterfaces.Themes;
using KSPAssets.Loaders;
using UniLinq;
using UnityEngine;
using UnityEngine.UI;

namespace AeroDynamicKerbalInterfaces.Controls;

/// <summary>
/// Represents the base class for a UI widget
/// </summary>
public abstract class Control : ICollection<Control>
{
    public int Id { get; }

    #region Styling

    public virtual string Style { get; set; } = "AreaBase";

    #region Style Properties

    public Font Font { get; set; }
    public int FontSize { get; set; }
    public FontStyle FontStyle { get; set; }
    public TextAnchor TextAlignment { get; set; }
    
    public bool RichText { get; set; }
    public bool WordWrap { get; set; }
    
    public RectOffset Padding { get; set; }
    public RectOffset Margin { get; set; }
    public RectOffset Border { get; set; }
    
    public float FixedHeight { get; set; }
    public float FixedWidth { get; set; }
    
    public bool ExpandHeight { get; set; }
    public bool ExpandWidth { get; set; }
    
    public Vector2 Offset { get; set; }

    #endregion
    
    public GUIStyle GetStyle()
    {
        if (!ThemesDictionary.HasStyle(Style))
            throw new StyleNotFoundException(Style);

        return ApplyStyle(ThemesDictionary.GetStyle(Style)!);
    }

    public GUIStyle ApplyStyle(GUIStyle apply)
    {
        if (apply == null)
            throw new StyleNotFoundException(Style);
        
        GUIStyle style = new GUIStyle(apply)
        {
            fontSize = FontSize,
            padding = Padding,
            alignment = TextAlignment,
            richText = RichText,
            wordWrap = WordWrap,
            margin = Margin,
            border = Border,
            fontStyle = FontStyle,
            font = Font,
            fixedHeight = FixedHeight,
            fixedWidth = FixedWidth,
            contentOffset = Offset,
            stretchHeight = ExpandHeight,
            stretchWidth = ExpandWidth, 
        };
        return style;
    }

    #endregion

    #region Content

    public GUILayoutOption[] LayoutOptions { get; set; }
    public Control? ParentControl { get; set; }
    private Dictionary<int, Control> ChildControls { get; }
    private List<Control> ChildControlsList { get; }
    public GUIContent Content { get; set; }

    public void RenderControl()
    {
        PreDraw();
        Draw();
        PostDraw();
    }

    protected virtual void PreDraw()
    {
        
    }
    
    protected abstract void Draw();

    protected virtual void PostDraw()
    {
        
    }

    public Vector2 GetSize()
    {
        return GetStyle().CalcSize(Content);
    }

    #endregion

    #region Construction

    public Control(int id, GUIContent content, params Control[] children)
    {
        Id = id;
        Content = content;
        ChildControls = new Dictionary<int, Control>();
        ChildControlsList = new List<Control>();
        AddRange(children);
        
        foreach (Control control in this)
        {
            control.ParentControl = this;
        }
        
        LayoutOptions = new GUILayoutOption[0];

        GUIStyle baseStyle = ThemesDictionary.GetStyle(Style);
        if (baseStyle == null)
            throw new StyleNotFoundException(Style);
        
        // initialize properties to default values
        // we do this to avoid issues
        FontSize = baseStyle.fontSize;
        Padding = baseStyle.padding;
        TextAlignment = baseStyle.alignment;
        RichText = baseStyle.richText;
        Margin = baseStyle.margin;
        WordWrap = baseStyle.wordWrap;
        Border = baseStyle.border;
        FontStyle = baseStyle.fontStyle;
        Font = baseStyle.font;
        FixedHeight = baseStyle.fixedHeight;
        FixedWidth = baseStyle.fixedWidth;
        ExpandHeight = baseStyle.stretchHeight;
        ExpandWidth = baseStyle.stretchWidth;
    }

    public Control(int id) : this(id, GUIContent.none)
    {
    }

    public Control(int id, Texture content) : this(id, new GUIContent(content))
    {
    }

    public Control(int id, string content) : this(id, new GUIContent(content))
    {
    }

    /// <summary>
    /// This is called when a control is first added to the <see cref="AeroInterfaceManager"/>
    /// </summary>
    public virtual void OnCreation()
    {
    }

    /// <summary>
    /// This is called when a control is removed directly from <see cref="AeroInterfaceManager"/>(e.g a window is closed)
    /// </summary>
    public virtual void OnDestruction()
    {
    }

    #endregion

    #region Collection

    public int Count => ChildControls.Count;
    public bool IsReadOnly => false;

    public IEnumerator<Control> GetEnumerator()
    {
        return ChildControlsList.GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }

    public Control? GetControl(int id)
    {
        if (!ChildControls.ContainsKey(id))
            return null;

        return ChildControls[id];
    }

    public int IndexOf(Control item) => ChildControlsList.IndexOf(item);

    public int IndexOf(int id)
    {
        Control? control = GetControl(id);
        if (control == null)
            return -1;

        return IndexOf(control);
    }

    public void Add(Control item)
    {
        ChildControls.Add(item.Id, item);
        ChildControlsList.Add(item);
        item.ParentControl = this;
    }

    public void AddRange(IEnumerable<Control> items)
    {
        foreach (Control item in items)
        {
            Add(item);
        }
    }

    public void Clear()
    {
        foreach (Control control in this)
        {
            control.ParentControl = null;
        }
        
        ChildControls.Clear();
        ChildControlsList.Clear();
    }

    public bool Contains(int id) => ChildControls.ContainsKey(id);
    public bool Contains(Control item) => ChildControls.ContainsKey(item.Id);

    /// <inheritdoc />
    /// <summary>
    /// TODO: is this how you actually implement this lol?
    /// </summary>
    public void CopyTo(Control[] array, int arrayIndex)
    {
        for (int i = 0; i < ChildControls.Count; i++, arrayIndex++)
        {
            array.SetValue(ChildControls[i], arrayIndex);
        }
    }

    public bool Remove(Control item)
    {
        if (ChildControls.Remove(item.Id) && ChildControlsList.Remove(item))
            return true;

        return false;
    }

    #endregion
}