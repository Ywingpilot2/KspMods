using ActionLanguage;

namespace ProgrammableMod.Modules.Computers;

public class ComputerModule : BaseComputer
{
    public override bool ValidateScript(ActionScript script)
    {
        return true;
    }
}