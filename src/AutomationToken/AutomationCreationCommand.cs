namespace AutoccultistNS.Tokens
{
    using System;
    using System.Collections.Generic;
    using Roost;
    using SecretHistories;
    using SecretHistories.Abstract;
    using SecretHistories.Commands.Encausting;
    using SecretHistories.Commands.SituationCommands;
    using SecretHistories.Services;
    using UnityEngine;

    public class AutomationCreationCommand : ITokenPayloadCreationCommand, IEncaustment
    {
        public AutomationCreationCommand()
        {
        }

        public AutomationCreationCommand(string imperativeId)
        {
            this.Id = $"!automation_{imperativeId}";
            this.EntityId = $"{AutomationPayload.EntityPrefix}{imperativeId}";
            this.Quantity = 1;
        }

        public string Id { get; set; }

        public string EntityId { get; set; }

        public int Quantity { get; set; }

        public bool Defunct { get; set; }

        public List<PopulateDominionCommand> Dominions { get; set; } = new();

        public static void Initialize()
        {
            Machine.Patch(
                original: typeof(PrefabFactory).GetMethodInvariant(nameof(PrefabFactory.CreateManifestationPrefab)),
                prefix: typeof(AutomationCreationCommand).GetMethodInvariant(nameof(HandleZoneManifestationPrefab)));

            Machine.Patch(
                original: typeof(EncaustablesSerializationBinder).GetConstructor(new Type[0]),
                postfix: typeof(AutomationCreationCommand).GetMethodInvariant(nameof(AddCommandToConstructor)));
        }

        public ITokenPayload Execute(Context context)
        {
            return new AutomationPayload(this.Id, this.EntityId);
        }

#pragma warning disable SA1313
        private static bool HandleZoneManifestationPrefab(Type manifestationType, Transform parent, ref IManifestation __result)
        {
            if (manifestationType != typeof(AutomationManifestation))
            {
                return true;
            }

            var go = new GameObject();
            go.transform.SetParent(parent);
            __result = go.AddComponent<AutomationManifestation>();
            return false;
        }

        private static void AddCommandToConstructor(EncaustablesSerializationBinder __instance)
        {
            var encaustmentTypes = Reflection.GetPrivateField<HashSet<Type>>(__instance, "encaustmentTypes");
            encaustmentTypes.Add(typeof(AutomationCreationCommand));
        }
#pragma warning restore SA1313
    }
}
