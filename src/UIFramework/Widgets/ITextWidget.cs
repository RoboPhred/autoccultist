namespace AutoccultistNS.UI
{
    using TMPro;
    using UnityEngine;

    public interface ITextWidget<TCoreType>
        where TCoreType : ITextWidget<TCoreType>
    {
        TextMeshProUGUI TextMeshBehavior { get; }

        TCoreType Font(string resourceName);

        TCoreType FontMaterial(string resourceName);

        TCoreType FontSize(float size);

        TCoreType FontStyle(FontStyles style);

        TCoreType FontWeight(FontWeight weight);

        TCoreType HorizontalAlignment(HorizontalAlignmentOptions alignment);

        TCoreType MaxFontSize(float size);

        TCoreType MinFontSize(float size);

        TCoreType Text(string value);

        TCoreType TextAlignment(TextAlignmentOptions alignment);

        TCoreType TextColor(Color color);
    }
}
