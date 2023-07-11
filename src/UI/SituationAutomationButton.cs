namespace AutoccultistNS.UI
{
    using System;
    using System.Linq;
    using AutoccultistNS.GameState;
    using SecretHistories.Entities;
    using SecretHistories.Manifestations;
    using SecretHistories.UI;
    using UnityEngine;
    using UnityEngine.EventSystems;
    using UnityEngine.UI;


    [RequireComponent(typeof(RectTransform))]
    [RequireComponent(typeof(Image))]
    class SituationAutomationButton : MonoBehaviour, IPointerClickHandler, IPointerEnterHandler, IPointerExitHandler
    {
        private const float RotationSpeedPerSecond = -360 / 8;

        private Sprite sprite;

        public Situation Situation { get; set; }

        public SituationAutomationWindow Window { get; private set; }

        private bool IsAutomating
        {
            get
            {
                var situation = GameStateProvider.Current.Situations.FirstOrDefault(x => x.SituationId == this.Window.Situation.VerbId);
                return !GameResources.Resource.Of<ISituationState>().IsAvailable(situation);
            }
        }

        public static void AttachToSituation(Situation situation)
        {
            var manifestation = situation.Token.gameObject.GetComponentInChildren<VerbManifestation>();
            if (manifestation == null)
            {
                NoonUtility.LogWarning($"Could not find VerbManifestation on {situation.Token.PayloadId}");
                return;
            }

            if (manifestation.GetComponentInChildren<SituationAutomationButton>() != null)
            {
                return;
            }

            var button = new GameObject();
            var handler = button.AddComponent<SituationAutomationButton>();
            handler.Situation = situation;

            button.transform.SetParent(manifestation.gameObject.transform, false);
            button.transform.localPosition = new Vector3(-65, 65, 0);
        }

        public void Start()
        {
            this.Window = SituationAutomationWindow.CreateWindow(this.Situation);

            var rect = this.GetComponent<RectTransform>();
            rect.pivot = new Vector2(0.5f, 0.5f);
            rect.sizeDelta = new Vector2(50, 50);

            this.sprite = ResourcesManager.GetSpriteForUI("autoccultist_situation_automation_badge");
            if (sprite == null)
            {
                NoonUtility.LogWarning($"Could not find sprite autoccultist_situation_automation_badge");
            }

            this.GetComponent<Image>().sprite = this.sprite;
        }

        public void Update()
        {
            if (!this.IsAutomating)
            {
                return;
            }

            this.transform.localRotation = Quaternion.Euler(0, 0, this.transform.localEulerAngles.z + Time.deltaTime * RotationSpeedPerSecond);
        }

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
            this.Window.OpenAt(token.Location.LocalPosition);
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
        }

        public void OnPointerExit(PointerEventData eventData)
        {
        }
    }
}
