using System.Collections.Generic;
using ProgrammableMod.Modules.Computers;
using SteelLanguage.Exceptions;
using SteelLanguage.Reflection;
using SteelLanguage.Token.Terms;
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