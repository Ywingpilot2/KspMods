﻿using AeroDynamicKerbalInterfaces;
using AeroDynamicKerbalInterfaces.Controls;
using AeroDynamicKerbalInterfaces.Controls.Buttons;
using AeroDynamicKerbalInterfaces.Controls.ContentControl;
using AeroDynamicKerbalInterfaces.Controls.Fields;
using AeroDynamicKerbalInterfaces.Controls.Space;
using JetBrains.Annotations;
using UnityEngine;

namespace ProgrammableMod.Controls;

public sealed class ExceptionBoxControl : DragWindow
{
    public static void Show(string message)
    {
        AeroInterfaceManager.AddControl(new ExceptionBoxControl(new System.Random().Next(), message));
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
    }
}