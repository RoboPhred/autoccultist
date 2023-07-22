namespace AutoccultistNS.UI
{
    using UnityEngine;

    [RequireComponent(typeof(RectTransform))]
    public class Spinner : MonoBehaviour
    {
        private bool isRunning = true;

        private RectTransform rectTransform;

        public float Speed { get; set; } = 1f;

        public void Start()
        {
            this.isRunning = true;
        }

        public void Stop()
        {
            this.isRunning = false;
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
