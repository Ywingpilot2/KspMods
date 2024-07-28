using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using AeroDynamicKerbalInterfaces;
using JetBrains.Annotations;
using ProgrammableMod.Controls;
using ProgrammableMod.Extensions;
using ProgrammableMod.Modules.ComputerTemp;
using ProgrammableMod.Scripting.Config.ScriptLibrary;
using ProgrammableMod.Scripting.Exceptions;
using ProgrammableMod.Scripting.Library;
using SteelLanguage;
using SteelLanguage.Exceptions;
using SteelLanguage.Extensions;
using SteelLanguage.Library;
using SteelLanguage.Reflection.Library;
using UniLinq;
using UnityEngine;
using UnityEngine.Serialization;
using Debug = UnityEngine.Debug;
using Random = System.Random;

namespace ProgrammableMod.Modules.Computers;

public enum StatusKind
{
    Exceptional,
    NotGreat = 1,
    Uhoh = 2
}

public abstract class BaseComputer : PartModule
{
    #region Display

    [KSPField(isPersistant = false, guiActive = true, guiName = "Program Status", guiActiveEditor = true)] [UsedImplicitly]
    public string status = "Operating";
    
    [KSPField(isPersistant = false, guiActive = false, guiName = "Error")] [UsedImplicitly]
    public string exception;

    #endregion

    #region Fields

    [KSPField(isPersistant = true)]
    public TokenContainer tokenContainer;

    [KSPField(isPersistant = true)]
    public float runTime;
    
    [KSPField]
    public bool createsHeat = false;

    [KSPField]
    public double inactiveHeatModifier = 0.25;

    #endregion

    #region Misc properties

    protected ILibrary[] Libraries;

    internal FlightCtrlState State;
    private SteelCompiler _compiler;
    protected SteelScript Script;
    protected Random Rng;

    #endregion

    #region Execution
    
    private void Execution(FlightCtrlState state)
    {
        OnExecute();

        if (shouldRun && HighLogic.LoadedSceneIsFlight)
        {
            if (Script == null)
            {
                ThrowException("Script is not compiled!");
                return;
            }
            
            State = state;
            if (ShouldSkipCycle())
                return;

            try
            {
                PreExecute();

                CancellationTokenSource cancel = new CancellationTokenSource();
                cancel.CancelAfter(150);

                Parallel.Invoke(new ParallelOptions { CancellationToken = cancel.Token, MaxDegreeOfParallelism = 2 },
                    Script.Execute);
                cancel.Token.ThrowIfCancellationRequested();

                PostExecute();
            }
            catch (Exception e)
            {
                CatchException(e);
            }
        }
    }

    #endregion

    #region Logic

    public override void OnAwake()
    {
        base.OnAwake();
        tokenContainer = new TokenContainer();
        State = new FlightCtrlState(); // to prevent compiler issues
    }

    public override void OnStart(StartState state)
    {
        Libraries = new ILibrary[]
        {
            new KerbalLibrary(this),
            new VesselLibrary(this),
            new ComputerLibrary(this)
        };
        
        Rng = new Random(GetHashCode());

        _compiler = new SteelCompiler(Libraries);
        _logControl = new LogControl(Rng.Next());
        _codeEditor = new CodeEditorControl(tokenContainer.craft, "code editor", _compiler, craft => tokenContainer.craft = craft);
        if (HighLogic.LoadedSceneIsEditor)
        {
            Fields[nameof(shouldRun)].guiName = "Start On";
        }

        ResetStatus();
        if (tokenContainer.shouldCompile || shouldRun)
        {
            CompileScript();
        }

        if (HighLogic.LoadedSceneIsFlight)
        {
            AssignGameEvents();
        }
    }

    private void OnDestroy()
    {
        RemoveGameEvents();
    }

    public override void OnCopy(PartModule fromModule)
    {
        BaseComputer computer = (BaseComputer)fromModule;
        tokenContainer.craft = new ScriptCraft(computer.tokenContainer.craft.Name, computer.tokenContainer.craft.Script,
            computer.tokenContainer.craft.Directory);
        shouldRun = computer.shouldRun;
        tokenContainer.shouldCompile = computer.tokenContainer.shouldCompile;
    }

    #endregion

    #region Script Events

    public bool running;
    public bool compiling;
    
    /// <summary>
    /// Called just before <see cref="Execution"/> is called
    /// </summary>
    protected virtual void OnExecute()
    {
    }

    protected virtual void PreExecute()
    {
    }
    
    protected virtual void PostExecute()
    {
        running = true;
    }
    
    protected virtual bool ShouldSkipCycle()
    {
        return false;
    }

    protected virtual void OnCompiled()
    {
        compiling = false;
    }

    #endregion

    #region Game Events

    private void AssignGameEvents()
    {
        vessel.OnFlyByWire += Execution;
        GameEvents.onPartWillDie.Add(OnBlownUp);
    }

    private void RemoveGameEvents()
    {
        if (vessel != null)
            vessel.OnFlyByWire -= Execution;
        
        GameEvents.onPartWillDie.Remove(OnBlownUp);
    }

    private void OnBlownUp(Part data)
    {
        if (!HighLogic.LoadedSceneIsFlight)
            return;
        
        // not our crash, or bob is trying to break in again...
        if (data.vessel == null || data.isVesselEVA || data.vessel.id != part.vessel.id)
            return;
        
        if (Rng.Next(0, 10) <= 5 || data.flightID == part.flightID)
        {
            ThrowException($"Oh no, an unknown error has occured! Any unsaved progress, in progress actions, or other important functions will be inoperable until computer is turned back on.\nError Code: {Rng.Next(404)}");
        }

        if (data.flightID == part.flightID)
        {
            RemoveGameEvents();
        }
    }

    /// <summary>
    /// Calculates the heat this Computer produces while executing
    /// </summary>
    /// <returns>A double representing the temperature to add to the computer</returns>
    public virtual double CalculateHeat()
    {
        return 0.0;
    }

    /// <summary>
    /// Called when a <see cref="ModuleComputerHeat"/> has a low temperature(25% or lower)
    /// </summary>
    /// <param name="coreTemp">Core temperature</param>
    /// <param name="shutdownTemp">Temperature at which this computer will shutdown</param>
    public virtual void OnLowTemp(double coreTemp, double shutdownTemp)
    {
    }

    /// <summary>
    /// Called when a <see cref="ModuleComputerHeat"/> has a low temperature(50% to shutdown temp)
    /// </summary>
    /// <param name="coreTemp">Core temperature</param>
    /// <param name="shutdownTemp">Temperature at which this computer will shutdown</param>
    public virtual void OnMedTemp(double coreTemp, double shutdownTemp)
    {
    }

    /// <summary>
    /// Called when a <see cref="ModuleComputerHeat"/> has a high temperature(75% or higher to shutdown temp)
    /// </summary>
    /// <param name="coreTemp">Core temperature</param>
    /// <param name="shutdownTemp">Temperature at which this computer will shutdown</param>
    public virtual void OnHighTemp(double coreTemp, double shutdownTemp)
    {
    }

    #endregion
    
    #region Compilation

    [KSPEvent(active = true, guiActive = true, guiName = "Compile Script", guiActiveEditor = true, guiActiveUnfocused = true, unfocusedRange = 15f)]
    public void CompileScript()
    {
        compiling = true;
        try
        {
            SteelScript script = _compiler.Compile(tokenContainer.Tokens);
            if (!ValidateScript(script, out string reason))
            {
                ThrowException(reason);
            }

            Script = script;
            tokenContainer.shouldCompile = true;
            ResetStatus();
        }
        catch (Exception e)
        {
            CatchException(e);
        }
        finally
        {
            OnCompiled();
        }
    }
    
    [KSPAction("Compile Script")]
    public void CompileAction(KSPActionParam param)
    {
        CompileScript();
    }

    public abstract bool ValidateScript(SteelScript script, out string reason);

    #endregion

    #region UI Buttons

    private CodeEditorControl _codeEditor;
    [KSPEvent(active = true, guiActive = true, guiName = "Open Code Editor", guiActiveEditor = true)]
    public void OpenEditor()
    {
        AeroInterfaceManager.AddControl(_codeEditor);
    }

    private LogControl _logControl;
    [KSPEvent(active = true, guiActive = true, guiName = "Open Log", guiActiveEditor = true)]
    public void OpenLog()
    {
        AeroInterfaceManager.AddControl(_logControl);
    }

    #endregion

    #region Execution buttons

    [UI_Toggle(scene = UI_Scene.All, controlEnabled = true, enabledText = "Enabled", disabledText = "Disabled")]
    [KSPField(guiActiveEditor = true, guiActive = true, guiName = "Execution:", isPersistant = true)]
    public bool shouldRun = false;
    
    [KSPAction("Toggle Execution")]
    public void Toggle(KSPActionParam param)
    {
        shouldRun = !shouldRun;
    }

    #endregion

    #region Exception catching

    public void ThrowException(string message, StatusKind kind = StatusKind.Uhoh, bool displayPopup = true)
    {
        if (FlightGlobals.ActiveVessel == null || FlightGlobals.ActiveVessel.id == vessel.id)
        {
            ExceptionBoxControl.Show(message);
        }
        SetStatus(message, kind);
    }

    protected void CatchException(Exception e)
    {
        switch (e)
        {
            case AggregateException ae:
            {
                if (ae.InnerExceptions.Count == 1 && ae.InnerException != null)
                {
                    CatchException(ae.InnerException);
                }
                else
                {
                    foreach (Exception ie in ae.InnerExceptions)
                    {
                        CatchException(ie);
                    }
                }
            } break;
            case OperationCanceledException:
            {
                ThrowException("Our engineers typically suggest writing scripts which don't loop forever, so they have shut down the script to prevent such a time paradox.");
            } break;
            default:
            {
                ThrowException(e.Message);
            } break;
        }
        
        Debug.Log($"[{DateTime.Now.Hour}:{DateTime.Now.Minute}:{DateTime.Now.Second}] Exception caught during execution! Data: {e.Message}\n{e.StackTrace}");
    }

    #endregion

    #region Status
    
    public void Log(string log, StatusKind kind = StatusKind.Exceptional)
    {
        string time;
        if (HighLogic.LoadedSceneIsFlight)
        {
            TimeSpan timeSpan = TimeSpan.FromSeconds(vessel.missionTime);
            time = $"[{timeSpan.Hours}:{timeSpan.Minutes}:{timeSpan.Seconds}]";
        }
        else
        {
            time = $"[{KSPUtil.dateTimeFormatter.PrintDateCompact(HighLogic.CurrentGame.UniversalTime, true, true)}]";
        }

        switch (kind)
        {
            default:
            case StatusKind.Exceptional:
            {
                _logControl.Log($"{time} {log}");
            } break;
            case StatusKind.NotGreat:
            {
                _logControl.Log($"<color=orange>{time}</color> {log}");
            } break;
            case StatusKind.Uhoh:
            {
                _logControl.Log($"<color=red>{time}</color> {log}");
            } break;
        }
    }

    public void SetStatus(string newStatus, StatusKind kind)
    {
        switch (kind)
        {
            case StatusKind.NotGreat:
            {
                status = $"We have a problem:\n{newStatus}";
                UpdateException(kind);
            } break;
            case StatusKind.Uhoh:
            {
                status = "Uh oh, something is breaking!\nSee below for info:";
                exception = newStatus;
                UpdateException(kind);
            } break;
            default:
            {
                ResetStatus();
                UpdateException(StatusKind.Exceptional);
            } break;
        }

        if (!string.IsNullOrEmpty(newStatus))
        {
            Log($"Status update: {newStatus}", kind);
        }
    }

    private void UpdateException(StatusKind kind)
    {
        if (kind == StatusKind.Uhoh)
        {
            shouldRun = false;
            tokenContainer.shouldCompile = false;
            
            Fields["exception"].guiActive = true;
            Fields["exception"].guiActiveEditor = true;
            Fields["exception"].guiActiveUnfocused = true;
        }
        else
        {
            Fields["exception"].guiActive = false;
            Fields["exception"].guiActiveEditor = false;
            Fields["exception"].guiActiveUnfocused = false;
        }
    }

    public void ResetStatus()
    {
        status = "Not actively breaking";
        UpdateException(StatusKind.Exceptional);
    }

    #endregion
}

/// <summary>
/// TODO: This is an extremely hacked together way of keeping certain characters without bricking saves
/// we also store other things here 'cuz why not
/// </summary>
[Serializable]
public class TokenContainer : IConfigNode
{
    public ScriptCraft craft = new ScriptCraft("untitled script", "", KerbinSuperComputer.Library.GetCurrentScriptsPath());

    public string Tokens => craft.Script;

    [SerializeField]
    public bool shouldCompile;

    #region Loading

    public void Load(ConfigNode node)
    {
        if (node.HasNode("script-craft"))
        {
            CraftLoad(node.GetNode("script-craft"));
        }

        if (node.HasValue("compile-startup"))
        {
            shouldCompile = true;
        }
    }

    private void CraftLoad(ConfigNode node)
    {
        string tokens = "";
        string directory = node.GetValue("directory");
        string name = node.GetValue("craft-name");
        
        if (int.TryParse(node.GetValue("script-length"), out int length))
        {
            tokens = TokensLoad(node.GetNode("script-lines"), length);
        }

        craft = new ScriptCraft(name, tokens, directory);
    }

    private string TokensLoad(ConfigNode node, int length)
    {
        string tokens = "";
        for (int i = 0; i < length; i++)
        {
            if (node.HasValue($"script-line{i}"))
            {
                tokens += $"{node.GetValue($"script-line{i}").ConfigDirty()}\n";
            }
        }

        tokens = tokens.TrimEnd();
        return tokens;
    }

    #endregion

    public void Save(ConfigNode node)
    {
        ConfigNode craftNode = new ConfigNode("script-craft");
        SaveCraft(craftNode);
        node.AddNode(craftNode);

        if (shouldCompile)
        {
            node.AddValue("compile-startup", "");
        }
    }

    public void SaveCraft(ConfigNode node)
    {
        string[] lines = craft.Script.TrimEnd().Split('\n');
        node.AddValue("script-length", lines.Length);

        ConfigNode linesNode = new ConfigNode("script-lines");
        for (int i = 0; i < lines.Length; i++)
        {
            linesNode.AddValue($"script-line{i}", lines[i].ConfigClean());
        }

        node.AddNode(linesNode);
        
        node.AddValue("directory", craft.Directory);
        node.AddValue("craft-name", craft.Name);
    }
}