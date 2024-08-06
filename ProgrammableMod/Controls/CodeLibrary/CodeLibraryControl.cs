using System;
using System.Linq;
using AeroDynamicKerbalInterfaces;
using AeroDynamicKerbalInterfaces.Controls.Buttons;
using AeroDynamicKerbalInterfaces.Controls.ContentControl.Flow;
using AeroDynamicKerbalInterfaces.Controls.ContentControl.Organization;
using AeroDynamicKerbalInterfaces.Controls.ContentControl.Windows;
using AeroDynamicKerbalInterfaces.Controls.Space;
using ProgrammableMod.Scripting.Config.ScriptLibrary;
using UnityEngine;
using Random = System.Random;

namespace ProgrammableMod.Controls.CodeLibrary;

internal sealed class CodeLibraryControl : DragWindow
{
    private ScriptDirectory _craftsList; // TODO: set stuff up for subfolders
    private readonly ScriptCraftGridControl _grid;

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

    public void LoadContents()
    {
        _craftsList = KerbinSuperComputer.Library.GetScriptCrafts();

        ScriptCraft[] contents = new ScriptCraft[_craftsList.Crafts.Count];
        for (int i = 0; i < _craftsList.Crafts.Count; i++)
        {
            contents.SetValue(_craftsList.Crafts[i], i);
        }

        _grid.SetContent(contents);
    }

    public CodeLibraryControl(int id) : base(id, "Select Script", new Rect(Screen.width / 2, Screen.height / 2, 625, 460))
    {
        TextAlignment = TextAnchor.UpperLeft;

        Random rng = new(GetHashCode());
        ScrollViewControl scrollView = new ScrollViewControl(rng.Next())
        {
            AutoScrollBottom = false
        };

        _grid = new ScriptCraftGridControl(rng.Next());

        scrollView.Add(_grid);

        Add(scrollView);

        ColumnControl columns = new ColumnControl(rng.Next());
        columns.Add(new ButtonControl(rng.Next(), "Cancel", (_, _) => Close()){FixedWidth = 150});
        columns.Add(new FillerControl(rng.Next()));
        columns.Add(new ButtonControl(rng.Next(), "Load", (_, _) => Load()){FixedWidth = 150});
        Add(columns);
    }
}