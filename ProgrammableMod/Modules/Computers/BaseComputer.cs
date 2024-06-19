using System;
using ActionLanguage;
using ActionLanguage.Library;
using ProgrammableMod.Scripting.Library;
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
    
    [KSPField(isPersistant = false, guiActive = true, guiName = "Program Status")]
    public string status = "Operating effectively";
    
    [KSPField(isPersistant = false, guiActive = false, guiName = "Error")]
    public string exception;

    [KSPField(isPersistant = true)]
    public TokenContainer tokenContainer;

    internal FlightCtrlState State;
    private ActionCompiler _compiler;
    
    protected ActionScript Script;

    #region Logic

    private void Execution(FlightCtrlState state)
    {
        if (tokenContainer.shouldRun && Script != null && HighLogic.LoadedSceneIsFlight)
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
            }
        }
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
            new KerbalLibrary(),
            new VesselLibrary(this),
            new ComputerLibrary(ActionCompiler.Library)
        };

        _compiler = new ActionCompiler(Libraries);
        if (tokenContainer.shouldCompile)
        {
            CompileScript();
        }
        
        if (HighLogic.LoadedSceneIsFlight)
        {
            vessel.OnFlyByWire += Execution;
        }
    }

    #endregion

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
            tokenContainer.tokens = _editorText;
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
        
        GUILayout.Label("An error has occured! Message:");
        //GUILayout.BeginArea(new Rect(200, (250 - 20) / 2, 400, 250 - 20)); // TODO: causing weird issues
        
        GUILayout.Label(exception);

        if (GUILayout.Button("Ok"))
        {
            DisplayPopup = false;
        }
        
        GUILayout.EndVertical();
    }

    #endregion

    #region UI Buttons

    [KSPEvent(active = true, guiActive = true, guiName = "Open Code Editor", guiActiveEditor = true)]
    public void OpenEditor()
    {
        _editorText = tokenContainer.tokens;
        DisplayEditor = true;
    }

    #endregion

    #region Compilation

    [KSPEvent(active = true, guiActive = true, guiName = "Compile Script")]
    public void CompileScript()
    {
        try
        {
            Script = _compiler.Compile(tokenContainer.tokens);
            tokenContainer.shouldCompile = true;
            ResetStatus();
        }
        catch (Exception e)
        {
            tokenContainer.shouldRun = false;
            tokenContainer.shouldCompile = false;
            UpdateButton();
            SetStatus(e.Message, StatusKind.Uhoh);
        }
    }

    public abstract bool ValidateScript(ActionScript script);

    #endregion

    #region Execution buttons

    [KSPEvent(active = true, guiActive = true, guiName = "Start execution")]
    [KSPAction("Execute Script")]
    public void Execute()
    {
        tokenContainer.shouldRun = true;
        UpdateButton();
    }

    [KSPEvent(active = false, guiActive = true, guiName = "Stop execution")]
    [KSPAction("Stop Script")]
    public void StopExecuting()
    {
        tokenContainer.shouldRun = false;
        UpdateButton();
    }

    [KSPAction("Toggle Script")]
    public void Toggle()
    {
        if (tokenContainer.shouldRun)
            StopExecuting();
        else
            Execute();
    }

    protected void UpdateButton()
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

    #endregion

    #region Status

    protected void SetStatus(string status, StatusKind kind)
    {
        switch (kind)
        {
            case StatusKind.NotGreat:
            {
                this.status = $"Warning: {status}";
                UpdateException(kind);
            } break;
            case StatusKind.Uhoh:
            {
                this.status = "An exception has occured!\nSee below for info:";
                exception = status;
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
        }
        else
        {
            Fields["exception"].guiActive = false;
        }
    }

    protected void ResetStatus()
    {
        status = "Operating properly";
        UpdateException(StatusKind.Exceptional);
    }

    #endregion
}

/// <summary>
/// TODO: This is an extremely hacked together way of keeping new lines on scripts
/// we also store other things here 'cuz why not
/// </summary>
[Serializable]
public class TokenContainer : IConfigNode
{
    [SerializeField]
    public string tokens = "";

    [SerializeField]
    public bool shouldRun = false;
    
    [SerializeField]
    public bool shouldCompile = false;
    
    public void Load(ConfigNode node)
    {
        if (node.HasValue("script-length") && int.TryParse(node.GetValue("script-length"), out int length))
        {
            for (int i = 0; i < length; i++)
            {
                if (node.HasValue($"script-line{i}"))
                {
                    tokens += $"{node.GetValue($"script-line{i}")}\n\r";
                }
            }
        }
    }

    public void Save(ConfigNode node)
    {
        string[] lines = tokens.Split('\n');
        node.AddValue("script-length", lines.Length);
        for (int i = 0; i < lines.Length; i++)
        {
            node.AddValue($"script-line{i}", lines[i].Trim());
        }
    }
}