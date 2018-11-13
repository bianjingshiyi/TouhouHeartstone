using UnityEngine;

namespace TouhouHeartstone.Frontend
{
    public class CardUseEffect : MonoBehaviour
    {
        [SerializeField]
        float duration;

        [SerializeField]
        SpriteRenderer sprite;

        private void Awake()
        {
            sprite = sprite ?? GetComponent<SpriteRenderer>();
        }

        float startTime = -1;
        public void Play()
        {
            startTime = Time.time;
        }

        private void Update()
        {
            if(startTime >= 0)
            {
                var val = (Time.time - startTime) / duration;
                sprite.material.SetFloat("_Val", val);

                if (val >= 1) startTime = -1;
            }
        }
    }
}
