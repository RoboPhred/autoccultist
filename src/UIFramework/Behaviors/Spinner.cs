namespace AutoccultistNS.UI
{
    using UnityEngine;

    [RequireComponent(typeof(RectTransform))]
    public class Spinner : MonoBehaviour
    {
        public const float DefaultSpeed = -360 / 16;

        private bool isRunning = true;

        private RectTransform rectTransform;

        public float Speed { get; set; } = DefaultSpeed;

        public Spinner SetSpeed(float speed)
        {
            this.Speed = speed;
            return this;
        }

        public Spinner StartSpinning()
        {
            this.isRunning = true;
            return this;
        }

        public Spinner StopSpinning()
        {
            this.isRunning = false;
            return this;
        }

        private void Awake()
        {
            this.rectTransform = this.GetComponent<RectTransform>();
        }

        private void Update()
        {
            if (!this.isRunning)
            {
                return;
            }

            this.rectTransform.Rotate(0f, 0f, this.Speed * Time.deltaTime);
        }
    }
}
