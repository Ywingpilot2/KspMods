﻿using System;
using ActionLanguage;
using ActionLanguage.Exceptions;
using ActionLanguage.Extensions;
using ActionLanguage.Library;
using ProgrammableMod.Scripting.Library;
using UniLinq;
using UnityEngine;
using UnityEngine.Serialization;

namespace ProgrammableMod.Modules.Computers;

public enum StatusKind
{
    Exceptional,
    NotGreat = 1,
    Uhoh = 2
}

public abstract class BaseComputer : PartModule, IComputer
{
    protected ILibrary[] Libraries;
    
    [KSPField(isPersistant = false, guiActive = true, guiName = "Program Status", guiActiveEditor = true)]
    public string status = "Operating";
    
    [KSPField(isPersistant = false, guiActive = false, guiName = "Error")]
    public string exception;

    [KSPField(isPersistant = true)]
    public TokenContainer tokenContainer;

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
            }
            tokenContainer.shouldRun = value;
        }
    }
    
    protected ActionScript Script;

    #region Logic
    
    private void Execution(FlightCtrlState state)
    {
        if (tokenContainer.shouldRun && HighLogic.LoadedSceneIsFlight)
        {
            State = state;

            try
            {
                Script.Execute();
            }
            catch (Exception e)
            {
                exception = e.Message;
                DisplayPopup = true;
                ShouldRun = false;
            }

            PostExecute();
        }
    }

    protected virtual void PostExecute()
    {
    }

    public override void OnAwake()
    {
        base.OnAwake();
        tokenContainer = new TokenContainer();
    }

    public override void OnStart(StartState state)
    {
        Libraries = new ILibrary[]
        {
            new KerbalLibrary(this),
            new VesselLibrary(this),
            new ComputerLibrary(ActionCompiler.Library, this)
        };

        _compiler = new ActionCompiler(Libraries);
        ResetStatus();
        if (tokenContainer.shouldCompile)
        {
            CompileScript();
        }
        
        if (HighLogic.LoadedSceneIsFlight)
        {
            vessel.OnFlyByWire += Execution;
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
        if (DisplayEditor && (HighLogic.LoadedSceneIsEditor || HighLogic.LoadedSceneIsFlight))
        {
            _codeWindowRect = GUILayout.Window(new System.Random(23123452).Next(), _codeWindowRect, DrawCodeEditor, "Edit Script");
        }
        
        if (DisplayPopup && (HighLogic.LoadedSceneIsFlight || HighLogic.LoadedSceneIsEditor))
        {
            _popUpRect = GUILayout.Window(new System.Random(482123452).Next(), _popUpRect, DrawPopUp, "Kerbaton Alert!");
        }

        if (DisplayLog && (HighLogic.LoadedSceneIsEditor || HighLogic.LoadedSceneIsFlight))
        {
            _logWindowRect = GUILayout.Window(new System.Random(752236422).Next(), _logWindowRect, DrawLog, "Computer Log");
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
        //GUILayout.BeginArea(new Rect(200, (250 - 20) / 2, 400, 250 - 20)); // TODO: causing weird issues
        
        GUILayout.Label(exception);

        GUILayout.BeginHorizontal();

        if (GUILayout.Button("Break Execution"))
        {
            DisplayPopup = false;
            ShouldRun = false;
            tokenContainer.shouldCompile = false;
            UpdateButton();
        }
        
        if (GUILayout.Button("Continue Execution"))
        {
            DisplayPopup = false;
            ShouldRun = true;
        }
        
        GUILayout.EndHorizontal();
        
        GUILayout.EndVertical();
    }

    #endregion

    #region Events

    protected virtual void OnCompiled()
    {
    }

    private string previous;
    public virtual void Log(string log)
    {
        if (previous == log)
            return;

        previous = log;
        TimeSpan time = TimeSpan.FromSeconds(vessel.missionTime);
        logText += $"[{time.Hours}:{time.Minutes}:{time.Seconds}] {log}\n";
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
                ShouldRun = false;
                tokenContainer.shouldCompile = false;
                UpdateButton();
                SetStatus(reason, StatusKind.Uhoh);
            }

            Script = script;
            tokenContainer.shouldCompile = true;
            ResetStatus();
        }
        catch (Exception e)
        {
            ShouldRun = false;
            tokenContainer.shouldCompile = false;
            UpdateButton();
            SetStatus(e.Message, StatusKind.Uhoh);
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
    
    [KSPAction("Toggle Script")]
    public void Toggle()
    {
        if (tokenContainer.shouldRun)
            StopExecuting();
        else
            Execute();
    }

    [KSPEvent(active = true, guiActive = true, guiName = "Start execution")]
    [KSPAction("Execute Script")]
    public void Execute()
    {
        ShouldRun = true;
        UpdateButton();
    }

    [KSPEvent(active = false, guiActive = true, guiName = "Stop execution")]
    [KSPAction("Stop Script")]
    public void StopExecuting()
    {
        ShouldRun = false;
        UpdateButton();
    }

    #endregion

    #region Editor

    [KSPField(guiActiveEditor = true, guiName = "Execution")]
    public string execEditorStatus = "Start inactive";

    [KSPEvent(guiActiveEditor = true, guiName = "Toggle startup")]
    public void ToggleStart()
    {
        ShouldRun = !ShouldRun;
        UpdateButton();
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
                execEditorStatus = "Start active";
            }
            else
            {
                execEditorStatus = "Start inactive";
            }
        }
    }

    #endregion

    #region Status

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
                    tokens += $"{node.GetValue($"script-line{i}").SanitizedReplace("|[|", "{").SanitizedReplace("|]|","}")}\n";
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
            node.AddValue($"script-line{i}", lines[i].Trim().SanitizedReplace("{", "|{|").SanitizedReplace("}", "|}|"));
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
}