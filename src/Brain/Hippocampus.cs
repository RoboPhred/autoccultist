namespace AutoccultistNS
{
    using System;
    using System.Linq;
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
                attr = new Element
                {
                    IsAspect = true,
                    Label = label ?? id,
                    Description = description ?? string.Empty,
                };
                attr.SetId(GetMemoryId(id));
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
        }

        private static ElementStack GetMemoryElementStack()
        {
            var tabletop = GameAPI.TabletopSphere;

            var token = tabletop.GetTokens().FirstOrDefault(t => t.PayloadEntityId == MemoryElementId);
            if (token == null)
            {
                token = tabletop.ProvisionElementToken(MemoryElementId, 1);
            }

            return token.Payload as ElementStack;
        }

        private static string GetMemoryId(string id)
        {
            return $"{MemoryElementId}_{id}";
        }
    }
}
