using System;
using System.Collections.Generic;
using AeroDynamicKerbalInterfaces.Controls;
using AeroDynamicKerbalInterfaces.Themes;
using KSP.UI;
using KSP.UI.Screens;
using UnityEngine;

namespace AeroDynamicKerbalInterfaces;

[KSPAddon(KSPAddon.Startup.AllGameScenes, false)]
public class AeroInterfaceManager : MonoBehaviour
{
    private static readonly Dictionary<int, Control> Controls;

    private void OnGUI()
    {
        if (!UIMasterController.Instance.IsUIShowing)
            return;

        GUI.skin = ThemesDictionary.Skin;
        foreach (Control control in Controls.Values)
        {
            control.Draw();
        }
        
        GUI.skin = null;
    }

    public static bool HasControl(int id) => Controls.ContainsKey(id);

    /// <summary>
    /// Fetches a <see cref="Control"/> and searches down the entire UI chain to find it
    /// </summary>
    /// <param name="id">The id of the <see cref="Control"/> to find</param>
    /// <returns>Null if the control was not found, otherwise the control with the specified ID</returns>
    public static Control? FetchControl(int id)
    {
        foreach (Control control in Controls.Values)
        {
            if (control.Id == id)
                return control;
            
            if (control.Contains(id))
                return control.GetControl(id);

            Control? potential = FetchFrom(control, id);
            if (potential != null)
                return potential;
        }

        return null;
    }

    private static Control? FetchFrom(Control control, int id)
    {
        foreach (Control child in control)
        {
            if (child.Id == id)
                return child;
            
            if (child.Contains(id))
                return child.GetControl(id);

            Control? potential = FetchFrom(child, id);
            if (potential != null)
                return potential;
        }
        
        return null;
    }

    public static Control? GetControl(int id)
    {
        if (!HasControl(id))
            return null;

        return Controls[id];
    }

    public static bool AddControl(Control control)
    {
        if (HasControl(control.Id))
            return false;
        
        Controls.Add(control.Id, control);
        control.OnCreation();
        return true;
    }

    public static void RemoveControl(int id)
    {
        if (!HasControl(id))
            return;
        
        Controls[id].OnDestruction();
        Controls.Remove(id);
    }

    public static IEnumerable<Control> EnumerateControls()
    {
        foreach (Control control in Controls.Values)
        {
            yield return control;
        }
    }

    public static void Clear() => Controls.Clear();

    private void Start()
    {
        Debug.Log("Aero Dynamic UI loaded!");
    }

    private void OnDestroy()
    {
        Clear();
    }

    static AeroInterfaceManager()
    {
        Controls = new Dictionary<int, Control>();
    }
}