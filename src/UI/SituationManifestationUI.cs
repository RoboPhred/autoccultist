namespace AutoccultistNS.UI
{
    using System;
    using SecretHistories.Entities;
    using SecretHistories.Manifestations;
    using SecretHistories.UI;
    using UnityEngine;
    using UnityEngine.EventSystems;
    using UnityEngine.UI;

    public class SituationManifestationUI : IDisposable
    {
        private readonly Situation situation;
        private readonly GameObject button;
        private readonly SituationAutomationWindow window;

        public SituationManifestationUI(Situation situation)
        {
            this.situation = situation;

            var manifestation = situation.Token.gameObject.GetComponentInChildren<VerbManifestation>();
            if (manifestation == null)
            {
                NoonUtility.LogWarning($"Could not find VerbManifestation on {situation.Token.PayloadId}");
                return;
            }

            this.window = SituationAutomationWindow.CreateWindow($"Window_{situation.VerbId}_automation");
            this.window.Attach(situation);

            this.button = this.CreateButton(this.window);
            this.button.transform.SetParent(manifestation.gameObject.transform, false);
            this.button.transform.localPosition = new Vector3(-65, 65, 0);
        }

        public void Dispose()
        {
            GameObject.Destroy(this.window.gameObject);
            GameObject.Destroy(this.button);
        }

        private GameObject CreateButton(SituationAutomationWindow window)
        {
            var button = new GameObject();

            var sprite = ResourceHack.FindSprite("situation_completions_badge");
            if (sprite == null)
            {
                NoonUtility.LogWarning($"Could not find sprite situation_completions_badge");
                return button;
            }

            var rect = button.AddComponent<RectTransform>();
            rect.sizeDelta = new Vector2(60, 60);

            var image = button.AddComponent<Image>();
            image.sprite = sprite;

            var handler = button.AddComponent<ButtonHandler>();
            handler.Window = window;

            return button;
        }

        private class ButtonHandler : MonoBehaviour, IPointerClickHandler, IPointerEnterHandler, IPointerExitHandler
        {
            public SituationAutomationWindow Window { get; set; }

            public void OnPointerClick(PointerEventData eventData)
            {
                if (this.Window.IsVisible)
                {
                    NoonUtility.LogWarning("Window is visible, closing");
                    this.Window.Close();
                    return;
                }

                NoonUtility.LogWarning("Window is not visible, opening");

                var token = this.GetComponentInParent<Token>();
                this.Window.Show(token.Location.LocalPosition);
            }

            public void OnPointerEnter(PointerEventData eventData)
            {
            }

            public void OnPointerExit(PointerEventData eventData)
            {
            }
        }
    }
}
