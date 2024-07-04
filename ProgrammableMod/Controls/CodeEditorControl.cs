using System;
using AeroDynamicKerbalInterfaces;
using AeroDynamicKerbalInterfaces.Controls;
using AeroDynamicKerbalInterfaces.Controls.Buttons;
using AeroDynamicKerbalInterfaces.Controls.ContentControl;
using AeroDynamicKerbalInterfaces.Controls.ContentControl.Organization;
using AeroDynamicKerbalInterfaces.Controls.Fields;
using JetBrains.Annotations;
using UnityEngine;
using Random = System.Random;

namespace ProgrammableMod.Controls;

public sealed class CodeEditorControl : Control
{
    public string Text => _textControl.Text;
    private Action<CodeEditorControl> _onSave;
    private Action<CodeEditorControl> _onClose;
    private TextAreaControl _textControl;
    
    public override void Draw()
    {
        foreach (Control control in this)
        {
            control.Draw();
        }
    }
    
    public static void Show(string content, string header, Action<CodeEditorControl> onSave, Action<CodeEditorControl> onClose)
    {
        Random rng = new Random();
        CodeEditorControl codeEditor = new CodeEditorControl(rng.Next(), content, header, onSave, onClose);
        AeroInterfaceManager.AddControl(codeEditor);
    }

    public void Hide()
    {
        _onClose.Invoke(this);
        AeroInterfaceManager.RemoveControl(Id);
    }

    private void Save()
    {
        _onSave.Invoke(this);
        Hide();
    }

    public CodeEditorControl(int id, string code, string content, Action<CodeEditorControl> onSave, Action<CodeEditorControl> onClose) : base(id)
    {
        Random rng = new Random();
        _textControl = new TextAreaControl(rng.Next(), code)
        {
            LayoutOptions = new []{GUILayout.ExpandWidth(true),GUILayout.ExpandHeight(true)}
        };

        WindowControl control = new WindowControl(rng.Next(), new GUIContent(content),
            new(Screen.width / 2, Screen.height / 2, 600, 450),
            new ScrollViewControl(rng.Next(), _textControl))
            {
                FontSize = 18,
            };

        ColumnControl columnControl = new ColumnControl(rng.Next(),
            new ButtonControl(rng.Next(), "Cancel", (_,_) => Hide()),
            new ButtonControl(rng.Next(), "Save", (_,_) => Save()));
        control.Add(columnControl);
        
        Add(control);
        _onSave = onSave;
        _onClose = onClose;
    }
}