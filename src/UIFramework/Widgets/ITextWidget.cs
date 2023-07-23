namespace AutoccultistNS.UI
{
    using TMPro;
    using UnityEngine;

    public interface ITextWidget<TCoreType>
        where TCoreType : ITextWidget<TCoreType>
    {
        TextMeshProUGUI TextMesh { get; }

        string Font { get; set; }

        string FontMaterial { get; set; }

        float FontSize { get; set; }

        FontStyles FontStyle { get; set; }

        FontWeight FontWeight { get; set; }

        HorizontalAlignmentOptions HorizontalAlignment { get; set; }

        VerticalAlignmentOptions VerticalAlignment { get; set; }

        TextAlignmentOptions TextAlignment { get; set; }

        bool WordWrapping { get; set; }

        TextOverflowModes OverflowMode { get; set; }

        float MaxFontSize { get; set; }

        float MinFontSize { get; set; }

        string Text { get; set; }

        Color Color { get; set; }

        TCoreType SetFont(string resourceName);

        TCoreType SetFontMaterial(string resourceName);

        TCoreType SetFontSize(float size);

        TCoreType SetFontStyle(FontStyles style);

        TCoreType SetFontWeight(FontWeight weight);

        TCoreType SetHorizontalAlignment(HorizontalAlignmentOptions alignment);

        TCoreType SetVerticalAlignment(VerticalAlignmentOptions alignment);

        TCoreType SetTextAlignment(TextAlignmentOptions alignment);

        TCoreType SetWordWrapping(bool enabled);

        TCoreType SetOverflowMode(TextOverflowModes mode);

        TCoreType SetMaxFontSize(float size);

        TCoreType SetMinFontSize(float size);

        TCoreType SetText(string value);

        TCoreType SetColor(Color color);
    }
}
