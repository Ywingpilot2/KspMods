using UnityEngine;

namespace ProgrammableMod.Scripting.Terms.Graphmatics.Curves;

internal interface IPiecewise
{
    public Vector2[] Points { get; }
    public float GetValue(float x);
}