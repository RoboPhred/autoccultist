using System;
using System.IO;
using System.Linq;
using AutoccultistNS;
using AutoccultistNS.Brain;
using AutoccultistNS.Config;
using AutoccultistNS.GameState;
using AutoccultistNS.GUI;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

/// <summary>
/// The main entrypoint for Autoccultist.
/// </summary>
public class Autoccultist : MonoBehaviour
{
    /// <summary>
    /// Gets the instance of the plugin.
    /// </summary>
    public static Autoccultist Instance
    {
        get;
        private set;
    }

    /// <summary>
    /// Gets the directory the mod dll is located in.
    /// </summary>
    public static string AssemblyDirectory
    {
        get
        {
            var assemblyLocation = typeof(Autoccultist).Assembly.Location;
            return Path.GetDirectoryName(assemblyLocation);
        }
    }

    public static void Initialise()
    {
        new GameObject().AddComponent<Autoccultist>();
    }

    /// <summary>
    /// Starts the mod.
    /// </summary>
    public void Start()
    {
        Instance = this;

        SceneManager.sceneLoaded += new UnityAction<Scene, LoadSceneMode>(this.HandleSceneLoaded);
        SceneManager.sceneUnloaded += new UnityAction<Scene>(this.HandleSceneUnloaded);

        GameAPI.Initialize();

        this.ReloadAll();
        if (Library.ParseErrors.Count > 0)
        {
            ParseErrorsGUI.IsShowing = true;
        }

        GameEventSource.GameStarted += (_, __) => Superego.AutoselectArc();

        GameEventSource.GameEnded += (_, __) =>
        {
            this.StopAutoccultist();
            Superego.SetArc(null);
        };

        this.LogInfo("Autoccultist initialized.");
    }

    /// <summary>
    /// Reload all tasks in the TaskDriver.
    /// </summary>
    public void ReloadAll()
    {
        var previousArcName = Superego.CurrentArc?.Name;

        this.StopAutoccultist();
        this.LogInfo("Reloading all configs");
        Library.LoadAll();
        Superego.Clear();

        if (GameAPI.IsRunning)
        {
            var arc = Library.Arcs.FirstOrDefault(a => a.Name == previousArcName);
            if (arc != null)
            {
                Superego.SetArc(arc);
            }
        }
    }

    /// <summary>
    /// Renders the mod GUI.
    /// </summary>
    public void OnGUI()
    {
        WindowManager.OnPreGUI();

        if (!GameAPI.IsRunning)
        {
            // Allow ParseErrorsGUI to run when the core game is not in play.
            ParseErrorsGUI.OnGUI();
            return;
        }

        ControlGUI.OnGUI();
        ParseErrorsGUI.OnGUI();
        DiagnosticsGUI.OnGUI();
        GoalsGUI.OnGUI();
        ArcsGUI.OnGUI();
    }

    /// <summary>
    /// Runs an update tick on the mod.
    /// </summary>
    public void Update()
    {
        GameStateProvider.Invalidate();
        MechanicalHeart.Update();
        this.HandleHotkeys();
    }

    /// <summary>
    /// Log an info-level message.
    /// </summary>
    /// <param name="message">The message to log.</param>
    public void LogInfo(string message)
    {
        NoonUtility.Log(message);
    }

    /// <summary>
    /// Log a trace-level message.
    /// </summary>
    /// <param name="message">The message to log.</param>
    public void LogTrace(string message)
    {
        NoonUtility.Log(message);
    }

    /// <summary>
    /// Log a warning-level message.
    /// </summary>
    /// <param name="message">The message to log.</param>
    public void LogWarn(string message)
    {
        NoonUtility.LogWarning(message);
    }

    /// <summary>
    /// Logs a warning-level message with an exception.
    /// </summary>
    /// <param name="ex">The exception to log.</param>
    /// <param name="message">The message.</param>
    public void LogWarn(Exception ex, string message)
    {
        NoonUtility.LogWarning($"{message}\n{ex.Message}\n{ex.StackTrace}");
    }

    /// <summary>
    /// Log and handle a fatal event.
    /// This will also stop the brain from running.
    /// </summary>
    /// <param name="message">The message to log.</param>
    public void Fatal(string message)
    {
        NoonUtility.LogWarning(message);
        GameAPI.Notify("Autoccultist Fatal", message);
        this.StopAutoccultist();
    }

    private void HandleSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (scene.name == "S4Tabletop")
        {
            GameEventSource.RaiseGameStarted();
        }
    }

    private void HandleSceneUnloaded(Scene scene)
    {
        if (scene.name == "S4Tabletop")
        {
            // TODO: This was called from HandleSceneLoaded but was called too late on victory, and we crash from not finding any spheres in GameStateProvider.FromCurrentState
            // Moved it to HandleSceneUnloaded... Does this fix the issue?
            GameEventSource.RaiseGameEnded();
        }
    }

    private void HandleHotkeys()
    {
        if (Keyboard.current[Key.F10].wasPressedThisFrame)
        {
            ControlGUI.IsShowing = !ControlGUI.IsShowing;
            if (!ControlGUI.IsShowing)
            {
                ArcsGUI.IsShowing = false;
                DiagnosticsGUI.IsShowing = false;
                GoalsGUI.IsShowing = false;
                ParseErrorsGUI.IsShowing = false;
            }
        }
    }

    private void StartAutoccultist()
    {
        Ego.Start();
        MechanicalHeart.Start();
    }

    private void StopAutoccultist()
    {
        MechanicalHeart.Stop();
        Ego.Stop();
        NucleusAccumbens.Reset();
        SituationOrchestrator.AbortAll();
    }
}
