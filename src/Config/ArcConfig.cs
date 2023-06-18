namespace AutoccultistNS.Config
{
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Threading.Tasks;
    using AutoccultistNS.Brain;
    using AutoccultistNS.Config.Conditions;
    using AutoccultistNS.Yaml;
    using SecretHistories.Commands.Encausting;
    using SecretHistories.Entities;
    using SecretHistories.Enums;
    using SecretHistories.Infrastructure.Persistence;
    using SecretHistories.UI;
    using YamlDotNet.Core;

    /// <summary>
    /// The configuration for an <see cref="IArc"/>.
    /// </summary>
    [LibraryPath("arcs")]
    public class ArcConfig : NamedConfigObject, IArc
    {
        private string arcFolder;

        /// <summary>
        /// Gets or sets the legacy this arc is designed to run on, if any.
        /// </summary>
        public string Legacy { get; set; }

        /// <summary>
        /// Gets the file path to the save file to use when starting this arc fresh.
        /// Leave as null if none is specified.
        /// If specified, this will take precidence over <see cref="Legacy"/> when starting new games from arcs.
        /// </summary>
        public string StartFromSave { get; set; }

        /// <summary>
        /// Gets or sets a list of motivations that will drive the execution of this arc.
        /// </summary>
        public List<MotivationConfig> Motivations { get; set; } = new();

        /// <summary>
        /// Gets or sets the selection hint to be used to determine the current arc on loading a save.
        /// </summary>
        public IGameStateConditionConfig SelectionHint { get; set; }

        /// <inheritdoc/>
        IReadOnlyList<IMotivation> IArc.Motivations => this.Motivations;

        /// <inheritdoc/>
        IGameStateCondition IArc.SelectionHint => this.SelectionHint;

        public bool SupportsNewGame
        {
            get
            {
                if (!string.IsNullOrEmpty(this.StartFromSave))
                {
                    return File.Exists(Path.Combine(this.arcFolder, this.StartFromSave));
                }

                if (!string.IsNullOrEmpty(this.Legacy))
                {
                    var legacies = Watchman.Get<Compendium>().GetEntitiesAsList<Legacy>();
                    var legacy = legacies.Find(l => l.Id == this.Legacy);
                    return legacy != null;
                }

                return false;
            }
        }

        /// <summary>
        /// Loads an ArcConfig from the given file.
        /// </summary>
        /// <param name="filePath">The path to the config file to load.</param>
        /// <returns>A ArcConfig loaded from the file.</returns>
        /// <exception cref="Brain.Config.InvalidConfigException">The config file at the path contains invalid configuration.</exception>
        public static ArcConfig Load(string filePath)
        {
            return Deserializer.Deserialize<ArcConfig>(filePath);
        }

        /// <inheritdoc/>
        public override void AfterDeserialized(Mark start, Mark end)
        {
            base.AfterDeserialized(start, end);

            this.arcFolder = Path.GetDirectoryName(Deserializer.CurrentFilePath);

            if (string.IsNullOrEmpty(this.Name))
            {
                this.Name = NameGenerator.GenerateName(Deserializer.CurrentFilePath, start);
            }

            if (this.Motivations == null || this.Motivations.Count == 0)
            {
                throw new InvalidConfigException("Brain must have at least one motivation.");
            }

            if (!string.IsNullOrEmpty(this.StartFromSave))
            {
                var fullPath = Path.Combine(this.arcFolder, this.StartFromSave);
                if (!File.Exists(fullPath))
                {
                    throw new InvalidConfigException($"Base save file \"{fullPath}\" does not exist.");
                }
            }

            if (!string.IsNullOrEmpty(this.Legacy))
            {
                var legacies = Watchman.Get<Compendium>().GetEntitiesAsList<Legacy>();
                if (!legacies.Any(l => l.Id == this.Legacy))
                {
                    throw new InvalidConfigException($"Legacy \"{this.Legacy}\" does not exist.");
                }
            }
        }

        public GamePersistenceProvider GetNewGameProvider()
        {
            // Base saves take priority over legacy.
            if (!string.IsNullOrEmpty(this.StartFromSave))
            {
                var path = Path.Combine(this.arcFolder, this.StartFromSave);
                if (!File.Exists(path))
                {
                    return null;
                }

                return new ArcPersistenceProvider(path);
            }

            if (!string.IsNullOrEmpty(this.Legacy))
            {
                var legacies = Watchman.Get<Compendium>().GetEntitiesAsList<Legacy>();
                var legacy = legacies.Find(l => l.Id == this.Legacy);
                if (legacy == null)
                {
                    return null;
                }

                return new FreshGameProvider(legacy);
            }

            return null;
        }

        private class ArcPersistenceProvider : GamePersistenceProvider
        {
            private readonly string arcSavePath;

            public ArcPersistenceProvider(string arcSavePath)
            {
                this.arcSavePath = arcSavePath;
            }

            public override GameSpeed GetDefaultGameSpeed()
            {
                return GameSpeed.Fast;
            }

            public override async Task<bool> SerialiseAndSaveAsyncWithDefaultSaveName()
            {
                // Save as a 'normal' game.
                return await this.SerialiseAndSaveAsync("save.json");
            }

            public override void DepersistGameState()
            {
                this._saveValidity = GamePersistenceProvider.SaveValidity.OK;
                this._persistedGameState = new SerializationHelper().DeserializeFromJsonString<PersistedGameState>(File.ReadAllText(this.arcSavePath));
            }

            protected override string GetSaveFileLocation()
            {
                return this.GetSaveFileLocation("save.json");
            }

            protected override string GetSaveFileLocation(string saveName)
            {
                return Watchman.Get<MetaInfo>().PersistentDataPath + "/" + saveName;
            }
        }
    }
}
