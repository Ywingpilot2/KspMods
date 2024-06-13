using System;
using ActionLanguage;
using ActionLanguage.Library;
using ProgrammableMod.Scripting.Library;
using UnityEngine;

namespace ProgrammableMod.Modules.Computers;

public enum StatusKind
{
    Exceptional,
    NotGreat = 1,
    Uhoh = 2
}

public enum ErrorKind
{
    Compiler = 0,
    Runtime = 1
}

public abstract class BaseComputer : PartModule, IComputer
{
    protected ILibrary[] Libraries;
    
    [KSPField(isPersistant = false, guiActive = true, guiName = "Program Status")]
    public string Status = "Operating effectively";
    
    [KSPField(isPersistant = false, guiActive = false, guiName = "Error")]
    public string Exception;

    protected string Tokens;
    protected ActionScript Script;
    protected bool ShouldRun;

    private void Execution(FlightCtrlState state)
    {
        if (Script != null && HighLogic.LoadedSceneIsFlight)
        {
            Script.Execute();
        }
    }

    private void Start()
    {
        Libraries = new ILibrary[]
        {
            new KerbalLibrary()
        };

        vessel.OnFlyByWire += Execution;
    }

    #region GUI Handling

    // TODO: We should really move this all into a framework of some kind...
    protected bool DisplayEditor;
    protected bool DisplayPopup;
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
    }

    private Rect _codeWindowRect = new(Screen.width / 2, Screen.height / 2, 600, 450);
    private Vector2 _scrollViewVector = Vector2.zero;
    private string _editorText;
    private void DrawCodeEditor(int winId)
    {
        GUI.DragWindow(new Rect(0,0, 600, 20));
        GUILayout.BeginVertical();
        
        _scrollViewVector = GUILayout.BeginScrollView(_scrollViewVector);

        _editorText = GUILayout.TextArea(_editorText, GUILayout.ExpandWidth(true), GUILayout.ExpandHeight(true));

        GUILayout.EndScrollView();

        GUILayout.BeginHorizontal();

        if (GUILayout.Button("Save")) // TODO: cancel button
        {
            Tokens = _editorText;
            DisplayEditor = false;
        }
        
        GUILayout.EndHorizontal();

        GUILayout.EndVertical();
    }
    
    private Rect _popUpRect = new(Screen.width / 2, Screen.height / 2, 400, 250);
    private void DrawPopUp(int winId)
    {
        GUI.DragWindow(new Rect(0,0, 400, 20));
        GUILayout.BeginVertical();
        
        // TODO: verify this math even works, should center us onto the window
        GUILayout.BeginArea(new Rect(200, (250 - 20) / 2, 400, 250 - 20));
        
        GUILayout.BeginVertical();
        GUILayout.Label("An error has occured! Message:"); // TODO: error kind
        GUILayout.Label(Exception);
        GUILayout.EndVertical();
        
        GUILayout.EndArea();

        if (GUILayout.Button("Ok"))
        {
            DisplayPopup = false;
        }
        
        GUILayout.EndVertical();
    }

    #endregion

    #region UI Buttons

    [KSPEvent(active = true, guiActive = true, guiName = "Open Code Editor")]
    public void OpenEditor()
    {
        _editorText = Tokens;
        DisplayEditor = true;
    }

    #endregion

    #region Compilation

    [KSPEvent(active = true, guiActive = true, guiName = "Compile Script")]
    public void CompileScript()
    {
        if (Tokens != null)
        {
            try
            {
                ActionCompiler compiler = new ActionCompiler(Tokens, Libraries);
                Script = compiler.CompileScript();
                ResetStatus();
            }
            catch (Exception e)
            {
                ShouldRun = false;
                UpdateButton();
                SetStatus(e.Message, StatusKind.Uhoh);
            }
        }
        else
        {
            DisplayPopup = true;
        }
    }

    public abstract bool ValidateScript();

    #endregion

    #region Execution buttons

    [KSPEvent(active = true, guiActive = true, guiName = "Start execution")]
    public void Execute()
    {
        ShouldRun = true;
        UpdateButton();
    }

    [KSPEvent(active = false, guiActive = true, guiName = "Stop execution")]
    public void StopExecuting()
    {
        ShouldRun = false;
        UpdateButton();
    }

    public void Toggle()
    {
        if (ShouldRun)
            StopExecuting();
        else
            Execute();
    }

    protected void UpdateButton()
    {
        if (ShouldRun)
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

    #endregion

    #region Status

    protected void SetStatus(string status, StatusKind kind)
    {
        switch (kind)
        {
            case StatusKind.NotGreat:
            {
                Status = $"Warning: {status}";
                UpdateException(kind);
            } break;
            case StatusKind.Uhoh:
            {
                Status = "An exception has occured!\nSee below for info:";
                Exception = status;
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
            Fields["Exception"].guiActive = true;
        }
        else
        {
            Fields["Exception"].guiActive = false;
        }
    }

    protected void ResetStatus()
    {
        Status = "Operating properly";
        UpdateException(StatusKind.Exceptional);
    }

    #endregion
}