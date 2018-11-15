using UnityEngine;
using System;

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

        Action callback;
        bool reverse;

        float startTime = -1;
        public void Play(bool reverse = false, Action finishCallback = null)
        {
            startTime = Time.time;
            sprite.enabled = true;

            callback = finishCallback;
            this.reverse = reverse;
        }


        private void Update()
        {
            if (startTime >= 0)
            {
                var val = (Time.time - startTime) / duration;
                sprite.material.SetFloat("_Val", reverse ? 1 - val : val);

                if (val >= 1)
                {
                    startTime = -1;
                    callback?.Invoke();
                }
            }
        }
    }
}
