using AeroDynamicKerbalInterfaces;
using AeroDynamicKerbalInterfaces.Controls;
using AeroDynamicKerbalInterfaces.Controls.Buttons;
using AeroDynamicKerbalInterfaces.Controls.ContentControl;
using AeroDynamicKerbalInterfaces.Controls.ContentControl.Flow;
using AeroDynamicKerbalInterfaces.Controls.ContentControl.Windows;
using AeroDynamicKerbalInterfaces.Controls.Fields;
using AeroDynamicKerbalInterfaces.Controls.Space;
using JetBrains.Annotations;
using UnityEngine;

namespace ProgrammableMod.Controls;

public sealed class ExceptionBoxControl : DragWindow
{
    private readonly double _closeTime;
    
    public static void Show(string message)
    {
        AeroInterfaceManager.AddControl(new ExceptionBoxControl(new System.Random().Next(), message));
    }

    protected override void PostDraw()
    {
        base.PostDraw();
        if (Time.fixedTime >= _closeTime)
            Close();
    }

    private void Close()
    {
        AeroInterfaceManager.RemoveControl(Id);
    }

    public ExceptionBoxControl(int id, [NotNull] string message) : base(id, "Error Window", new(Screen.width / 2, Screen.height / 2, 400, 250))
    {
        System.Random rng = new System.Random(GetHashCode());
        Add(new LabelControl(rng.Next(), "An error has occured! Message:"));
        Add(new ScrollViewControl(rng.Next(), new LabelControl(rng.Next(), new GUIContent(message), GUILayout.ExpandHeight(true), GUILayout.ExpandWidth(true))));
        Add(new ButtonControl(rng.Next(), "OK", (_,_) => Close()));
        _closeTime = Time.fixedTime + 32;
    }
}