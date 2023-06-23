using System;
using System.IO;
using System.Linq;
using System.Threading;
using AutoccultistNS;
using AutoccultistNS.Brain;
using AutoccultistNS.Config;
using AutoccultistNS.GameState;
using AutoccultistNS.GUI;
using AutoccultistNS.Yaml;
using SecretHistories.Services;
using SecretHistories.UI;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

/// <summary>
/// The main entrypoint for Autoccultist.
/// </summary>
public class Autoccultist : MonoBehaviour
{
    private IArc loadArcOnGameStart;

    private Thread mainThread;

    public static event EventHandler GlobalUpdate;

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
    /// Log an info-level message.
    /// </summary>
    /// <param name="message">The message to log.</param>
    public static void LogInfo(string message)
    {
        NoonUtility.Log(message);
    }

    /// <summary>
    /// Log a trace-level message.
    /// </summary>
    /// <param name="message">The message to log.</param>
    public static void LogTrace(string message)
    {
        NoonUtility.Log(message);
    }

    /// <summary>
    /// Log a warning-level message.
    /// </summary>
    /// <param name="message">The message to log.</param>
    public static void LogWarn(string message)
    {
        NoonUtility.LogWarning(message);
    }

    /// <summary>
    /// Logs a warning-level message with an exception.
    /// </summary>
    /// <param name="ex">The exception to log.</param>
    /// <param name="message">The message.</param>
    public static void LogWarn(Exception ex, string message)
    {
        NoonUtility.LogWarning($"{message}\n{ex.Message}\n{ex.ToString()}");
    }

    /// <summary>
    /// Starts the mod.
    /// </summary>
    public void Start()
    {
        Instance = this;

        this.mainThread = Thread.CurrentThread;

        SceneManager.sceneLoaded += new UnityAction<Scene, LoadSceneMode>(this.HandleSceneLoaded);
        SceneManager.sceneUnloaded += new UnityAction<Scene>(this.HandleSceneUnloaded);

        AutoccultistSettings.Initialize();
        GameAPI.Initialize();

        // The order is important for these initializers, so they take subscriptions to MechanicalHeart.OnBeat in the right order.
        NucleusAccumbens.Initialize();
        SituationOrchestrator.Initialize();

        this.ReloadAll();
        if (Library.ParseErrors.Count > 0)
        {
            ParseErrorsGUI.IsShowing = true;
        }

        GameEventSource.GameStarted += (_, __) =>
        {
            if (this.loadArcOnGameStart != null)
            {
                LogTrace($"Game started with a scheduled arc {this.loadArcOnGameStart.Name}");
                NucleusAccumbens.AddImperative(this.loadArcOnGameStart);
                this.loadArcOnGameStart = null;
                this.StartAutoccultist();
            }

            // TODO: Autoselect an arc.
        };

        GameEventSource.GameEnded += (_, __) =>
        {
            this.StopAutoccultist();
        };

        LogInfo("Autoccultist initialized.");
    }

    public void EnsureMainThread()
    {
        if (Thread.CurrentThread != this.mainThread)
        {
            throw new InvalidOperationException("This method must be called from the main thread.");
        }
    }

    /// <summary>
    /// Reload all tasks in the TaskDriver.
    /// </summary>
    public void ReloadAll()
    {
        var previousArc = NucleusAccumbens.CurrentImperatives.OfType<IArc>().FirstOrDefault();

        this.StopAutoccultist();
        LogInfo("Reloading all configs");

        Deserializer.ClearCache();
        Library.LoadAll();

        if (GameAPI.IsRunning)
        {
            var arc = Library.Arcs.FirstOrDefault(a => a.Name == previousArc.Name);
            if (arc != null)
            {
                NucleusAccumbens.AddImperative(arc);
            }
        }
    }

    /// <summary>
    /// Renders the mod GUI.
    /// </summary>
    public void OnGUI()
    {
        WindowManager.OnPreGUI();

        ControlGUI.OnGUI();
        ParseErrorsGUI.OnGUI();
        NewGameGUI.OnGUI();

        if (!GameAPI.IsRunning)
        {
            return;
        }

        DiagnosticsGUI.OnGUI();
        PerformanceGUI.OnGUI();
        GoalsGUI.OnGUI();
        ArcsGUI.OnGUI();
    }

    /// <summary>
    /// Runs an update tick on the mod.
    /// </summary>
    public void Update()
    {
        PerfMonitor.Monitor($"Global Update", () =>
        {
            // We want all our await continuations to be handled on this frame.
            // If we had continuations scheduled up outside of this from last frame, they will
            // be executed before our action here gets executed.
            ImmediateSynchronizationContext.Run(() =>
            {
                this.HandleHotkeys();

                GameStateProvider.Invalidate();
                MechanicalHeart.Update();
                GlobalUpdate?.Invoke(this, EventArgs.Empty);
            });
        });
    }

    public void StartNewGame(IArc arc)
    {
        if (arc == null)
        {
            LogWarn("Cannot StartNewGame with null arc");
            return;
        }

        if (!arc.SupportsNewGame)
        {
            LogWarn($"Cannot start a new game based on arc {arc.Name}: This arc does not support new games.");
            return;
        }

        var provider = arc.GetNewGameProvider();
        if (provider != null)
        {
            LogTrace($"Starting a new game with arc {arc.Name}");
            this.loadArcOnGameStart = arc;
            Watchman.Get<StageHand>().LoadGameOnTabletop(provider);
        }
        else
        {
            LogWarn($"Cannot start a new game based on arc {arc.Name}: No provider was found.");
        }
    }

    public void StartAutoccultist()
    {
        MechanicalHeart.Start();
    }

    public void StopAutoccultist()
    {
        MechanicalHeart.Stop();
        NucleusAccumbens.Reset();
        SituationOrchestrator.AbortAll();
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
}
