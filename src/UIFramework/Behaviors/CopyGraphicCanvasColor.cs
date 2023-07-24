namespace AutoccultistNS.UI
{
    using UnityEngine;
    using UnityEngine.UI;

    public class CopyGraphicCanvasColor : MonoBehaviour
    {
        public Graphic CopyFrom { get; set; }

        public Graphic CopyTo { get; set; }

        public void Update()
        {
            // Graphic.color is a red herring.  Button color behaviors set canvasRenderer color directly.
            this.CopyTo.canvasRenderer.SetColor(this.CopyFrom.canvasRenderer.GetColor());
        }
    }
}
