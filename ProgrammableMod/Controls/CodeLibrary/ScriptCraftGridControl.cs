using System;
using System.Linq;
using AeroDynamicKerbalInterfaces.Controls;
using AeroDynamicKerbalInterfaces.Controls.Fields;
using ProgrammableMod.Scripting.Config.ScriptLibrary;
using UnityEngine;
using Random = System.Random;

namespace ProgrammableMod.Controls.CodeLibrary;

internal sealed class ScriptCraftGridControl : Control
{
    public int Selected = 0;

    internal sealed class ScriptCraftControl : Control
    {
        public bool Selected;
        public override string Style => "ButtonBase";

        private Rect _rect = new Rect();
        protected override void Draw()
        {
            bool selected = GUI.Toggle(_rect, Selected, GUIContent.none, GetStyle());
            if (selected && !Selected)
            {
                Selected = true;
                ((ScriptCraftGridControl)ParentControl)?.OnSelected(this);
            }
            
            GUILayout.BeginVertical(GUILayout.Width(200));
            foreach (Control control in this)
            {
                control.RenderControl();
            }
            GUILayout.EndVertical();

            if (Event.current.type == EventType.Repaint)
                _rect = GUILayoutUtility.GetLastRect();
        }
    
        public ScriptCraftControl(int id, string content, string name) : base(id, content)
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

            Add(new LabelControl(rng.Next(), content)
            {
                FixedHeight = 150,
                WordWrap = false,
                FontSize = 8,
            });
        }
    }

    private void OnSelected(ScriptCraftControl craftControl)
    {
        int idx = IndexOf(craftControl);
        foreach (Control control in this)
        {
            if (control.Id == craftControl.Id)
                continue;
            
            ((ScriptCraftControl)control).Selected = false;
        }

        Selected = idx;
    }

    private const int MaxCollums = 3;
    private int _horizontal = 0;
    protected override void Draw()
    {
        foreach (Control control in this)
        {
            if (_horizontal == 0)
                GUILayout.BeginHorizontal();
            
            control.RenderControl();
            _horizontal++;
            
            if (_horizontal >= MaxCollums)
            {
                _horizontal = 0;
                GUILayout.EndHorizontal();
            }
        }
        
        if (_horizontal != 0)
        {
            _horizontal = 0;
            GUILayout.EndHorizontal();
        }
    }

    public void SetContent(ScriptCraft[] crafts)
    {
        Clear();

        for (int i = 0; i < crafts.Length; i++)
        {
            ScriptCraft craft = crafts[i];
            Add(new ScriptCraftControl(craft.GetHashCode(), StripScriptText(craft.Script), craft.Name)
            {
                Selected = i == 0
            });
        }

        Selected = 0;
    }

    private string StripScriptText(string scriptText)
    {
        string[] lines = scriptText.Split('\n');
        return lines.Length > 15 ? string.Join("\n", lines.Take(14)) + "\n..." : scriptText;
    }
    
    public ScriptCraftGridControl(int id) : base(id)
    {
    }
}