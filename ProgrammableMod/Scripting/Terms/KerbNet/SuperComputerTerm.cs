using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using ProgrammableMod.Scripting.Exceptions;
using ProgrammableMod.Scripting.ValueStasher;
using SteelLanguage.Reflection;
using SteelLanguage.Token.Functions;
using SteelLanguage.Token.Interaction;
using SteelLanguage.Token.Terms;

namespace ProgrammableMod.Scripting.Terms.KerbNet;

[SuppressMessage("ReSharper", "HeapView.BoxingAllocation")]
public class SuperComputerTerm : BaseTerm
{
    public override string ValueType => "kerfur";

    public override IEnumerable<IFunction> GetFunctions()
    {
        yield return new Function("has_value", "bool",
            terms => new ReturnValue(KerbinSuperComputer.CurrentStasher.HasValue(terms[0].CastToStr()), "bool"),
            "string");

        yield return new Function("get_value", "term", terms =>
        {
            ProtoStash stash = KerbinSuperComputer.CurrentStasher.GetValue(terms[0].CastToStr());

            if (stash.IsBasic)
            {
                return new ReturnValue(stash.GetBasicValue(), stash.ValueType);
            }

            TermType type = TypeLibrary.GetTermType(stash.ValueType);
            BaseTerm term = type.Construct(null, 0, TypeLibrary);
            if (!((IStashableTerm)term).Load(stash.Node))
                throw new StashableInvalidException(0, stash);

            return new ReturnValue(term.GetValue(), stash.ValueType);
        }, "string");

        yield return new Function("can_stash", "bool",
            terms => new ReturnValue(KerbinSuperComputer.CurrentStasher.CanStashType(terms[0]), "bool"), "term");

        yield return new Function("stash_value", terms =>
        {
            KerbinSuperComputer.CurrentStasher.StashValue(terms[0].CastToStr(), terms[1]);
        }, "string" ,"term");
    }

    public override bool SetValue(object value)
    {
        return true;
    }

    public override bool CopyFrom(BaseTerm term)
    {
        return true;
    }

    public override object GetValue()
    {
        return this;
    }
}