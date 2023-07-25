namespace AutoccultistNS.UI
{
    using UnityEngine;
    using UnityEngine.EventSystems;

    [RequireComponent(typeof(Glow))]
    public class HoverGlow : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
    {
        private Glow glow;
        public void Awake()
        {
            this.glow = this.GetComponent<Glow>();
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            this.glow.Show();
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            this.glow.Hide();
        }
    }
}
