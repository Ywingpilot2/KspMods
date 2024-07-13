using SteelLanguage.Exceptions;
using UnityEngine;

namespace ProgrammableMod.Scripting.Terms.Graphmatics.Curves; // shutup rider "graphmatics" is 100% correct spelling

/// <summary>
/// My beloved
/// </summary>
internal struct PiecewiseCurve : IPiecewise, IConfigNode
{
    public Vector2[] Points { get; }

    public float GetValue(float x)
    {
        // TODO: Is this how we should be doing this? I forgor how the math behind piecewise curves actually works
        if (x <= Points[0].x)
            return Points[0].y;
        
        if (x >= Points[Points.Length - 1].x)
            return Points[Points.Length - 1].y;

        for (int i = 1; i < Points.Length; i++)
        {
            Vector2 p = Points[i - 1];
            Vector2 c = Points[i];

            if (x > p.x && x < c.x)
            {
                return Mathf.Lerp(p.x, c.x, x / c.x);
            }
        }

        return Points[Points.Length - 1].y;
    }
    
    /// <summary>
    /// Constructs a Piecewise curve, or a function with multiple points defined in its graph.
    /// <see cref="GetValue"/> on this type of Piecewise will be interpolated between the 2 nearest points on the curve.
    /// </summary>
    /// <param name="points">The first value will be used as a default minimum, the last value will be used as a default maximum</param>
    /// <exception cref="InvalidCompilationException">The curve has to have more then 2 points</exception>
    public PiecewiseCurve(params Vector2[] points)
    {
        if (points.Length <= 1)
            throw new InvalidCompilationException(0, "Cannot have a piecewise with less then 2 points");

        Points = points;
    }

    public void Load(ConfigNode node)
    {
        
    }

    public void Save(ConfigNode node)
    {
        throw new System.NotImplementedException();
    }
}