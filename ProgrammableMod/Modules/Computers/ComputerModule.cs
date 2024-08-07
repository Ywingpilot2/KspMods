using System;
using System.Collections.Generic;
using System.Globalization;
using JetBrains.Annotations;
using ProgrammableMod.Scripting.Exceptions;
using ProgrammableMod.Scripting.Terms.Vessel;
using ProgrammableMod.Scripting.Terms.Vessel.ActionGroups;
using SteelLanguage;
using UnityEngine;

namespace ProgrammableMod.Modules.Computers;

public class ComputerModule : BaseComputer, IResourceConsumer
{
    #region Module Fields

    [KSPField(guiActive = true, guiActiveEditor = true, guiName = "Tokens Available", guiActiveUnfocused = true, unfocusedRange = 25f)]
    public float tokenLimit;

    [KSPField]
    public string requiredResource;

    [KSPField]
    public double requiredConsumption = 1.0;
    
    [KSPField]
    public double maxHeat = 100;

    [KSPField]
    public bool canOverclock = false;

    [KSPField]
    public float overclockPowerMod = 1.0f;

    [KSPField(isPersistant = true)]
    public bool isOverclocked = false;

    [KSPField]
    public float overclockPercent = 0.25f;

    [KSPField]
    public int mediumMalfunctionChance = 0;
    
    [KSPField]
    public int highMalfunctionChance = 11;

    #endregion

    #region Module Events

    private float _originalLimit;
    [KSPEvent(active = false, guiActive = true, guiActiveEditor = true, guiName = "CPU Over Clocking: false", guiActiveUnfocused = true, unfocusedRange = 25f)]
    public void ToggleOverclocking()
    {
        if (!HighLogic.LoadedSceneIsFlight)
            return;

        isOverclocked = !isOverclocked;
        Events[nameof(ToggleOverclocking)].guiName = $"CPU Over Clocking: {isOverclocked}";
        OverClockTokens();
    }

    private void OverClockTokens()
    {
        if (isOverclocked)
        {
            tokenLimit += _originalLimit * overclockPercent;
        }
        else
        {
            tokenLimit = _originalLimit;
            Script = null;
            CompileScript(); // recompile the script to ensure player isnt cheating
        }
    }

    #endregion

    #region Resource Handling
    
    private List<PartResourceDefinition> _consumedResources; 

    public List<PartResourceDefinition> GetConsumedResources()
    {
        return _consumedResources;
    }

    #endregion

    #region Game Events

    public override double CalculateHeat()
    {
        if (isOverclocked)
        {
            float ratio = CalculateCost(Script) / _originalLimit;

            return maxHeat * ratio;
        }
        else
        {
            float ratio = CalculateCost(Script) / tokenLimit;

            return maxHeat * ratio;
        }
    }

    #region Temp

    private double _lastStatusUpdate = 0.0;
    public override void OnMedTemp(double coreTemp, double shutdownTemp)
    {
        if (mediumMalfunctionChance == 0)
            return;
        
        if (Rng.Next(1, mediumMalfunctionChance) != 5)
            return;

        int max = 7;
        if (coreTemp / shutdownTemp < 0.45)
            max = 3;
        
        if (Math.Abs(Time.fixedTime - _lastStatusUpdate) > 50)
        {
            SetStatus("Temperature anomaly, potential CPU skipping...", StatusKind.NotGreat);
            _lastStatusUpdate = Time.fixedTime;
        }
        
        MediumAnomaly(Rng.Next(0, max));
    }
    
    public override void OnHighTemp(double coreTemp, double shutdownTemp)
    {
        if (highMalfunctionChance == 0)
            return;
        
        if (Rng.Next(1, highMalfunctionChance) != 5)
            return;
        
        if (Math.Abs(Time.fixedTime - _lastStatusUpdate) > 50)
        {
            SetStatus("Temperature anomaly, potential corruption...", StatusKind.NotGreat);
            ScreenMessages.PostScreenMessage("Extreme temperatures detected, unexpected issues may occur");
            _lastStatusUpdate = Time.fixedTime;
        }
        
        MediumAnomaly(Rng.Next(0, 7));
        HighAnomaly(Rng.Next(0, 7));
    }

    #endregion

    #region Errors

    private double _lastHeatCycle = 0.0;
    private void MediumAnomaly(int random)
    {
        switch (random)
        {
            case 0:
            {
                if (Math.Abs(Time.fixedTime - _lastHeatCycle) < 10)
                    return;
                
                _heatCycle = true;
                _skipUntil = Time.fixedTime + Rng.Next(1, 3);
                _lastHeatCycle = Time.fixedTime;
            } break;
            case 1:
            {
                vessel.ActionGroups.ToggleGroup(KSPActionGroup.Light);
            } break;
            case 2:
            {
                vessel.ActionGroups.ToggleGroup(KSPActionGroup.Gear);
            } break;
            case 3:
            {
                vessel.ActionGroups.ToggleGroup(KSPActionGroup.Brakes);
            } break;
            case 4:
            {
                vessel.ActionGroups.ToggleGroup(KSPActionGroup.RCS);
            } break;
            case 5:
            {
                vessel.ActionGroups.ToggleGroup(KSPActionGroup.SAS);
            } break;
            case 6:
            {
                State.mainThrottle = Convert.ToSingle(Rng.NextDouble());
            } break;
        }
    }
    
    private void HighAnomaly(int random)
    {
        switch (random)
        {
            case 3:
            {
                if (Math.Abs(Time.fixedTime - _lastHeatCycle) < 50)
                    return;
                
                _heatCycle = true;
                _skipUntil = Time.fixedTime + Rng.Next(1, 10);
                _lastHeatCycle = Time.fixedTime;
            } break;
            case 4:
            {
                List<ModulePartFirework> fireworks = vessel.FindPartModulesImplementing<ModulePartFirework>();
                foreach (ModulePartFirework firework in fireworks)
                {
                    if (Rng.Next(3) > 1)
                        continue;
                    
                    firework.LaunchShell();
                }
            } break;
            case 5:
            {
                switch (Rng.Next(0,4))
                {
                    case 0:
                    {
                        shouldRun = false;
                    } break;
                    case 1:
                    {
                        ThrowException("Temperature anomalies occuring! Any unsaved progress, in progress actions, or other important functions will be inoperable until computer is turned back on.");
                    } break;
                    case 3:
                    {
                        _rateMultiplier = Rng.Next(1, 10);
                        _resetRateAt = Time.fixedTime + Rng.Next(1, 10);
                    } break;
                }
            } break;
            case 6:
            {
                switch (Rng.Next(5))
                {
                    case 0:
                    {
                        vessel.ActionGroups.ToggleGroup(KSPActionGroup.Abort);
                    } break;
                    case 1:
                    {
                        MylStagingManager stagingManager = new MylStagingManager(this);
                        stagingManager.NextStage();
                    } break;
                    case 2:
                    {
                        List<ModuleCommand> commandModules = vessel.FindPartModulesImplementing<ModuleCommand>();
                        foreach (ModuleCommand module in commandModules)
                        {
                            if (!module.hasHibernation)
                                continue;

                            module.hibernation = true;
                        }
                    } break;
                }
            } break;
        }
    }

    #endregion

    #endregion

    #region Script Events

    public override bool ValidateScript(SteelScript script, out string reason)
    {
        float total = CalculateCost(script);
        if (total > tokenLimit)
        {
            reason = $"Token limit surpassed! Current cost: {total}";
            return false;
        }

        tokensUsed = total == 0 ? "none" : total.ToString(CultureInfo.CurrentCulture);
        
        
        if (HighLogic.LoadedSceneIsFlight && !vessel.IsFirstFrame() && (vessel.Connection.GetControlLevel() == Vessel.ControlLevel.NONE ||
                                                                     vessel.Connection.GetControlLevel() == Vessel.ControlLevel.PARTIAL_UNMANNED))
        {
            throw new ControlLostException(0);
        }
        
        reason = "working";
        return true;
    }

    private static float CalculateCost(SteelScript script)
    {
        if (script == null)
            return 0.0f;
        
        int termCost = script.TermTokens * 5;
        int callCost = script.CallTokens;
        float keyCost = script.KeyTokens * 0.5f;
        float total = termCost + callCost + keyCost;

        return total;
    }

    private bool _heatCycle = false;
    private double _skipUntil = 0.0;
    protected override bool ShouldSkipCycle()
    {
        if (_heatCycle)
        {
            if (Time.fixedTime > _skipUntil)
            {
                _heatCycle = false;
                return false;
            }
            
            if (Math.Abs(Time.fixedTime - _skipUntil) > 0.5)
            {
                return true;
            }
            
            _heatCycle = false;
        }
        
        return false;
    }

    #endregion

    #region Display
    
    [KSPField(guiActive = true, guiActiveEditor = true, guiName = "Tokens Used", guiActiveUnfocused = true, unfocusedRange = 25f)] [UsedImplicitly]
    public string tokensUsed;

    public override string GetModuleDisplayName() => "Processing Chips";

    public override string GetInfo()
    {
        string message = "Computer chips for processing tokens and instructions. Warranty void if ingested.";
        message += canOverclock
            ? $"\n- Token limit of {tokenLimit}, can be increased by {tokenLimit + (_originalLimit * overclockPercent)} if overclocked"
            : $"\n- Token limit of {tokenLimit}";

        if (requiredConsumption != 0)
        {
            message += $"\n- Consumes {requiredConsumption} {KSPUtil.PrintModuleName(requiredResource)} per token";
        }
        
        if (createsHeat)
        {
            message += "\n- Produces heat";
        }

        if (canOverclock)
        {
            message += "\n- Limiter removed, warranty voided";
        }

        return message;
    }

    #endregion

    #region Logic

    public override void OnAwake()
    {
        base.OnAwake();
        tokensUsed = "none";
        _originalLimit = tokenLimit;

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
        if (string.IsNullOrEmpty(requiredResource))
            requiredResource = "ElectricCharge";

        if (resHandler.inputResources.Count != 0)
            return;

        ModuleResource resource = new()
        {
            name = requiredResource,
            title = KSPUtil.PrintModuleName(requiredResource),
            id = requiredResource.GetHashCode(),
            rate = requiredConsumption
        };
        
        resHandler.inputResources.Add(resource);

        if (canOverclock)
        {
            Events[nameof(ToggleOverclocking)].active = true;
            Events[nameof(ToggleOverclocking)].guiName = $"CPU Over Clocking: {isOverclocked}";
        }
        else
        {
            Events[nameof(ToggleOverclocking)].active = false;
        }
        
        if (isOverclocked)
            OverClockTokens();
        
        base.OnStart(state);
    }

    private double _rateMultiplier = 1.0;
    private double _resetRateAt = 0.0;
    private void FixedUpdate()
    {
        if (HighLogic.LoadedSceneIsFlight)
        {
            if (Math.Abs(Time.fixedTime - _resetRateAt) < 0.5)
                _rateMultiplier = 1.0;
            
            string error = "";
            double rate = shouldRun ? CalculateCost(Script) : 0.0;
            rate *= _rateMultiplier;

            if (isOverclocked)
                rate *= overclockPowerMod;
        
            if (!resHandler.UpdateModuleResourceInputs(ref error, rate, 0.9, true) && shouldRun)
            {
                ThrowException("Computer has ran out of power! Any unsaved progress, in progress actions, or other important functions will be inoperable until computer is turned back on");
            }
        }
    }

    #endregion
}