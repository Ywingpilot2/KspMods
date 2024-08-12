using System.Collections.Generic;
using ProgrammableMod.Modules.Computers;
using ProgrammableMod.Scripting.Exceptions;
using SteelLanguage.Exceptions;
using SteelLanguage.Token.Functions.Single;
using SteelLanguage.Token.Terms;

namespace ProgrammableMod.Scripting.Terms;

internal abstract class BaseComputerTerm : BaseTerm
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
        if (term is BaseComputerTerm vesselTerm)
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
    
    /// <summary>
    /// Connect to the <see cref="KerbinSuperComputer"/>!
    /// Warranty void if connection unstable.
    /// </summary>
    protected void EstablishConnection()
    {
        if (Computer != null && !Computer.compiling && !Computer.vessel.Connection.IsConnected)
            throw new KerbnetLostException(0);
    }
}