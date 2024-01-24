using TMPro;
using UnityEngine;

namespace UI
{
    public class GuideTip : MonoBehaviour
    {
        public void show(RectTransform targetTransform, string text)
        {
            if (!targetTransform)
            {
                hide();
                return;
            }
            transform.parent = targetTransform.parent;
            transform.SetAsLastSibling();
            this.targetTransform = targetTransform;
            tipboxText.text = text;
            gameObject.SetActive(true);
            time = 0;
            var selfTrans = transform as RectTransform;
            selfTrans.sizeDelta = targetTransform.rect.size;
            updateTransform();
        }
        public void hide()
        {
            gameObject.SetActive(false);
            targetTransform = null;
        }
        private void Update()
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
            time += Time.deltaTime;
            time %= interval;

            updateTransform();
        }
        private void updateTransform()
        {
            var center = targetTransform.localPosition;
            var size = targetTransform.rect.size;

            center += (Vector3)((Vector2.one * 0.5f - targetTransform.pivot) * size);
            var localPos = center;
            localPos.z = 0;
            transform.localPosition = localPos;
            transform.localScale = Vector3.one;

            var lerp = sizeCurve.Evaluate(time / interval);
            frameTransform.sizeDelta = Vector2.Lerp(size, size + sizeDelta, lerp);
        }
        [SerializeField]
        private GameObject rootObj;
        [Header("Frame")]
        [SerializeField]
        private RectTransform targetTransform;
        [SerializeField]
        private Vector2 sizeDelta = new Vector2(50,50);
        [SerializeField]
        private float interval = 1f;
        [SerializeField]
        private AnimationCurve sizeCurve;
        [SerializeField]
        private RectTransform frameTransform;

        [Header("Frame")]
        [SerializeField]
        private TextMeshProUGUI tipboxText;
        private float time;
    }
}
