namespace AutoccultistNS
{
    using System.Collections.Generic;
    using System.Linq;
    using TMPro;
    using UnityEngine;

    public static class ResourceHack
    {
        private static readonly Dictionary<string, Material> materialCache = new();
        private static readonly Dictionary<string, TMP_FontAsset> fontCache = new();
        private static readonly Dictionary<string, Sprite> spriteCache = new();

        public static Material FindMaterial(string name)
        {
            if (materialCache.TryGetValue(name, out var material))
            {
                return material;
            }

            material = UnityEngine.Resources.FindObjectsOfTypeAll<Material>().FirstOrDefault(x => x.name == name);
            if (material == null)
            {
                NoonUtility.LogWarning($"Could not find material {name}");
                return null;
            }

            materialCache[name] = material;
            return material;
        }

        public static TMP_FontAsset FindFont(string name)
        {
            if (fontCache.TryGetValue(name, out var font))
            {
                return font;
            }

            font = UnityEngine.Resources.FindObjectsOfTypeAll<TMP_FontAsset>().FirstOrDefault(x => x.name == name);
            if (font == null)
            {
                NoonUtility.LogWarning($"Could not find font {name}");
                return null;
            }

            fontCache[name] = font;
            return font;
        }

        public static Sprite FindSprite(string name)
        {
            if (spriteCache.TryGetValue(name, out var sprite))
            {
                return sprite;
            }

            sprite = UnityEngine.Resources.FindObjectsOfTypeAll<Sprite>().FirstOrDefault(x => x.name == name);
            if (sprite == null)
            {
                NoonUtility.LogWarning($"Could not find sprite {name}");
                return null;
            }

            spriteCache[name] = sprite;
            return sprite;
        }
    }
}
