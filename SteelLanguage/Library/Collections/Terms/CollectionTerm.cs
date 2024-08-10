using System.Collections.Generic;
using SteelLanguage.Library.System.Terms.Complex;
using SteelLanguage.Token.Fields;
using SteelLanguage.Token.Functions;
using SteelLanguage.Token.Interaction;
using SteelLanguage.Token.Terms;

namespace SteelLanguage.Library.Collections.Terms;

public class CollectionTerm : EnumeratorTerm
{
    public override string ValueType => "Collection";
    protected virtual int Count { get; }

    public override IEnumerable<IFunction> GetFunctions()
    {
        foreach (IFunction function in base.GetFunctions())
        {
            yield return function;
        }

        yield return new Function("add", Add, ContainedTypeInputs);
        yield return new Function("remove", terms =>
        {
            Remove(terms);
        }, ContainedTypeInputs);
        yield return new Function("clear", Clear);
    }

    public override IEnumerable<TermField> GetFields()
    {
        foreach (TermField field in base.GetFields())
        {
            yield return field;
        }

        yield return new TermField("count", "int", Count);
    }

    protected virtual void Add(BaseTerm[] term)
    {
    }
    
    protected virtual bool Remove(BaseTerm[] term)
    {
        return false;
    }

    protected virtual void Clear()
    {
    }
}