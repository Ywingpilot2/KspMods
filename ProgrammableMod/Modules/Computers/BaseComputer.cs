using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using ActionLanguage;
using ActionLanguage.Exceptions;
using ActionLanguage.Extensions;
using ActionLanguage.Library;
using JetBrains.Annotations;
using ProgrammableMod.Scripting.Exceptions;
using ProgrammableMod.Scripting.Library;
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
    private ActionCompiler _compiler;

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
            tokenContainer.shouldRun = value;
            UpdateButton();
        }
    }
    
    protected ActionScript Script;

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

        if (_count == 250)
        {
            logText = "";
            _count = 0;
        }

        _count++;
        _previous = log;
        TimeSpan time = TimeSpan.FromSeconds(vessel.missionTime);
        logText += $"[{time.Hours}:{time.Minutes}:{time.Seconds}] {log}\n";
    }

    #endregion

    #region Logic

    public override void OnAwake()
    {
        base.OnAwake();
        tokenContainer = new TokenContainer();
    }

    public override void OnStart(StartState state)
    {
        Libraries = new ILibrary[]
        {
            new KerbalLibrary(this, ActionCompiler.Library),
            new VesselLibrary(this),
            new ComputerLibrary(ActionCompiler.Library, this)
        };

        _compiler = new ActionCompiler(Libraries);
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

    #region GUI Handling

    // TODO: We should really move this all into a framework of some kind...
    protected bool DisplayEditor;
    protected bool DisplayPopup;
    protected bool DisplayLog;
    private void OnGUI()
    {
        if (!HighLogic.LoadedSceneIsGame)
            return;
        
        if (DisplayEditor && (HighLogic.LoadedSceneIsEditor || HighLogic.LoadedSceneIsFlight))
        {
            _codeWindowRect = GUILayout.Window(new Random(23123452).Next(), _codeWindowRect, DrawCodeEditor, "Edit Script");
        }
        
        if (DisplayPopup && (HighLogic.LoadedSceneIsFlight || HighLogic.LoadedSceneIsEditor))
        {
            _popUpRect = GUILayout.Window(new Random(482123452).Next(), _popUpRect, DrawPopUp, "Kerbaton Alert!");
        }

        if (DisplayLog && (HighLogic.LoadedSceneIsEditor || HighLogic.LoadedSceneIsFlight))
        {
            _logWindowRect = GUILayout.Window(new Random(752236422).Next(), _logWindowRect, DrawLog, "Computer Log");
        }
    }

    private Rect _codeWindowRect = new(Screen.width / 2, Screen.height / 2, 600, 450);
    private Vector2 _scrollCodeVector = Vector2.zero;
    private string _editorText;
    private void DrawCodeEditor(int winId)
    {
        GUI.DragWindow(new Rect(0,0, 600, 20));
        GUILayout.BeginVertical();
        
        _scrollCodeVector = GUILayout.BeginScrollView(_scrollCodeVector);

        _editorText = GUILayout.TextArea(_editorText, GUILayout.ExpandWidth(true), GUILayout.ExpandHeight(true));

        GUILayout.EndScrollView();

        GUILayout.BeginHorizontal();

        if (GUILayout.Button("Cancel"))
        {
            DisplayEditor = false;
        }

        if (GUILayout.Button("Save"))
        {
            tokenContainer.tokens = _editorText;
            DisplayEditor = false;
        }
        
        GUILayout.EndHorizontal();

        GUILayout.EndVertical();
    }

    private Rect _logWindowRect = new(Screen.width / 2, Screen.height / 2, 600, 450);
    private Vector2 _scrollLogVector = Vector2.zero;
    
    public string logText;
    private void DrawLog(int winid)
    {
        GUI.DragWindow(new Rect(0,0, 600, 20));
        GUILayout.BeginVertical();
        
        _scrollLogVector = GUILayout.BeginScrollView(_scrollLogVector);

        GUILayout.TextArea(logText, GUILayout.ExpandWidth(true), GUILayout.ExpandHeight(true));

        GUILayout.EndScrollView();

        GUILayout.BeginHorizontal();

        if (GUILayout.Button("Clear"))
        {
            logText = "";
        }

        if (GUILayout.Button("Close"))
        {
            DisplayLog = false;
        }
        
        GUILayout.EndHorizontal();

        GUILayout.EndVertical();
    }
    
    private Rect _popUpRect = new(Screen.width / 2, Screen.height / 2, 400, 250);
    private void DrawPopUp(int winId)
    {
        GUI.DragWindow(new Rect(0,0, 400, 20));
        GUILayout.BeginVertical();
        
        GUILayout.Label("An error has occured! Message:");

        GUILayout.Label(exception);

        GUILayout.BeginHorizontal();

        if (executionException)
        {
            if (GUILayout.Button("Break Execution"))
            {
                DisplayPopup = false;
                ShouldRun = false;
                tokenContainer.shouldCompile = false;
            }
        
            if (GUILayout.Button("Continue Execution"))
            {
                DisplayPopup = false;
                ShouldRun = true;
            }
        }
        else
        {
            if (GUILayout.Button("OK"))
            {
                DisplayPopup = false;
                ShouldRun = false;
                tokenContainer.shouldCompile = false;
            }
        }

        GUILayout.EndHorizontal();
        
        GUILayout.EndVertical();
    }

    #endregion

    #region Script Events

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
    }

    protected virtual void OnCompiled()
    {
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
        // not our crash
        if (data.missionID != part.missionID || data.isVesselEVA)
            return;
        
        Random rng = new Random();
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
        try
        {
            ActionScript script = _compiler.Compile(tokenContainer.tokens);
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

    public abstract bool ValidateScript(ActionScript script, out string reason);

    #endregion

    #region UI Buttons

    [KSPEvent(active = true, guiActive = true, guiName = "Open Code Editor", guiActiveEditor = true)]
    public void OpenEditor()
    {
        if (!DisplayEditor)
        {
            _editorText = tokenContainer.tokens;
        }
        DisplayEditor = true;
    }
    
    [KSPEvent(active = true, guiActive = true, guiName = "Open Log", guiActiveEditor = true)]
    public void OpenLog()
    {
        DisplayLog = true;
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

    private bool executionException = false;
    public void ThrowException(string message, StatusKind kind = StatusKind.Uhoh, bool displayPopup = true)
    {
        DisplayPopup = displayPopup;
        if (kind == StatusKind.Uhoh)
        {
            executionException = ShouldRun;
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