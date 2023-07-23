namespace AutoccultistNS.Cronicle
{
    using System;
    using System.IO;
    using AutoccultistNS.Brain;
    using SecretHistories.Entities;
    using SecretHistories.UI;
    using UnityEngine;

    public static class AutoccultistCronicle
    {
        private static string activeFolder = null;

        private static string CroniclesPath => Path.GetFullPath(Path.Combine(Watchman.Get<MetaInfo>().PersistentDataPath, "cronicles"));

        public static void Start(string folder, string description = null, bool addend = false)
        {
            Stop();

            if (folder == null)
            {
                throw new ArgumentNullException(nameof(folder));
            }

            folder = Path.Combine(CroniclesPath, folder);

            if (!Directory.Exists(CroniclesPath))
            {
                Directory.CreateDirectory(CroniclesPath);
            }

            if (!addend)
            {
                FilesystemHelpers.DeleteDirectory(folder);
            }

            if (!Directory.Exists(folder))
            {
                Directory.CreateDirectory(folder);
            }

            activeFolder = folder;

            if (!string.IsNullOrEmpty(description))
            {
                File.AppendAllText(CroniclePath("description", "txt"), description + "\n");
            }

            BrainEvents.OperationStarted += OnOperationStarted;
            BrainEvents.OperationCompleted += OnOperationCompleted;
            BrainEvents.OperationAborted += OnOperationAborted;
        }

        public static void Stop()
        {
            activeFolder = null;

            BrainEvents.OperationStarted -= OnOperationStarted;
            BrainEvents.OperationCompleted -= OnOperationCompleted;
            BrainEvents.OperationAborted -= OnOperationAborted;
        }

        private static void OnOperationStarted(object sender, OperationEventArgs e)
        {
            Snapshot($"{e.Operation.Id}_start");
        }

        private static void OnOperationCompleted(object sender, OperationEventArgs e)
        {
            Snapshot($"{e.Operation.Id}_complete");
        }

        private static void OnOperationAborted(object sender, OperationEventArgs e)
        {
            Snapshot($"{e.Operation.Id}_abort");
        }

        private static void Snapshot(string addendum)
        {
            ScreenCapture.CaptureScreenshot(CroniclePath(addendum, "png"));
        }

        private static string CroniclePath(string name, string ext)
        {
            return Path.GetFullPath(Path.Combine(CroniclesPath, activeFolder, Filename(name, ext)));
        }

        private static string Filename(string name, string ext)
        {
            name = name.Replace(".", "_").Replace(" ", "_").Replace("/", "_").Replace("\\", " ");
            return $"{DateTime.Now:yyyy-MM-dd_HH-mm-ss}_{name}.{ext}";
        }
    }
}
