using System;
using System.IO;
using System.Linq;
using System.Threading;
using AutoccultistNS;
using AutoccultistNS.Brain;
using AutoccultistNS.Config;
using AutoccultistNS.GameResources;
using AutoccultistNS.GameState;
using AutoccultistNS.GUI;
using AutoccultistNS.UI;
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
    public static void LogWarn(Exception ex, string message = null)
    {
        if (!string.IsNullOrEmpty(message))
        {
            message = $"{message}\n{ex.Message}\n{ex.ToString()}";
        }
        else
        {
            message = $"{ex.Message}\n{ex.ToString()}";
        }

        NoonUtility.LogWarning(message);
    }

    /// <summary>
    /// Starts the mod.
    /// </summary>
    public void Start()
    {
        Instance = this;

        this.mainThread = Thread.CurrentThread;

        GameEventSource.GameStarted += this.HandleGameStarted;
        GameEventSource.GameEnded += this.HandleGameEnded;

        SceneManager.sceneLoaded += new UnityAction<Scene, LoadSceneMode>(this.HandleSceneLoaded);
        SceneManager.sceneUnloaded += new UnityAction<Scene>(this.HandleSceneUnloaded);

        // Run initializations inside a sync context to capture created threads.
        AutoccultistSettings.Initialize();
        GameAPI.Initialize();

        SituationManifestationUIManager.Initialize();

        // The order is important for these initializers, so they take subscriptions to MechanicalHeart.OnBeat in the right order.
        NucleusAccumbens.Initialize();

        this.ReloadAll();

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

        if (Library.ParseErrors.Count > 0)
        {
            ParseErrorsGUI.IsShowing = true;
        }
        else
        {
            ParseErrorsGUI.IsShowing = false;
            if (GameAPI.IsRunning)
            {
                var arc = Library.Arcs.FirstOrDefault(a => a.Name == previousArc.Name);
                if (arc != null)
                {
                    NucleusAccumbens.AddImperative(arc);
                }
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
        PerfMonitor.Monitor($"Autoccultist Update", () =>
        {
            // We want all our await continuations to be handled on this frame.
            // If we had continuations scheduled up outside of this from last frame, they will
            // be executed before our action here gets executed.
            // In practice, this is no longer needed as all our awaits are continuing from within GlobalUpdate,
            // but this was previously useful when we awaited Task.Delay which resumes from another thread and was synchronized
            // into unity's SynchronizationContext.
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

            // FIXME: GameEnded should call this.  GameStarted starts us properly, so why does GameEnded not?
            // If we reload the same scene do we never get SceneUnloaded?
            this.StopAutoccultist();
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
        Cerebellum.AbortAllActions();
        GameResource.ClearAll();
        CacheUtils.ClearStatistics();
        PerfMonitor.ClearStatistics();
    }

    private void HandleGameStarted(object sender, EventArgs e)
    {
        if (this.loadArcOnGameStart != null)
        {
            LogTrace($"Game started with a scheduled arc {this.loadArcOnGameStart.Name}");
            NucleusAccumbens.AddImperative(this.loadArcOnGameStart);
            this.loadArcOnGameStart = null;
            this.StartAutoccultist();
        }

        // TODO: Autoselect an arc.
    }

    private void HandleGameEnded(object sender, EventArgs e)
    {
        LogTrace("Game ended, stopping Autoccultist.");
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
