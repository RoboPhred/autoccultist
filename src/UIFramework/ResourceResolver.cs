namespace AutoccultistNS.UI
{
    using System.IO;
    using SecretHistories.Infrastructure.Modding;
    using SecretHistories.UI;
    using UnityEngine;

    public static class ResourceResolver
    {
        private const string DefaultCategory = "ui";

        public static Sprite GetSprite(string resourceName)
        {
            var parts = resourceName.Split(':');
            if (parts.Length == 1)
            {
                parts = new[] { DefaultCategory, parts[0] };
            }

            switch (parts[0])
            {
                case "ui":
                    return GetSpriteFromFolder("ui", parts[1]);
                case "aspect":
                    return GetSpriteFromFolder("aspects", parts[1]);
                case "element":
                    return GetSpriteFromFolder("elements", parts[1]);
                case "legacy":
                    return GetSpriteFromFolder("legacies", parts[1]);
                case "verb":
                    return GetSpriteFromFolder("verbs", parts[1]);
                case "internal":
                    return ResourceHack.FindSprite(parts[1]);
            }

            NoonUtility.LogWarning($"Unknown sprite category {parts[0]}");
            return null;
        }

        private static Sprite GetSpriteFromFolder(string folder, string resourceName)
        {
            // This is a partial reimplementation of ResourcesManager.GetSpriteForUI
            // We want to return null if the sprite is not found, while GetSpriteForUI returns a fallback.
            string uiPath = Path.Combine(Watchman.Get<SecretHistories.Services.Config>().GetConfigValue("imagesdir"), folder, resourceName);

            var sprite = Watchman.Get<ModManager>().GetSprite(uiPath);
            if (sprite == null)
            {
                sprite = Resources.Load<Sprite>(uiPath);
            }

            return sprite;
        }
    }
}
