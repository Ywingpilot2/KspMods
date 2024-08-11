using ProgrammableMod.Modules.Computers;
using SteelLanguage.Exceptions;
using UniLinq;

namespace ProgrammableMod.Scripting.Terms.VesselTerms.ActionGroups;

/// <summary>
/// Util class used for managing vessel stages
/// </summary>
internal class MylStagingManager
{
    private readonly Vessel _vessel;

    #region Current stage info

    public double CurrentStageMass() => GetStageMass(_vessel.currentStage);
    public double CurrentDryMass() => GetStageDryMass(_vessel.currentStage);

    public float GetCurrentDeltaV() => GetStageDeltaV(_vessel.currentStage);
    public double GetCurrentBurnTime() => GetStageBurnTime(_vessel.currentStage);

    #endregion

    #region Stage Info

    public DeltaVStageInfo GetStage(int stage)
    {
        if (!ValidateStage(stage))
            stage = 0;

        return _vessel.VesselDeltaV.OperatingStageInfo[stage];
    }

    private double GetStageBurnTime(int stage)
    {
        if (!ValidateStage(stage))
            stage = 0;
        
        DeltaVStageInfo info = _vessel.VesselDeltaV.OperatingStageInfo[stage];
        if (info == null)
            return 0.0f;
        
        return info.stageBurnTime;
    }

    private float GetStageDeltaV(int stage)
    {
        if (!ValidateStage(stage))
            stage = 0;
        
        DeltaVStageInfo info = _vessel.VesselDeltaV.OperatingStageInfo[stage];
        if (info == null)
            return 0.0f;

        return info.deltaVActual;
    }
    
    private double GetStageMass(int stage)
    {
        if (!ValidateStage(stage))
            stage = 0;
        
        DeltaVStageInfo info = _vessel.VesselDeltaV.OperatingStageInfo[stage];
        if (info == null)
            return _vessel.totalMass;

        return info.stageMass;
    }

    private double GetStageDryMass(int stage)
    {
        if (!ValidateStage(stage))
            stage = 0;
        
        DeltaVStageInfo info = _vessel.VesselDeltaV.OperatingStageInfo[stage];
        if (info == null)
            return _vessel.totalMass;

        return info.dryMass;
    }

    #endregion

    public DeltaVStageInfo NextStage()
    {
        DeltaVStageInfo info = GetStage(_vessel.currentStage - 1);
        
        int current = _vessel.currentStage - 1;
        foreach (Part part in _vessel.parts)
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

        if (_vessel.VesselDeltaV.OperatingStageInfo.ElementAtOrDefault(stage) == null)
        {
            return false;
        }

        if (_vessel.VesselDeltaV.OperatingStageInfo.Count == 0)
            throw new InvalidActionException(0, "Vessel has no stages");

        return true;
    }

    public MylStagingManager(Vessel vessel)
    {
        _vessel = vessel;
    }
}
