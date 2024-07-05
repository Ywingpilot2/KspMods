using System;
using System.Collections;
using System.Collections.Generic;
using SteelLanguage.Reflection;
using SteelLanguage.Token.Functions;
using SteelLanguage.Token.Interaction;

namespace SteelLanguage.Token.Terms.Complex;

public class EnumeratorTerm : BaseTerm
{
    public IEnumerable Value { get; set; }
    public override bool ContainsType => true;

    public override string ValueType => "enumerable";

    #region Functions

    public override IEnumerable<IFunction> GetFunctions()
    {
        foreach (IFunction function in base.GetFunctions())
        {
            yield return function;
        }

        yield return new Function("next", "bool", _ => new ReturnValue(Value.GetEnumerator().MoveNext(), "bool"));
        yield return new Function("reset", "void", _ =>
        {
            Value.GetEnumerator().Reset();
            return new ReturnValue();
        });
        yield return new Function("get_current", "term", terms => new ReturnValue(Value.GetEnumerator().Current, ContainedType));
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

    public override TermType GetTermType()
    {
        TermType type = base.GetTermType();
        type.ContainedType = ContainedType;
        return type;
    }
}