using UnityEngine;

namespace IGensoukyo.Utilities
{
    [RequireComponent(typeof(RectTransform))]
    [RequireComponent(typeof(SpriteRenderer))]
    [ExecuteInEditMode]
    public class SpriteAutoScale : MonoBehaviour
    {
        SpriteRenderer sr => GetComponent<SpriteRenderer>();
        RectTransform rt => GetComponent<RectTransform>();

        void Resize()
        {
            float w = rt.rect.width;
            float h = rt.rect.height;

            var sprite = sr.sprite;
            if (sprite != null)
            {
                var pw = sprite.rect.width / sprite.pixelsPerUnit;
                var ph = sprite.rect.height / sprite.pixelsPerUnit;

                float factor = 1;

                if (w / h > pw / ph)
                {
                    factor = w / pw;
                }
                else
                {
                    factor = h / ph;
                }

                transform.localScale = new Vector3(factor, factor, factor);
            }
        }

        Sprite oldSprite;

        void Update()
        {
            if (oldSprite != sr?.sprite)
            {
                oldSprite = sr?.sprite;
                Resize();
            }
        }
    }
}
