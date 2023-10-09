using System;
using UnityEngine;

namespace Game
{
    [RequireComponent(typeof(AudioSource))]
    public class SESource : MonoBehaviour
    {
        #region 公有方法
        public void play()
        {
            isStarted = true;
            source.Play();
        }
        public void fadeOut(float time)
        {
            isFadingOut = true;
            fadingTargetTime = time;
        }
        public void remove()
        {
            Destroy(gameObject);
        }
        #endregion
        #region 私有方法
        private void Update()
        {
            if (isStarted)
            {
                if (!source.isPlaying)
                {
                    finish();
                }
                else if (isFadingOut)
                {
                    fadingTime += Time.deltaTime;
                    source.volume = 1 - Mathf.Clamp01(fadingTime / fadingTargetTime);
                    if (fadingTime >= fadingTargetTime)
                    {
                        finish();
                    }
                }
            }
        }
        private void finish()
        {
            onFinished?.Invoke(this);
        }
        #endregion
        #region 事件
        public event Action<SESource> onFinished;
        #endregion
        #region 属性字段
        public AudioClip clip
        {
            get => source.clip;
            set => source.clip = value;
        }
        public bool loop
        {
            get => source.loop;
            set => source.loop = value;
        }
        private AudioSource source
        {
            get
            {
                if (!_source)
                {
                    _source = GetComponent<AudioSource>();
                }
                return _source;
            }
        }
        private bool isStarted;
        private bool isFadingOut;
        private float fadingTime;
        private float fadingTargetTime;
        private AudioSource _source;
        #endregion
    }
}
