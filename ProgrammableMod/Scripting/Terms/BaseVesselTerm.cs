using System.Collections.Generic;
using ProgrammableMod.Modules.Computers;
using SteelLanguage.Exceptions;
using SteelLanguage.Token.Functions.Single;
using SteelLanguage.Token.Terms;

namespace ProgrammableMod.Scripting.Terms;

public abstract class BaseVesselTerm : BaseTerm
{
    internal BaseComputer Computer;

    public override IEnumerable<TermConstructor> GetConstructors()
    {
        throw new TypeNotConstructableException(0, ValueType);
    }

    protected virtual void ExtraBuilding()
    {
    }
    
    

    public override bool CopyFrom(BaseTerm term)
    {
        if (term is BaseVesselTerm vesselTerm)
        {
            Computer = vesselTerm.Computer;
            ExtraBuilding();
            return true;
        }
        
        return false;
    }

    public override bool SetValue(object value)
    {
        if (value is BaseComputer computer)
        {
            Computer = computer;
            ExtraBuilding();
            return true;
        }

        return false;
    }

    public override object GetValue()
    {
        return Computer;
    }
}