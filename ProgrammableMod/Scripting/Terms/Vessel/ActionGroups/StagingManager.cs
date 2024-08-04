using ProgrammableMod.Modules.Computers;
using SteelLanguage.Exceptions;
using UniLinq;

namespace ProgrammableMod.Scripting.Terms.Vessel.ActionGroups;

/// <summary>
/// Util class used for managing vessel stages
/// </summary>
internal class MylStagingManager
{
    private readonly BaseComputer _computer;

    #region Current stage info

    public double CurrentStageMass() => GetStageMass(_computer.vessel.currentStage);
    public double CurrentDryMass() => GetStageDryMass(_computer.vessel.currentStage);

    public float GetCurrentDeltaV() => GetStageDeltaV(_computer.vessel.currentStage);
    public double GetCurrentBurnTime() => GetStageBurnTime(_computer.vessel.currentStage);

    #endregion

    #region Stage Info

    public DeltaVStageInfo GetStage(int stage)
    {
        if (!ValidateStage(stage))
            stage = 0;

        return _computer.vessel.VesselDeltaV.OperatingStageInfo[stage];
    }

    private double GetStageBurnTime(int stage)
    {
        if (!ValidateStage(stage))
            stage = 0;
        
        DeltaVStageInfo info = _computer.vessel.VesselDeltaV.OperatingStageInfo[stage];
        if (info == null)
            return 0.0f;
        
        return info.stageBurnTime;
    }

    private float GetStageDeltaV(int stage)
    {
        if (!ValidateStage(stage))
            stage = 0;
        
        DeltaVStageInfo info = _computer.vessel.VesselDeltaV.OperatingStageInfo[stage];
        if (info == null)
            return 0.0f;

        return info.deltaVActual;
    }
    
    private double GetStageMass(int stage)
    {
        if (!ValidateStage(stage))
            stage = 0;
        
        DeltaVStageInfo info = _computer.vessel.VesselDeltaV.OperatingStageInfo[stage];
        if (info == null)
            return _computer.vessel.totalMass;

        return info.stageMass;
    }

    private double GetStageDryMass(int stage)
    {
        if (!ValidateStage(stage))
            stage = 0;
        
        DeltaVStageInfo info = _computer.vessel.VesselDeltaV.OperatingStageInfo[stage];
        if (info == null)
            return _computer.vessel.totalMass;

        return info.dryMass;
    }

    #endregion

    public DeltaVStageInfo NextStage()
    {
        DeltaVStageInfo info = GetStage(_computer.vessel.currentStage - 1);
        
        int current = _computer.vessel.currentStage - 1;
        foreach (Part part in _computer.vessel.parts)
        {
            if (part.inverseStage == current)
                part.force_activate();
        }

        return info;
    }

    public bool ValidateStage(int stage)
    {
        if (stage == -1)
            return false;

        if (_computer.vessel.VesselDeltaV.OperatingStageInfo.ElementAtOrDefault(stage) == null)
        {
            return false;
        }

        if (_computer.vessel.VesselDeltaV.OperatingStageInfo.Count == 0)
            throw new InvalidActionException(0, "Vessel has no stages");

        return true;
    }

    public MylStagingManager(BaseComputer computer)
    {
        _computer = computer;
    }
}
