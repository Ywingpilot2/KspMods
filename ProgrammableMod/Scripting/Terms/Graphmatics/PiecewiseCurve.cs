using System.Collections.Generic;
using ActionLanguage.Exceptions;
using ActionLanguage.Library;
using ActionLanguage.Reflection;
using ActionLanguage.Token.Functions;
using ActionLanguage.Token.Interaction;
using ActionLanguage.Token.Terms;
using ActionLanguage.Token.Terms.Complex.Enumerators;
using UnityEngine;

namespace ProgrammableMod.Scripting.Terms.Graphmatics; // shutup rider "graphmatics" is 100% correct spelling

public interface IPiecewise
{
    public Vector2[] Points { get; }
    public float GetValue(float x);
}

/// <summary>
/// My beloved
/// </summary>
public struct PiecewiseCurve : IPiecewise
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
}

public struct PiecewiseLinear : IPiecewise
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

/// <summary>
/// TODO: <see cref="PiecewiseLinear"/>
/// </summary>
public class PiecewiseCTerm : BaseTerm
{
    public override string ValueType => "piecewise";
    private IPiecewise _piecewise;

    public override IEnumerable<TermConstructor> GetConstructors()
    {
        yield return new TermConstructor(terms =>
        {
            ArrayTerm arrayTerm = (ArrayTerm)terms[0];

            TermArray array = (TermArray)arrayTerm.Value;
            Vector2[] vecs = new Vector2[array.Length];
            for (int i = 0; i < array.Length; i++)
            {
                vecs.SetValue((Vector2)array.GetValue(i).GetValue(), i);
            }

            return new ReturnValue(new PiecewiseCurve(vecs), "piecewise");
        }, "array<vec2>");
    }

    public override IEnumerable<IFunction> GetFunctions()
    {
        foreach (IFunction function in base.GetFunctions())
        {
            yield return function;
        }

        yield return new Function("get", "float",
            terms => new ReturnValue(_piecewise.GetValue(terms[0].CastToFloat()), "float"),
            "float");
    }

    public override bool SetValue(object value)
    {
        if (value is IPiecewise piecewise)
        {
            _piecewise = piecewise;
            return true;
        }

        return false;
    }

    public override bool CopyFrom(BaseTerm term)
    {
        if (term is PiecewiseCTerm piecewiseCTerm)
        {
            _piecewise = piecewiseCTerm._piecewise;
            return true;
        }

        return false;
    }

    public override object GetValue()
    {
        return _piecewise;
    }
}