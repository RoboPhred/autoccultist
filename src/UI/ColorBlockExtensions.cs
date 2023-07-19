namespace AutoccultistNS.UI
{
    using UnityEngine.UI;

    public static class ColorBlockExtensions
    {
        public static ColorBlock Clone(this ColorBlock colorBlock)
        {
            return new ColorBlock
            {
                normalColor = colorBlock.normalColor,
                highlightedColor = colorBlock.highlightedColor,
                pressedColor = colorBlock.pressedColor,
                selectedColor = colorBlock.selectedColor,
                disabledColor = colorBlock.disabledColor,
                colorMultiplier = colorBlock.colorMultiplier,
                fadeDuration = colorBlock.fadeDuration,
            };
        }
    }
}
