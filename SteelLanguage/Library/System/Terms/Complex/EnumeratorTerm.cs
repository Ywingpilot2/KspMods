﻿using System;
using System.Collections;
using System.Collections.Generic;
using SteelLanguage.Token.Functions;
using SteelLanguage.Token.Interaction;
using SteelLanguage.Token.Terms;

namespace SteelLanguage.Library.System.Terms.Complex;

public class EnumeratorTerm : BaseTerm
{
    /// <summary>
    /// Programmer utility. This is exactly the same as <see cref="GetValue"/>, except it returns a <see cref="IEnumerable"/>
    /// </summary>
    /// <returns><see cref="GetValue"/> as an <see cref="IEnumerable"/></returns>
    public IEnumerable GetEnumerableValue() => (IEnumerable)GetValue();

    public override bool ContainsType => true;
    public override string[] ContainedTypeInputs => new []{"term"};

    public virtual string GetEnumerationType()
    {
        if (ContainedType == null || ContainedType.Length == 0)
            return "term";

        return ContainedType[0];
    }

    public override string ValueType => "Enumerable";

    #region Functions

    public override IEnumerable<IFunction> GetFunctions()
    {
        foreach (IFunction function in base.GetFunctions())
        {
            yield return function;
        }

        yield return new Function("next", "bool", () => new ReturnValue(GetEnumerableValue().GetEnumerator().MoveNext(), "bool"));
        yield return new Function("reset", () =>
        {
            GetEnumerableValue().GetEnumerator().Reset();
        });
        yield return new Function("get_current", "term", () => new ReturnValue(GetEnumerableValue().GetEnumerator().Current, GetEnumerationType()));
    }

    #endregion

    #region Value

    public override bool SetValue(object value)
    {
        throw new NotImplementedException();
    }

    public override bool CopyFrom(BaseTerm term)
    {
        throw new NotImplementedException();
    }

    public override object GetValue()
    {
        throw new NotImplementedException();
    }

    #endregion
}