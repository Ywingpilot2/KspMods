using AeroDynamicKerbalInterfaces;
using AeroDynamicKerbalInterfaces.Controls;
using AeroDynamicKerbalInterfaces.Controls.Buttons;
using AeroDynamicKerbalInterfaces.Controls.ContentControl;
using AeroDynamicKerbalInterfaces.Controls.Fields;
using AeroDynamicKerbalInterfaces.Controls.Space;
using JetBrains.Annotations;
using UnityEngine;

namespace ProgrammableMod.Controls;

public sealed class ExceptionBoxControl : Control
{
    public override void Draw()
    {
        foreach (Control control in this)
        {
            control.Draw();
        }
    }

    public static void Show(string message)
    {
        AeroInterfaceManager.AddControl(new ExceptionBoxControl(new System.Random().Next(), message));
    }

    private void Close()
    {
        AeroInterfaceManager.RemoveControl(Id);
    }
    
    public ExceptionBoxControl(int id, string message) : base(id)
    {
        System.Random rng = new System.Random(GetHashCode());
        WindowControl window = new WindowControl(rng.Next(), new GUIContent("Error Window"), new(Screen.width / 2, Screen.height / 2, 400, 250), 
            new LabelControl(rng.Next(), "An error has occured! Message:"),
            new LabelControl(rng.Next(), message), 
            new FillerControl(rng.Next()), 
            new ButtonControl(rng.Next(), "OK", _ => Close()));
        
        Add(window);
    }
}