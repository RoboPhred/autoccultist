namespace AutoccultistNS.UI
{
    using UnityEngine;
    using UnityEngine.UI;

    public class CopyColorFrom : MonoBehaviour
    {
        public Image CopyFrom { get; set; }

        public void Update()
        {
            // TODO: Does unity bust any caches when we do this?  Should we check to see if the change is needed?
            this.GetComponent<Image>().color = this.CopyFrom.color;
        }
    }
}
