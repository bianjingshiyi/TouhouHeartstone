using TMPro;
using UnityEngine;

namespace UI
{
    public class GuideTip : MonoBehaviour
    {
        public void show(GuideTipViewData viewData)
        {
            var targetTransform = viewData.targetTransform;
            var text = viewData.text;
            var tipOnTop = viewData.tipOnTop;
            this.targetTransform = targetTransform;

            var rect = targetTransform.rect;
            var size = rect.size;

            transform.parent = viewData.parent ?? defaultParent;
            transform.SetAsLastSibling();

            resetDragFingerPosition();
            updateTransform(0);

            tipboxText.text = text;
            tipboxTransform.pivot = new Vector2(0.5f, tipOnTop ? 0 : 1);
            tipboxTransform.anchorMin =  new Vector2(0.5f, tipOnTop ? 1 : 0);
            tipboxTransform.anchorMax = tipboxTransform.anchorMin;

            dragRootObj.SetActive(viewData.drag);
            if (viewData.drag)
            {
                dragRootTransform.anchoredPosition = size * (viewData.dragPosition * 0.5f);
                dragArrowTransform.localEulerAngles = Vector3.forward * Vector2.Angle(viewData.dragDirection, Vector2.right);
            }
        }
        public void hide()
        {
            targetTransform = null;
            resetDragFingerPosition();
        }
        public void update(float time)
        {
            if (!targetTransform)
                return;
            if (!targetTransform.gameObject.activeInHierarchy)
            {
                rootObj.SetActive(false);
                return;
            }
            else
            {
                rootObj.SetActive(true);
            }
            updateTransform(time);
        }
        private void resetDragFingerPosition()
        {
            dragFingerTransform.localPosition = Vector3.zero;
        }
        private void updateTransform(float time)
        {
            var center = targetTransform.position;
            var rect = targetTransform.rect;
            var pivot = targetTransform.pivot;
            var size = rect.size;

            var pos = center;
            pos.x += size.x * (0.5f - pivot.x) * targetTransform.lossyScale.x;
            pos.y += size.y * (0.5f - pivot.y) * targetTransform.lossyScale.y;
            pos.z = 0;
            transform.position = pos;
            transform.localScale = Vector3.one;

            var lerp = sizeCurve.Evaluate(time);
            frameTransform.sizeDelta = Vector2.Lerp(size, size + sizeDelta, lerp);

            var selfTrans = transform as RectTransform;
            selfTrans.sizeDelta = size;
        }
        [SerializeField]
        private GameObject rootObj;
        [SerializeField]
        private Transform defaultParent;

        [Header("Frame")]
        [SerializeField]
        private RectTransform targetTransform;
        [SerializeField]
        private Vector2 sizeDelta = new Vector2(50,50);
        [SerializeField]
        private AnimationCurve sizeCurve;
        [SerializeField]
        private RectTransform frameTransform;

        [Header("Tipbox")]
        [SerializeField]
        private TextMeshProUGUI tipboxText;
        [SerializeField]
        private RectTransform tipboxTransform;

        [Header("Drag")]
        [SerializeField]
        private GameObject dragRootObj;
        [SerializeField]
        private RectTransform dragRootTransform;
        [SerializeField]
        private Transform dragArrowTransform;
        [SerializeField]
        private Transform dragFingerTransform;
    }
    public struct GuideTipViewData
    {
        public Transform parent;
        public RectTransform targetTransform;
        public string text;
        public bool tipOnTop;
        public bool drag;
        public Vector2 dragPosition;
        public Vector2 dragDirection;
        public bool isValid() => targetTransform;
    }
}
