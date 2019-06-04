using UnityEngine;
using UnityEngine.UI;

namespace IGensoukyo.Utilities
{
    [RequireComponent(typeof(Image))]
    [RequireComponent(typeof(RectTransform))]
    [ExecuteInEditMode]
    public class ImageAutoScale : MonoBehaviour
    {
        [SerializeField]
        Vector2 CurrentSize = new Vector2(100, 100);

        Vector2 rtSize
        {
            get
            {
                return rt.rect.size;
            }
            set
            {
                var oldSize = rtSize;
                var offset = (value - oldSize) / 2;
                rt.offsetMin -= offset;
                rt.offsetMax += offset;
            }
        }

        Image image => GetComponent<Image>();
        RectTransform rt => GetComponent<RectTransform>();

        Sprite oldSprite;

        void Resize()
        {
            var sprite = image.sprite;
            if (sprite != null)
            {
                float wh1 = sprite.rect.width / sprite.rect.height;
                float wh2 = CurrentSize.x / CurrentSize.y;

                var realSize = CurrentSize;
                if (wh1 > wh2)
                {
                    realSize.x = realSize.x * (wh1 / wh2);
                }
                else
                {
                    realSize.y = realSize.y * (wh2 / wh1);
                }
                rtSize = realSize;
            }
            else
            {
                rtSize = CurrentSize;
            }
        }

        private void Update()
        {
            if (oldSprite != image?.sprite)
            {
                oldSprite = image?.sprite;
                Resize();
            }
        }
    }
}
