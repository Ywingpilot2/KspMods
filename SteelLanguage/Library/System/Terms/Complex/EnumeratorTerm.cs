using System;
using System.Collections;
using System.Collections.Generic;
using SteelLanguage.Token.Functions;
using SteelLanguage.Token.Interaction;
using SteelLanguage.Token.Terms;

namespace SteelLanguage.Library.System.Terms.Complex;

public class EnumeratorTerm : BaseTerm
{
    public IEnumerable Value { get; protected set; }
    public override bool ContainsType => true;

    public override string ValueType => "Enumerable";

    #region Functions

    public override IEnumerable<IFunction> GetFunctions()
    {
        foreach (IFunction function in base.GetFunctions())
        {
            yield return function;
        }

        yield return new Function("next", "bool", () => new ReturnValue(Value.GetEnumerator().MoveNext(), "bool"));
        yield return new Function("reset", () =>
        {
            Value.GetEnumerator().Reset();
        });
        yield return new Function("get_current", "term", () => new ReturnValue(Value.GetEnumerator().Current, ContainedType));
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
        return Value;
    }

    #endregion
}