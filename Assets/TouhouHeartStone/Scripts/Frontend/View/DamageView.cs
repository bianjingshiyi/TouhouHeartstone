using UnityEngine;
using UnityEngine.UI;

namespace TouhouHeartstone.Frontend.View
{
    public class DamageView : MonoBehaviour
    {
        [SerializeField]
        float duration = 0.5f;

        [SerializeField]
        Text text = null;

        [SerializeField]
        Vector3 offset = Vector3.zero;

        public void Show(int damage)
        {
            transform.localPosition = offset;
            text.text = damage.ToString();
            alpha = 1;
        }

        private void Update()
        {
            if (alpha > 0)
            {
                alpha -= Time.deltaTime / duration;
                if (alpha == 0)
                    Destroy(gameObject);
            }
        }

        float alpha
        {
            get
            {
                return text.color.a;
            }
            set
            {
                var c = text.color;
                c.a = Mathf.Clamp01(value);
                text.color = c;
            }
        }
    }
}
