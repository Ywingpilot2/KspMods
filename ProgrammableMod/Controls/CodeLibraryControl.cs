using System;
using System.Collections.Generic;
using System.Linq;
using AeroDynamicKerbalInterfaces;
using AeroDynamicKerbalInterfaces.Controls;
using AeroDynamicKerbalInterfaces.Controls.Buttons;
using AeroDynamicKerbalInterfaces.Controls.ContentControl.Flow;
using AeroDynamicKerbalInterfaces.Controls.ContentControl.Organization;
using AeroDynamicKerbalInterfaces.Controls.ContentControl.Windows;
using AeroDynamicKerbalInterfaces.Controls.Fields;
using AeroDynamicKerbalInterfaces.Controls.Space;
using JetBrains.Annotations;
using ProgrammableMod.Scripting.Config.ScriptLibrary;
using UnityEngine;
using Random = System.Random;

namespace ProgrammableMod.Controls;

public sealed class CodeLibraryControl : DragWindow
{
    private ScriptDirectory _craftsList; // TODO: set stuff up for subfolders
    private const int MaxCollumns = 4;
    private readonly SelectionGridControl _grid;

    public event EventHandler<ScriptCraft> OnSelected;

    private void Load()
    {
        OnSelected?.Invoke(this, _craftsList.Crafts[_grid.Selected]);
        Close();
    }

    private void Close()
    {
        AeroInterfaceManager.RemoveControl(Id);
    }

    private const int MaxLength = 15;
    public void LoadContents()
    {
        _craftsList = KerbinSuperComputer.Library.GetScriptCrafts();
        
        GUIContent[] contents = new GUIContent[_craftsList.Crafts.Count];
        for (int i = 0; i < _craftsList.Crafts.Count; i++)
        {
            ScriptCraft current = _craftsList.Crafts[i];
            string[] lines = current.Script.Split('\n');
            string shortened = lines.Length > MaxLength ? string.Join("\n", lines.Take(MaxLength - 1)) + "..." : current.Script;
            
            contents.SetValue(new GUIContent(shortened, current.Name), i);
        }

        _grid.Contents = contents;
        _grid.Selected = 0;
    }

    public CodeLibraryControl(int id) : base(id, "Select Script", new Rect(Screen.width / 2, Screen.height / 2, 540, 460))
    {
        TextAlignment = TextAnchor.UpperLeft;

        Random rng = new(GetHashCode());
        ScrollViewControl scrollView = new ScrollViewControl(rng.Next())
        {
            AutoScrollBottom = false
        };

        _grid = new SelectionGridControl(rng.Next(), MaxCollumns)
        {
            FixedHeight = 150,
            FixedWidth = 150,
            FontSize = 8,
            TextAlignment = TextAnchor.UpperLeft,
            WordWrap = false
        };

        scrollView.Add(_grid);

        Add(scrollView);

        ColumnControl columns = new ColumnControl(rng.Next());
        columns.Add(new ButtonControl(rng.Next(), "Cancel", (_, _) => Close()));
        columns.Add(new FillerControl(rng.Next()));
        columns.Add(new ButtonControl(rng.Next(), "Load", (_, _) => Load()));
        Add(columns);
    }
}

public sealed class ScriptCraftControl : Control
{
    public override void Draw()
    {
        GUILayout.BeginVertical();
        foreach (Control control in this)
        {
            control.Draw();
        }
        GUILayout.EndVertical();
    }
    
    public ScriptCraftControl(int id, string text, string name) : base(id)
    {
        Random rng = new Random();
        
        Add(new LabelControl(rng.Next(), name)
        {
            Style = "LabelHead",
            FontSize = 14,
            TextAlignment = TextAnchor.MiddleCenter,
            FontStyle = FontStyle.Bold,
            ExpandWidth = true,
        });

        Add(new TextAreaControl(rng.Next(), text)
        {
            ReadOnly = true,
            FixedHeight = 150,
            //FixedWidth = 150,
            WordWrap = false,
            FontSize = 8,
        });
    }
}