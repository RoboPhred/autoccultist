namespace AutoccultistNS
{
    using System;
    using System.Linq;
    using AutoccultistNS.GameState;
    using SecretHistories.Entities;
    using SecretHistories.UI;

    public static class Hippocampus
    {
        private const string MemoryElementId = "autoccultist_memory";

        public static void RegisterMemory(string id, string label, string description)
        {
            var compendium = Watchman.Get<Compendium>();
            id = GetMemoryId(id);

            Element attr;
            if (compendium.EntityExists<Element>(id))
            {
                attr = compendium.GetEntityById<Element>(id);
                NoonUtility.LogWarning($"Memory {id} already exists.");

                if (!string.IsNullOrEmpty(label))
                {
                    attr.Label = label;
                }

                if (!string.IsNullOrEmpty(description))
                {
                    attr.Description = description;
                }
            }
            else
            {
                NoonUtility.LogWarning($"Memory {id} does not yet exist.");
                attr = new Element
                {
                    Label = label ?? id,
                    Description = description ?? string.Empty,
                    NoArtNeeded = true,
                    IsAspect = true,
                    // Fucine defaults
                    Comments = string.Empty,
                    Icon = string.Empty,
                    VerbIcon = string.Empty,
                    DecayTo = string.Empty,
                    BurnTo = string.Empty,
                    DrownTo = string.Empty,
                    UniquenessGroup = string.Empty,
                    Sort = "zzz.zzz",
                    ManifestationType = "Card",
                    Audio = "Card",
                    Achievements = new(),
                    Inherits = string.Empty,
                    Aspects = new(),
                    Commute = new(),
                    Slots = new(),
                    XTriggers = new(),
                    Xexts = new(),
                };
                attr.SetId(id);
                compendium.TryAddEntity(attr);
            }
        }

        public static void SetMemory(string id, string label, string description, int value)
        {
            if (!GameAPI.IsRunning)
            {
                throw new InvalidOperationException("Cannot set memory outside of game.");
            }

            RegisterMemory(id, label, description);

            // RegisterMemory also does this, so wait until we call that before applying it ourselves.
            id = GetMemoryId(id);

            var memoryElement = GetMemoryElementStack();
            memoryElement.SetMutation(id, value, false);

            NoonUtility.LogWarning($"Set memory {id} to {memoryElement.GetAspects()[id]}.");

            GameStateProvider.Invalidate();
        }

        public static void AddMemory(string id, string label, string description, int value)
        {
            if (!GameAPI.IsRunning)
            {
                throw new InvalidOperationException("Cannot set memory outside of game.");
            }

            RegisterMemory(id, label, description);

            // RegisterMemory also does this, so wait until we call that before applying it ourselves.
            id = GetMemoryId(id);

            var memoryElement = GetMemoryElementStack();
            memoryElement.SetMutation(id, value, false);
            NoonUtility.LogWarning($"Set memory {id} to {memoryElement.GetAspects()[id]}.");

            GameStateProvider.Invalidate();
        }

        private static ElementStack GetMemoryElementStack()
        {
            var tabletop = GameAPI.TabletopSphere;

            var token = tabletop.GetTokens().FirstOrDefault(t => t.PayloadEntityId == MemoryElementId);
            if (token == null)
            {
                token = tabletop.ProvisionElementToken(MemoryElementId, 1);
                NoonUtility.LogWarning($"Created memory token {token.PayloadEntityId} in sphere {token.Sphere.Id}.");
            }

            // I don't actually know if this is needed.  The bug turned out to be something else entirely.
            // But it works with it, so meh.
            token.Manifest();

            return token.Payload as ElementStack;
        }

        private static string GetMemoryId(string id)
        {
            return $"{MemoryElementId}_{id}";
        }
    }
}
