using System.Collections.Generic;
using System.Globalization;
using ActionLanguage;
using JetBrains.Annotations;

namespace ProgrammableMod.Modules.Computers;

public class ComputerModule : BaseComputer, IResourceConsumer
{
    #region Display

    [KSPField(guiActive = true, guiActiveEditor = true, guiName = "Tokens Avaliable")] [UsedImplicitly]
    public string tokenField;

    [KSPField(guiActive = true, guiActiveEditor = true, guiName = "Tokens Used")] [UsedImplicitly]
    public string tokensUsed;

    #endregion

    #region Module Fields

    [KSPField]
    public float tokenLimit;

    [KSPField]
    public string requiredResource;

    [KSPField]
    public string requiredConsumption;

    #endregion

    #region Resource Handling

    private double _consumptionAmount;
    private List<PartResourceDefinition> _consumedResources; 

    public List<PartResourceDefinition> GetConsumedResources()
    {
        return _consumedResources;
    }

    #endregion

    #region Logic

    protected override void PreExecute()
    {
        string error = "";
        double rate = ShouldRun ? CalculateCost(Script) : 0.0;
        
        if (!resHandler.UpdateModuleResourceInputs(ref error, rate, 0.9, true) && ShouldRun)
        {
            ThrowException("Computer has ran out of power! Any unsaved progress, in progress actions, or other important functions will be inoperable until computer is turned back on");
        }
    }

    public override bool ValidateScript(ActionScript script, out string reason)
    {
        float total = CalculateCost(script);
        if (total > tokenLimit)
        {
            reason = $"Token limit surpassed! Current cost: {total}";
            return false;
        }
        
        tokensUsed = total.ToString(CultureInfo.CurrentCulture);
        reason = "working";
        return true;
    }

    private static float CalculateCost(ActionScript script)
    {
        int termCost = script.TermTokens * 5;
        int callCost = script.CallTokens;
        float keyCost = script.KeyTokens * 0.5f;
        float total = termCost + callCost + keyCost;

        return total;
    }

    #endregion

    #region Display

    public override string GetModuleDisplayName() => "Processor Chips";

    public override string GetInfo() => $"State of the art chips with a token limit of {tokenLimit} and consumption rate of {requiredConsumption} {requiredResource} per token.\nNOT FOR EATING!";

    #endregion

    public override void OnAwake()
    {
        base.OnAwake();
        tokenField = tokenLimit.ToString(CultureInfo.CurrentCulture);
        tokensUsed = "none";

        if (_consumedResources == null)
            _consumedResources = new List<PartResourceDefinition>();
        else
            _consumedResources.Clear();

        for (int i = 0; i < resHandler.inputResources.Count; i++)
        {
            _consumedResources.Add(PartResourceLibrary.Instance.GetDefinition(resHandler.inputResources[i].name));
        }
    }

    public override void OnStart(StartState state)
    {
        base.OnStart(state);
        
        if (string.IsNullOrEmpty(requiredResource))
            requiredResource = "ElectricCharge";

        if (string.IsNullOrEmpty(requiredConsumption))
            requiredConsumption = "1.0";

        if (!double.TryParse(requiredConsumption, NumberStyles.Float, new NumberFormatInfo(), out _consumptionAmount))
            _consumptionAmount = 1.0;
        
        if (resHandler.inputResources.Count != 0)
            return;

        ModuleResource resource = new()
        {
            name = requiredResource,
            title = KSPUtil.PrintModuleName(requiredResource),
            id = requiredResource.GetHashCode(),
            rate = _consumptionAmount
        };
        
        resHandler.inputResources.Add(resource);
    }
}