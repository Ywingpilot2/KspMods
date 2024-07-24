using System;
using System.Collections.Generic;
using ProgrammableMod.Modules.Computers;

namespace ProgrammableMod.Modules.ComputerTemp;

public class ModuleComputerHeat : ModuleCoreHeat
{
    private readonly Random _rng = new Random();
    
    public override double CoreTemperature
    {
        get
        {
            if (double.IsNaN(CoreThermalEnergy))
            {
                CheckStartingTemperature();
            }

            double temp = CoreThermalEnergy * part.thermalMass;
            if (!HighLogic.LoadedSceneIsFlight)
                return temp;
            
            BaseComputer computer = part.FindModuleImplementing<BaseComputer>();
            if (computer == null)
                return temp;

            if (!computer.createsHeat)
                return 0.0;

            double extraHeat = computer.CalculateHeat();
            extraHeat += vessel.externalTemperature * vessel.atmDensity;

            if (!computer.running)
                extraHeat /= part.thermalMass;

            return (temp + extraHeat) * part.thermalMass;
        }
    }

    public override string GetModuleDisplayName() => "CPU Overclocking";
    public override string GetInfo()
    {
        return "";
    }

    protected override bool HasActiveConverters()
    {
        if (!HighLogic.LoadedSceneIsFlight)
            return false;
        
        if (part.HasModuleImplementing<BaseComputer>() && part.FindModuleImplementing<BaseComputer>().running)
            return true;

        return false;
    }

    // TODO: how are these 2 methods actually supposed to work? what is the difference between them?
    protected override bool HasEnabledConverters() => HasActiveConverters();

    protected override void CheckCoreShutdown()
    {
        if (!HighLogic.LoadedSceneIsFlight)
            return;
        
        if (CoreTemperature <= CoreShutdownTemp)
            return;

        BaseComputer computer = part.FindModuleImplementing<BaseComputer>();
        if (computer == null)
            return;
        
        if (!computer.running)
            return;
        
        Random rng = new();
        computer.ThrowException($"Computer CPU is overheating! Any unsaved progress, in progress actions, or other important functions will be inoperable until computer is turned back on.\nError Code: {rng.Next(405)}");
    }
    
    protected override void ResolveConverterEnergy(double deltaTime)
    {
    }

    public override void UpdateConverterModuleCache()
    {
        // TODO:
    }
}