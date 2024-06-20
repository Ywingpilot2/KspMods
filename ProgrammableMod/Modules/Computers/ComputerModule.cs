using System.Globalization;
using ActionLanguage;
using JetBrains.Annotations;

namespace ProgrammableMod.Modules.Computers;

public class ComputerModule : BaseComputer
{
    [KSPField]
    public float tokenLimit;

    [KSPField(guiActive = true, guiActiveEditor = true, guiName = "Tokens Avaliable")] [UsedImplicitly]
    public string tokenField;

    [KSPField(guiActive = true, guiActiveEditor = true, guiName = "Tokens Used")] [UsedImplicitly]
    public string tokensUsed;

    public override void OnAwake()
    {
        base.OnAwake();
        tokenField = tokenLimit.ToString(CultureInfo.CurrentCulture);
        tokensUsed = "none";
    }

    public override bool ValidateScript(ActionScript script, out string reason)
    {
        float total = CalculateCost(script);
        tokensUsed = total.ToString(CultureInfo.CurrentCulture);
        if (total > tokenLimit)
        {
            reason = $"Token limit surpassed! Current cost: {total}";
            return false;
        }
        
        reason = "working";
        return true;
    }

    protected float CalculateCost(ActionScript script)
    {
        int termCost = script.TermTokens * 5;
        int callCost = script.CallTokens;
        float keyCost = script.KeyTokens * 0.5f;
        float total = termCost + callCost + keyCost;

        return total;
    }
}