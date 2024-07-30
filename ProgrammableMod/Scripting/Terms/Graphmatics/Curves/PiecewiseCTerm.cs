using System.Collections.Generic;
using SteelLanguage.Library.System.Terms.Complex.Enumerators;
using SteelLanguage.Token.Functions;
using SteelLanguage.Token.Functions.Single;
using SteelLanguage.Token.Interaction;
using SteelLanguage.Token.Terms;
using UnityEngine;

namespace ProgrammableMod.Scripting.Terms.Graphmatics.Curves;

/// <summary>
/// TODO: <see cref="PiecewiseLinear"/>
/// </summary>
internal class PiecewiseCTerm : BaseTerm
{
    public override string ValueType => "piecewise";
    private IPiecewise _piecewise;

    public override IEnumerable<TermConstructor> GetConstructors()
    {
        yield return new TermConstructor(terms =>
        {
            ArrayTerm arrayTerm = (ArrayTerm)terms[0];

            TermArray array = (TermArray)arrayTerm.GetEnumerableValue();
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