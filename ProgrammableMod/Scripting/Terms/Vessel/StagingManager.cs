using System;
using ProgrammableMod.Modules.Computers;
using UniLinq;
using UnityEngine;

namespace ProgrammableMod.Scripting.Terms.Vessel;

/// <summary>
/// Util class used for managing vessel stages
/// </summary>
public class MylStagingManager
{
    private BaseComputer _computer;
    public int CurrentStage => _computer.vessel.currentStage;

    public double CurrentStageMass() => GetStageMass(_computer.vessel.currentStage);
    public double CurrentDryMass() => GetStageDryMass(_computer.vessel.currentStage);

    public float GetCurrentDeltaV() => GetStageDeltaV(_computer.vessel.currentStage);
    public double GetCurrentBurnTime() => GetStageBurnTime(_computer.vessel.currentStage);

    public MylStageInfo GetStage(int stage)
    {
        if (_computer.vessel.VesselDeltaV.OperatingStageInfo.ElementAtOrDefault(stage) == null)
            return new MylStageInfo();
        
        return new MylStageInfo(stage, GetStageMass(stage), GetStageDryMass(stage), GetStageDeltaV(stage),
            GetStageBurnTime(stage));
    }

    private double GetStageBurnTime(int stage)
    {
        DeltaVStageInfo info = _computer.vessel.VesselDeltaV.OperatingStageInfo[stage];
        if (info == null)
            return 0.0f;

        return info.stageBurnTime;
    }

    private float GetStageDeltaV(int stage)
    {
        DeltaVStageInfo info = _computer.vessel.VesselDeltaV.OperatingStageInfo[stage];
        if (info == null)
            return 0.0f;

        return info.deltaVActual;
    }
    
    private double GetStageMass(int stage)
    {
        DeltaVStageInfo info = _computer.vessel.VesselDeltaV.OperatingStageInfo[stage];
        if (info == null)
            return _computer.vessel.totalMass;

        return info.stageMass;
    }

    private double GetStageDryMass(int stage)
    {
        DeltaVStageInfo info = _computer.vessel.VesselDeltaV.OperatingStageInfo[stage];
        if (info == null)
            return _computer.vessel.totalMass;

        return info.dryMass;
    }

    public MylStageInfo NextStage()
    {
        MylStageInfo info = GetStage(_computer.vessel.currentStage - 1);
        
        int current = _computer.vessel.currentStage - 1;
        foreach (Part part in _computer.vessel.parts)
        {
            if (part.inverseStage == current)
                part.force_activate();
        }

        return info;
    }
    
    public MylStagingManager(BaseComputer computer)
    {
        _computer = computer;
    }
}

public struct MylStageInfo
{
    public int Id { get; }
    
    public double Mass { get; }
    public double DryMass { get; }
    
    public float DeltaV { get; }
    public double BurnTime { get; }

    public MylStageInfo()
    {
        Id = -1;
    }

    public MylStageInfo(int id, double mass, double dryMass, float deltaV, double burnTime)
    {
        Id = id;
        Mass = mass;
        DryMass = dryMass;
        DeltaV = deltaV;
        BurnTime = burnTime;
    }   
}
