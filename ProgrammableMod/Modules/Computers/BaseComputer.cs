using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using AeroDynamicKerbalInterfaces;
using JetBrains.Annotations;
using ProgrammableMod.Controls;
using ProgrammableMod.Scripting.Exceptions;
using ProgrammableMod.Scripting.Library;
using SteelLanguage;
using SteelLanguage.Extensions;
using SteelLanguage.Library;
using UniLinq;
using UnityEngine;
using UnityEngine.Serialization;
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
    protected ILibrary[] Libraries;
    
    [KSPField(isPersistant = false, guiActive = true, guiName = "Program Status", guiActiveEditor = true)] [UsedImplicitly]
    public string status = "Operating";
    
    [KSPField(isPersistant = false, guiActive = false, guiName = "Error")]
    public string exception;

    [KSPField(isPersistant = true)]
    public TokenContainer tokenContainer;

    [KSPField(isPersistant = true)]
    public float runTime;

    internal FlightCtrlState State;
    private SteelCompiler _compiler;

    public bool ShouldRun
    {
        get => tokenContainer.shouldRun;
        set
        {
            if (value)
            {
                runTime = Time.fixedTime;
                ResetStatus();
            }
            else
            {
                running = false;
            }
            tokenContainer.shouldRun = value;
            UpdateButton();
        }
    }

    protected SteelScript Script;

    #region Execution
    
    private void Execution(FlightCtrlState state)
    {
        OnExecute();

        if (tokenContainer.shouldRun && HighLogic.LoadedSceneIsFlight)
        {
            State = state;

            try
            {
                PreExecute();

                CancellationTokenSource cancel = new CancellationTokenSource();
                cancel.CancelAfter(80);

                Parallel.Invoke(new ParallelOptions { CancellationToken = cancel.Token, MaxDegreeOfParallelism = 2 },
                    Script.Execute);
                cancel.Token.ThrowIfCancellationRequested();

                PostExecute();
            }
            catch (OperationCanceledException)
            {
                ThrowException(
                    "Our engineers typically suggest writing scripts which don't loop forever, so they have shut down the script to prevent such a time paradox.");
            }
            catch (AggregateException e)
            {
                if (e.InnerExceptions.Count == 1 && e.InnerException != null)
                {
                    ThrowException(e.InnerException.Message);
                }
                else
                {
                    string error = "Big problem, multiple errors have occured! The errors:\n";
                    
                    foreach (Exception innerException in e.InnerExceptions)
                    {
                        error += innerException.Message;
                    }
                    
                    ThrowException(error);
                }
            }
            catch (Exception e)
            {
                ThrowException(e.Message);
            }
        }
    }

    private string _previous;
    private int _count;
    public virtual void Log(string log)
    {
        if (_previous == log)
            return;

        _count++;
        _previous = log;
        TimeSpan time = TimeSpan.FromSeconds(vessel.missionTime);
        _logControl.Log($"[{time.Hours}:{time.Minutes}:{time.Seconds}] {log}");
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
            new KerbalLibrary(this, SteelCompiler.Library),
            new VesselLibrary(this),
            new ComputerLibrary(SteelCompiler.Library, this)
        };

        _compiler = new SteelCompiler(Libraries);
        _logControl = new LogControl(new Random(GetHashCode()).Next(), _ => _logOpen = false);
        
        ResetStatus();
        if (tokenContainer.shouldCompile || ShouldRun)
        {
            CompileScript();
        }

        if (HighLogic.LoadedSceneIsFlight)
        {
            AssignGameEvents();
        }
        
        UpdateButton();
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

    protected virtual void OnCompiled()
    {
        compiling = false;
    }

    #endregion

    #region Game Events

    private void AssignGameEvents()
    {
        vessel.OnFlyByWire += Execution;
        GameEvents.onPartDestroyed.Add(OnBlownUp);
    }

    private void RemoveGameEvents()
    {
        vessel.OnFlyByWire -= Execution;
        GameEvents.onPartDestroyed.Remove(OnBlownUp);
    }

    private void OnBlownUp(Part data)
    {
        // not our crash, or bob is trying to break in again...
        if (data.vessel.id != part.vessel.id || data.isVesselEVA)
            return;
        
        Random rng = new();
        if (rng.Next(0, 10) <= 5 || data.flightID == part.flightID)
        {
            ThrowException($"Oh no, an unknown error has occured! Any unsaved progress, in progress actions, or other important functions will be inoperable until computer is turned back on.\nError Code: {rng.Next(404)}");
        }

        if (data.flightID == part.flightID)
        {
            RemoveGameEvents();
        }
    }

    #endregion
    
    #region Compilation

    [KSPEvent(active = true, guiActive = true, guiName = "Compile Script", guiActiveEditor = true, guiActiveUnfocused = true, unfocusedRange = 15f)]
    public void CompileScript()
    {
        compiling = true;
        try
        {
            SteelScript script = _compiler.Compile(tokenContainer.tokens);
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
            ThrowException(e.Message);
        }
        finally
        {
            OnCompiled();
        }
    }

    public abstract bool ValidateScript(SteelScript script, out string reason);

    #endregion

    #region UI Buttons

    private bool _editorOpen;
    [KSPEvent(active = true, guiActive = true, guiName = "Open Code Editor", guiActiveEditor = true)]
    public void OpenEditor()
    {
        if (_editorOpen)
            return;
        
        _editorOpen = true;
        CodeEditorControl.Show(tokenContainer.tokens, "Code Editor",
            control => tokenContainer.tokens = control.Text, _ => _editorOpen = false);
    }

    private LogControl _logControl;
    private bool _logOpen;
    [KSPEvent(active = true, guiActive = true, guiName = "Open Log", guiActiveEditor = true)]
    public void OpenLog()
    {
        AeroInterfaceManager.AddControl(_logControl);
    }

    #endregion

    #region Execution buttons

    #region Flight
    
    [KSPAction("Toggle Computer")]
    public void Toggle()
    {
        if (tokenContainer.shouldRun)
            StopExecuting();
        else
            Execute();
    }

    [KSPEvent(active = true, guiActive = true, guiName = "Start execution")]
    [KSPAction("Turn On")]
    public void Execute()
    {
        ShouldRun = true;
    }

    [KSPEvent(active = false, guiActive = true, guiName = "Stop execution")]
    [KSPAction("Turn Off")]
    public void StopExecuting()
    {
        ShouldRun = false;
    }

    #endregion

    #region Editor

    [KSPEvent(guiActiveEditor = true, guiName = "Toggle startup")]
    public void ToggleStart()
    {
        ShouldRun = !ShouldRun;
    }

    #endregion

    protected void UpdateButton()
    {
        if (HighLogic.LoadedSceneIsFlight)
        {
            if (tokenContainer.shouldRun)
            {
                Events["StopExecuting"].active = true;
                Events["Execute"].active = false;
            }
            else
            {
                Events["StopExecuting"].active = false;
                Events["Execute"].active = true;
            }
        }
        else
        {
            if (tokenContainer.shouldRun)
            {
                Events["ToggleStart"].guiName = "Start On";
                Events["ToggleStart"].assigned = true;
            }
            else
            {
                Events["ToggleStart"].guiName = "Start Off";
                Events["ToggleStart"].assigned = false;
            }
        }
    }

    #endregion

    #region Status

    public void ThrowException(string message, StatusKind kind = StatusKind.Uhoh, bool displayPopup = true)
    {
        ExceptionBoxControl.Show(message);
        if (kind == StatusKind.Uhoh)
        {
            ShouldRun = false;
            tokenContainer.shouldCompile = false;
        }
        SetStatus(message, kind);
    }

    public void SetStatus(string newStatus, StatusKind kind)
    {
        switch (kind)
        {
            case StatusKind.NotGreat:
            {
                status = $"Somethings off:\n{newStatus}";
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
            Log($"Status update: {newStatus}");
        }
    }

    private void UpdateException(StatusKind kind)
    {
        if (kind == StatusKind.Uhoh)
        {
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
    [SerializeField]
    public string tokens = "";

    [SerializeField]
    public bool shouldRun;
    
    [SerializeField]
    public bool shouldCompile;
    
    public void Load(ConfigNode node)
    {
        if (node.HasValue("script-length") && int.TryParse(node.GetValue("script-length"), out int length))
        {
            for (int i = 0; i < length; i++)
            {
                if (node.HasValue($"script-line{i}"))
                {
                    tokens += $"{Dirty(node.GetValue($"script-line{i}"))}\n";
                }
            }
        }

        if (node.HasValue("compile-startup"))
        {
            shouldCompile = true;
        }

        if (node.HasValue("script-startup"))
        {
            shouldRun = true;
        }
    }

    public void Save(ConfigNode node)
    {
        string[] lines = tokens.Split('\n');
        node.AddValue("script-length", lines.Length);
        for (int i = 0; i < lines.Length; i++)
        {
            node.AddValue($"script-line{i}", Clean(lines[i]));
        }

        if (shouldCompile)
        {
            node.AddValue("compile-startup", "");
        }
        
        if (shouldRun)
        {
            node.AddValue("script-startup", "");
        }
    }

    private string Clean(string dirty) => dirty.Trim().SanitizedReplace("{", "|{|").SanitizedReplace("}", "|}|").SanitizedReplace("\t", "|t|");

    private string Dirty(string clean) => clean.SanitizedReplace("|[|", "{").SanitizedReplace("|]|", "}").SanitizedReplace("|t|", "\t");
}