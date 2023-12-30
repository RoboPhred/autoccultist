namespace AutoccultistNS.Cronicle
{
    using System;
    using System.IO;
    using System.Text;
    using System.Threading.Tasks;
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

            BrainEvents.OperationRecipeExecuted += OnOperationRecipeExecuted;
            BrainEvents.OperationCompleted += OnOperationCompleted;
            BrainEvents.OperationAborted += OnOperationAborted;
        }

        public static void Stop()
        {
            activeFolder = null;

            BrainEvents.OperationRecipeExecuted -= OnOperationRecipeExecuted;
            BrainEvents.OperationCompleted -= OnOperationCompleted;
            BrainEvents.OperationAborted -= OnOperationAborted;
        }

        private static void OnOperationRecipeExecuted(object sender, OperationRecipeEventArgs e)
        {
            Snapshot($"{e.Operation.Id}_recipe_{e.RecipeName}");

            var sb = new StringBuilder();
            sb.AppendLine($"Operation: {e.Operation.Name} [{e.Operation.Id}]");
            sb.AppendLine($"Recipe: {e.RecipeName}");
            sb.AppendLine("Input Cards");
            foreach (var pair in e.SlottedCards)
            {
                sb.AppendLine($"  {pair.Key}");
                if (pair.Value == null)
                {
                    continue;
                }
                else
                {
                    sb.AppendLine($"    {pair.Value.ElementId}");
                    foreach (var aspect in pair.Value.Aspects)
                    {
                        sb.AppendLine($"      {aspect.Key}: {aspect.Value}");
                    }
                }
            }

            File.WriteAllText(CroniclePath($"{e.Operation.Id}_recipe_{e.RecipeName}", "txt"), sb.ToString());
        }

        private static void OnOperationCompleted(object sender, OperationCompletedEventArgs e)
        {
            Snapshot($"{e.Operation.Id}_complete");

            var sb = new StringBuilder();
            sb.AppendLine("Output cards:");
            foreach (var output in e.OutputCards)
            {
                sb.AppendLine("  " + output.ElementId);
                foreach (var pair in output.Aspects)
                {
                    sb.AppendLine($"    {pair.Key}: {pair.Value}");
                }
            }

            File.WriteAllText(CroniclePath($"{e.Operation.Id}_complete", "txt"), sb.ToString());
        }

        private static void OnOperationAborted(object sender, OperationEventArgs e)
        {
            Snapshot($"{e.Operation.Id}_abort");
        }

        private static void Snapshot(string addendum)
        {
            // This seems to be delayed a few frames.
            // We want a screenshot immediately.
            // ScreenCapture.CaptureScreenshot(CroniclePath(addendum, "png"));
            var texture = PerfMonitor.Monitor("Snapshot.CaptureScreenshotAsTexture", () => ScreenCapture.CaptureScreenshotAsTexture());

            // This is slow.
            Task.Run(() =>
            {
                var encoded = ImageConversion.EncodeToPNG(texture);
                File.WriteAllBytes(CroniclePath(addendum, "png"), encoded);
            });
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
