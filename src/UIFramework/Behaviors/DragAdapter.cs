namespace AutoccultistNS.UI
{
    using System;
    using UnityEngine;
    using UnityEngine.EventSystems;

    public class DragAdapter : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
    {
        public event EventHandler<PointerEventData> BeginDrag;

        public event EventHandler<PointerEventData> Drag;

        public event EventHandler<PointerEventData> EndDrag;

        public void OnBeginDrag(PointerEventData eventData)
        {
            this.BeginDrag?.Invoke(this, eventData);
        }

        public void OnDrag(PointerEventData eventData)
        {
            this.Drag?.Invoke(this, eventData);
        }

        public void OnEndDrag(PointerEventData eventData)
        {
            this.EndDrag?.Invoke(this, eventData);
        }
    }
}
