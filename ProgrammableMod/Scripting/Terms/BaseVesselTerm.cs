using System.Collections.Generic;
using ActionLanguage.Exceptions;
using ActionLanguage.Reflection;
using ActionLanguage.Token.Terms;
using ProgrammableMod.Modules.Computers;
using UnityEngine.Audio;

namespace ProgrammableMod.Scripting.Terms;

public abstract class BaseVesselTerm : BaseTerm
{
    internal BaseComputer Computer;

    public override IEnumerable<TermConstructor> GetConstructors()
    {
        throw new TypeNotConstructableException(0, ValueType);
    }

    public override bool CopyFrom(BaseTerm term)
    {
        if (term is BaseVesselTerm vesselTerm)
        {
            Computer = vesselTerm.Computer;
            return true;
        }
        
        return false;
    }

    public override bool SetValue(object value)
    {
        if (value is BaseComputer computer)
        {
            Computer = computer;
            return true;
        }

        return false;
    }

    public override object GetValue()
    {
        return Computer;
    }
}