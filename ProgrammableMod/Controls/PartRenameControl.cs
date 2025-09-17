using System;
using AeroDynamicKerbalInterfaces;
using AeroDynamicKerbalInterfaces.Controls;
using AeroDynamicKerbalInterfaces.Controls.Buttons;
using AeroDynamicKerbalInterfaces.Controls.ContentControl.Organization;
using AeroDynamicKerbalInterfaces.Controls.ContentControl.Windows;
using AeroDynamicKerbalInterfaces.Controls.Fields;
using AeroDynamicKerbalInterfaces.Controls.Space;
using JetBrains.Annotations;
using UnityEngine;
using Random = System.Random;

namespace ProgrammableMod.Controls;

public class PartRenameControl : HeaderWindow
{
    private readonly TextFieldControl _field;
    private readonly Action<string> _apply;

    private void Apply(object sender, EventArgs eventArgs)
    {
        if (string.IsNullOrEmpty(_field.Text))
        {
            ScreenMessages.PostScreenMessage("Name cannot be blank", 5, ScreenMessageStyle.UPPER_CENTER);
            return;
        }
        
        _apply.Invoke(_field.Text);
        Close(sender, eventArgs);
    }
    
    private void Close(object sender, EventArgs eventArgs)
    {
        AeroInterfaceManager.RemoveControl(Id);
    }

    public void Show(string name = null)
    {
        name ??= _field.Text;

        _field.Text = name;
        AeroInterfaceManager.AddControl(this);
    }

    public PartRenameControl(string name, Action<string> onApply) : base(new Random().Next(), "Set Part Name", new Rect(Screen.width / 2, Screen.height / 2, 450, 150))
    {
        Random rng = new Random();
        _apply = onApply;
        TextAlignment = TextAnchor.UpperLeft;
        FontSize = 18;

        Add(new LabelControl(rng.Next(), "Part Name:")
        {
            FontSize = 12,
            TextAlignment = TextAnchor.UpperLeft
        });
        _field = new TextFieldControl(rng.Next(), name)
        {
            ExpandWidth = true,
            WordWrap = false,
            Style = "FlatArea"
        };

        Add(_field);
        Add(new FillerControl(rng.Next()));

        ColumnControl columnControl = new ColumnControl(rng.Next());
        ButtonControl a = new ButtonControl(rng.Next(), "Cancel", Close)
        {
            FixedWidth = 150
        };
        ButtonControl b = new ButtonControl(rng.Next(), "Confirm", Apply)
        {
            FixedWidth = 150
        };
        
        columnControl.Add(a);
        columnControl.Add(new FillerControl(rng.Next()));
        columnControl.Add(b);
        
        Add(columnControl);
    }
}