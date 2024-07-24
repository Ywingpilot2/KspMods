using System;
using AeroDynamicKerbalInterfaces;
using AeroDynamicKerbalInterfaces.Controls;
using AeroDynamicKerbalInterfaces.Controls.Buttons;
using AeroDynamicKerbalInterfaces.Controls.ContentControl;
using AeroDynamicKerbalInterfaces.Controls.ContentControl.Flow;
using AeroDynamicKerbalInterfaces.Controls.ContentControl.Organization;
using AeroDynamicKerbalInterfaces.Controls.ContentControl.Windows;
using AeroDynamicKerbalInterfaces.Controls.Fields;
using AeroDynamicKerbalInterfaces.Controls.Space;
using JetBrains.Annotations;
using ProgrammableMod.Scripting.Config.ScriptLibrary;
using SteelLanguage;
using UnityEngine;
using Random = System.Random;

namespace ProgrammableMod.Controls;

public sealed class CodeEditorControl : DragWindow
{
    public string Text => _codeText.Text;
    private readonly Action<ScriptCraft> _onSave;

    private readonly TextAreaControl _codeText;
    private readonly TextFieldControl _nameText;
    private readonly CodeLibraryControl _libraryControl;
    private ScriptCraft _baseCraft;

    public ScriptCraft GetCraft()
    {
        return new ScriptCraft(_nameText.Text, _codeText.Text, _baseCraft.Directory);
    }

    private void Hide()
    {
        AeroInterfaceManager.RemoveControl(Id);
    }

    private void Save()
    {
        _onSave.Invoke(GetCraft());
        if (_baseCraft.Name != GetCraft().Name)
            _baseCraft = GetCraft();
        
        KerbinSuperComputer.Library.SaveScript(GetCraft());
        Hide();
    }

    private void Load()
    {
        _libraryControl.LoadContents();
        AeroInterfaceManager.AddControl(_libraryControl);
    }

    private void OnLoaded(object sender, ScriptCraft craft)
    {
        _baseCraft = craft;
        _nameText.Text = craft.Name;
        _codeText.Text = craft.Script;
    }

    public CodeEditorControl(ScriptCraft content, string header, SteelCompiler compiler, Action<ScriptCraft> onSave) : this(
        new Random().Next(), content, header, onSave, compiler)
    {
    }

    private CodeEditorControl(int id, ScriptCraft content, string header, Action<ScriptCraft> onSave, SteelCompiler compiler) : base(id, header, new(Screen.width / 2, Screen.height / 2, 600, 450))
    {
        Random rng = new Random();
        _baseCraft = content;

        _codeText = new TextAreaControl(rng.Next(), content.Script)
        {
            LayoutOptions = new []{GUILayout.ExpandWidth(true),GUILayout.ExpandHeight(true)},
            WordWrap = false
        };
        _nameText = new TextFieldControl(rng.Next(), content.Name)
        {
            WordWrap = false,
            LayoutOptions = new []{GUILayout.ExpandWidth(true)}
        };
        _libraryControl = new CodeLibraryControl(rng.Next());
        _libraryControl.OnSelected += OnLoaded;

        Add(_nameText);
        Add(new ScrollViewControl(rng.Next(), _codeText){AutoScrollBottom = false});

        ColumnControl columnControl = new ColumnControl(rng.Next());
        columnControl.Add(new ButtonControl(rng.Next(), new GUIContent("Cancel"), (_,_) => Hide(), GUILayout.Width(115)));
        columnControl.Add(new FillerControl(rng.Next()));
        
        columnControl.Add(new ButtonControl(rng.Next(), new GUIContent("Load"), (_,_) => Load(), GUILayout.Width(150)));
        columnControl.Add(new ButtonControl(rng.Next(), new GUIContent("Save"), (_,_) => Save(), GUILayout.Width(150)));
        
        Add(columnControl);
        
        _onSave = onSave;
    }
}