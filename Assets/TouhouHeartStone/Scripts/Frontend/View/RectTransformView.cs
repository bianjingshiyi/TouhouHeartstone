using UnityEngine;

namespace TouhouHeartstone.Frontend.View
{
    [RequireComponent(typeof(RectTransform))]
    public class RectTransformView : MonoBehaviour
    {
        RectTransform _rt;
        RectTransform rt
        {
            get
            {
                _rt = _rt ?? GetComponent<RectTransform>();
                return _rt;
            }
        }
        public float Width
        {
            get
            {
                return rt.offsetMax.x - rt.offsetMin.x;
            }
            set
            {
                var ofstMax = rt.offsetMax;
                ofstMax.x = rt.offsetMin.x + value;
                rt.offsetMax = ofstMax;
            }
        }
    }
}
