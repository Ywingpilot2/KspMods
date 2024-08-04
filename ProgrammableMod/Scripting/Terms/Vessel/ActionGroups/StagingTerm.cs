using System.Collections.Generic;
using SteelLanguage.Token.Fields;
using SteelLanguage.Token.Functions;
using SteelLanguage.Token.Interaction;
using SteelLanguage.Token.Terms;

namespace ProgrammableMod.Scripting.Terms.Vessel.ActionGroups;

internal class StagingTerm : BaseTerm
{
    public override string ValueType => "staging";
    private MylStagingManager _manager;

    public override IEnumerable<IFunction> GetFunctions()
    {
        foreach (IFunction function in base.GetFunctions())
        {
            yield return function;
        }

        yield return new Function("get_burntime", "double",
            () => new ReturnValue(_manager.GetCurrentBurnTime(), "double"));
        yield return new Function("get_deltav", "float",
            () => new ReturnValue(_manager.GetCurrentDeltaV(), "float"));
        yield return new Function("get_current_mass", "double",
            () => new ReturnValue(_manager.CurrentStageMass(), "double"));
        yield return new Function("get_dry_mass", "double",
            () => new ReturnValue(_manager.CurrentDryMass(), "double"));
        yield return new Function("get_stage", "stage", terms =>
        {
            int stage = terms[0].CastToInt();
            return new ReturnValue(_manager.GetStage(stage), "stage");
        }, "int");
        yield return new Function("next_stage", "stage", () => new ReturnValue(_manager.NextStage(), "stage"));
    }

    public override bool SetValue(object value)
    {
        if (value is MylStagingManager manager)
        {
            _manager = manager;
            return true;
        }

        return false;
    }

    public override bool CopyFrom(BaseTerm term)
    {
        if (term is StagingTerm stagingTerm)
        {
            _manager = stagingTerm._manager;
            return true;
        }
        
        return false;
    }

    public override object GetValue()
    {
        return _manager;
    }
}

internal class StageInfoTerm : BaseTerm
{
    public override string ValueType => "stage";
    private MylStageInfo _stageInfo;

    public override IEnumerable<TermField> GetFields()
    {
        foreach (TermField field in base.GetFields())
        {
            yield return field;
        }

        yield return new TermField("id", "int", _stageInfo.Id);
        yield return new TermField("mass", "double", _stageInfo.Mass);
        yield return new TermField("dry_mass", "double", _stageInfo.DryMass);
        yield return new TermField("deltav", "float", _stageInfo.DeltaV);
        yield return new TermField("burn_time", "double", _stageInfo.BurnTime);
    }

    public override bool SetValue(object value)
    {
        if (value is MylStageInfo stageInfo)
        {
            _stageInfo = stageInfo;
            return true;
        }
        
        return false;
    }

    public override bool CopyFrom(BaseTerm term)
    {
        if (term is StageInfoTerm infoTerm)
        {
            _stageInfo = infoTerm._stageInfo;
        }
        
        return false;
    }

    public override object GetValue()
    {
        return _stageInfo;
    }
}