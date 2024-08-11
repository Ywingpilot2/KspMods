using UnityEngine;

namespace ProgrammableMod.Scripting.Terms.VesselTerms.Parts;

internal readonly record struct PartField
{
    public string Name { get; }
    private readonly string _internalName;
    public UI_Control Control { get; }
    private readonly BaseFieldList _source;

    public object GetValue() => _source.GetValue(_internalName);

    public void SetValue(object value)
    {
        switch (Control)
        {
            case UI_FloatRange c:
            {
                float v = (float)value;
                SetInternal(Mathf.Clamp(v, c.minValue, c.maxValue));
            } break;
            case UI_MinMaxRange c:
            {
                Vector2 v = (Vector2)value;
                SetInternal(new Vector2(Mathf.Clamp(v.x, c.minValueX, c.maxValueX), Mathf.Clamp(v.y, c.minValueY, c.maxValueY)));
            } break;
            case UI_Toggle c:
            {
                SetInternal((bool)value);
            } break;
        }
    }

    public string TypeNameFromControl()
    {
        return Control switch
        {
            UI_FloatRange => "float",
            UI_MinMaxRange => "vec2",
            UI_Toggle => "bool",
            _ => "term"
        };
    }

    private void SetInternal(object value)
    {
        _source.SetValue(_internalName, value);
    }

    public PartField(string name, string internalName, UI_Control control, BaseFieldList source)
    {
        Name = name;
        _internalName = internalName;
        Control = control;
        _source = source;
    }

    public PartField(BaseField field, BaseFieldList source) : this(field.guiName, field.name, field.uiControlFlight,
        source)
    {
    }
}