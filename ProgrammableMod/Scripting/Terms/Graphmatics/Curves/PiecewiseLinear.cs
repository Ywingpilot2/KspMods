using SteelLanguage.Exceptions;
using UnityEngine;

namespace ProgrammableMod.Scripting.Terms.Graphmatics.Curves;

internal readonly record struct PiecewiseLinear : IPiecewise
{
    public Vector2[] Points { get; }

    public float GetValue(float x)
    {
        // TODO: Is this how we should be doing this? I forgor how the math behind piecewise curves actually works
        if (x <= Points[0].x)
            return Points[0].y;
        
        if (x >= Points[Points.Length - 1].x)
            return Points[Points.Length - 1].y;

        for (int i = 0; i < Points.Length; i++)
        {
            Vector2 c = Points[i];
            if (c.x >= x)
                return c.y;
        }

        return Points[Points.Length - 1].y;
    }
    
    /// <summary>
    /// Constructs a Piecewise curve, or a function with multiple points defined in its graph.
    /// <see cref="GetValue"/> on this type of Piecewise will return the closest point on the graph
    /// </summary>
    /// <param name="points">The first value will be used as a default minimum, the last value will be used as a default maximum</param>
    /// <exception cref="InvalidCompilationException">The curve has to have more then 2 points</exception>
    public PiecewiseLinear(params Vector2[] points)
    {
        if (points.Length <= 1)
            throw new InvalidCompilationException(0, "Cannot have a piecewise with less then 2 points");

        Points = points;
    }
}