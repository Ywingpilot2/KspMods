using System.Collections;
using System.Collections.Generic;
using ActionLanguage.Token.Fields;
using ActionLanguage.Token.Functions;
using ActionLanguage.Token.Interaction;

namespace ActionLanguage.Token.Terms.Complex;

public class EnumeratorTerm : BaseTerm
{
    public IEnumerable Value { get; set; }
    public virtual string ContainedType { get; protected set; }

    public override string ValueType => "enumerable";

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
        yield return new Function("get_current", "term", terms =>
        {
            EnumeratorTerm term = (EnumeratorTerm)terms[0];
            return new ReturnValue(term.Value.GetEnumerator().Current, term.ContainedType);
        });
    }

    public override bool SetValue(object value)
    {
        throw new System.NotImplementedException();
    }

    public override bool CopyFrom(BaseTerm term)
    {
        throw new System.NotImplementedException();
    }

    public override object GetValue()
    {
        return Value;
    }
}