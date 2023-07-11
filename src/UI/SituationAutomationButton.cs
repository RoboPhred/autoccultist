namespace AutoccultistNS.UI
{
    using SecretHistories.Entities;
    using SecretHistories.Manifestations;
    using SecretHistories.UI;
    using UnityEngine;
    using UnityEngine.EventSystems;
    using UnityEngine.UI;

    [RequireComponent(typeof(RectTransform))]
    [RequireComponent(typeof(Image))]
    public class SituationAutomationButton : MonoBehaviour, IPointerClickHandler
    {
        private const float RotationSpeedPerSecond = -360 / 8;

        private Sprite defaultSprite;
        private Sprite lockoutSprite;
        private Image image;

        public Situation Situation { get; set; }

        public SituationAutomationWindow Window { get; private set; }

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

            this.defaultSprite = ResourcesManager.GetSpriteForUI("autoccultist_situation_automation_badge");
            if (this.defaultSprite == null)
            {
                NoonUtility.LogWarning($"Could not find sprite autoccultist_situation_automation_badge");
            }

            this.lockoutSprite = ResourcesManager.GetSpriteForUI("autoccultist_situation_automation_badge_lockout");
            if (this.lockoutSprite == null)
            {
                NoonUtility.LogWarning($"Could not find sprite autoccultist_situation_automation_badge_lockout");
            }

            this.image = this.GetComponent<Image>();
            this.image.sprite = this.defaultSprite;
        }

        public void Update()
        {
            if (this.Window.IsLockedOut && this.image.sprite != this.lockoutSprite)
            {
                this.image.sprite = this.lockoutSprite;
            }
            else if (!this.Window.IsLockedOut && this.image.sprite != this.defaultSprite)
            {
                this.image.sprite = this.defaultSprite;
            }

            if (this.Window.IsAutomating)
            {
                this.transform.localRotation = Quaternion.Euler(0, 0, this.transform.localEulerAngles.z + (Time.deltaTime * RotationSpeedPerSecond));
            }
            else if (this.Window.IsLockedOut)
            {
                this.transform.localRotation = Quaternion.Euler(0, 0, 0);
            }
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            switch (eventData.button)
            {
                case PointerEventData.InputButton.Left:
                    this.OnLeftClick(eventData);
                    break;
                case PointerEventData.InputButton.Right:
                    this.OnRightClick(eventData);
                    break;
            }
        }

        private void OnLeftClick(PointerEventData eventData)
        {
            if (this.Window.IsVisible)
            {
                this.Window.Close();
                return;
            }

            var token = this.GetComponentInParent<Token>();
            this.Window.OpenAt(token.Location.LocalPosition);
        }

        private void OnRightClick(PointerEventData eventData)
        {
            this.Window.ToggleLockout();
        }
    }
}
