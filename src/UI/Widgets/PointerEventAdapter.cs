namespace AutoccultistNS.UI
{
    using System;
    using UnityEngine;
    using UnityEngine.EventSystems;

    public class PointerEventAdapter : MonoBehaviour, IPointerClickHandler, IPointerEnterHandler, IPointerExitHandler
    {
        public event EventHandler<PointerEventData> PointerClick;

        public event EventHandler<PointerEventData> PointerEnter;

        public event EventHandler<PointerEventData> PointerExit;

        public void OnPointerClick(PointerEventData eventData)
        {
            this.PointerClick?.Invoke(this, eventData);
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            this.PointerEnter?.Invoke(this, eventData);
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            this.PointerExit?.Invoke(this, eventData);
        }
    }
}
