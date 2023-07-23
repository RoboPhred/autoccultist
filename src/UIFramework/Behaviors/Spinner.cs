namespace AutoccultistNS.UI
{
    using UnityEngine;

    [RequireComponent(typeof(RectTransform))]
    public class Spinner : MonoBehaviour
    {
        private bool isRunning = true;

        private RectTransform rectTransform;

        public float Speed { get; set; } = 1f;

        public void StartSpinning()
        {
            NoonUtility.LogWarning("Spinner started");
            this.isRunning = true;
        }

        public void StopSpinning()
        {
            NoonUtility.LogWarning("Spinner stopped");
            this.isRunning = false;
        }

        private void Awake()
        {
            this.rectTransform = this.GetComponent<RectTransform>();
        }

        private void Update()
        {
            NoonUtility.LogWarning("Spinner update " + this.isRunning);

            if (!this.isRunning)
            {
                return;
            }

            this.rectTransform.Rotate(0f, 0f, this.Speed * Time.deltaTime);
        }
    }
}
