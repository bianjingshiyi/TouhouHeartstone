
using UnityEngine;

namespace TouhouHeartstone.Frontend
{
    public class UIStone : MonoBehaviour
    {
        [SerializeField]
        Sprite normal;

        [SerializeField]
        Sprite highlight;

        [SerializeField]
        SpriteRenderer spriteRenderer;

        protected void Awake()
        {
            spriteRenderer = spriteRenderer ?? GetComponent<SpriteRenderer>();
        }

        public void SetLight(bool state)
        {
            if (spriteRenderer == null)
            {
                spriteRenderer = spriteRenderer ?? GetComponent<SpriteRenderer>();
            }
            spriteRenderer.sprite = state ? highlight : normal;
        }
    }
}
