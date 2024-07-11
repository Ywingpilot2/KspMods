using System.Collections.Generic;
using SteelLanguage.Token.Fields;
using SteelLanguage.Token.Functions;
using SteelLanguage.Token.Interaction;

namespace SteelLanguage.Token.Terms.Complex.Enumerators;

public class CollectionTerm : EnumeratorTerm
{
    public override string ValueType => "collection";
    protected virtual int Count { get; }

    public override IEnumerable<IFunction> GetFunctions()
    {
        foreach (IFunction function in base.GetFunctions())
        {
            yield return function;
        }

        yield return new Function("add", "void", terms =>
        {
            Add(terms[0]);
            return new ReturnValue();
        }, ContainedType);
        yield return new Function("remove", "void", terms =>
        {
            Remove(terms[0]);
            return new ReturnValue();
        }, ContainedType);
        yield return new Function("add", "void", _ =>
        {
            Clear();
            return new ReturnValue();
        });
        yield return new Function("contains", "bool", terms => new ReturnValue(Contains(terms[0]), "bool"),
            ContainedType);
    }

    public override IEnumerable<TermField> GetFields()
    {
        foreach (TermField field in base.GetFields())
        {
            yield return field;
        }

        yield return new TermField("count", "int", Count);
    }

    protected virtual void Add(BaseTerm term)
    {
    }
    
    protected virtual bool Remove(BaseTerm term)
    {
        return false;
    }

    protected virtual void Clear()
    {
    }

    protected virtual bool Contains(BaseTerm term)
    {
        return false;
    }
}