using BJSYGameCore;
using UnityEngine;

namespace Game.BattleTable
{
    public class TableSEPlayer : MonoBehaviour
    {
        #region 公有方法
        public void Play()
        {
            if (clip)
                PlaySEClip(clip);
            else if (!string.IsNullOrEmpty(se_name))
                PlaySE(se_name);
        }
        public void PlaySE(string name)
        {
            if (manager != null && manager.sounds != null)
                manager.sounds.PlaySE(name, _pitch);
        }
        public void PlaySEClip(AudioClip clip)
        {
            PlaySEClip(clip, null, false, 0);
        }
        public void PlaySEClip(AudioClip clip, Transform parent = null, bool loop = false, float duration = 0)
        {
            if (manager != null && manager.sounds != null)
                manager.sounds.playSE(clip, parent: parent, pitch: _pitch, loop: loop, duration: duration);
            else
            {
                AudioSource audioSource = new GameObject(nameof(AudioSource)).AddComponent<AudioSource>();
                if (parent != null)
                    audioSource.transform.parent = parent;
                audioSource.clip = clip;
                audioSource.loop = loop;
                audioSource.Play();
                Destroy(audioSource.gameObject, duration > 0 ? duration : clip.length);
            }
        }
        public void PlaySEClipLoop(AudioClip clip)
        {
            if (manager != null && manager.sounds != null)
                manager.sounds.playSE(clip, null, _pitch, true);
        }
        public void stopSEClip(AudioClip clip)
        {
            if (manager != null && manager.sounds != null)
                manager.sounds.stopSE(clip);
        }
        public void fadeoutSEClip(AudioClip clip)
        {
            if (manager != null && manager.sounds != null)
                manager.sounds.fadeoutSE(clip, _fadeoutTime);
        }
        #endregion
        #region 属性字段
        public BattleTableManager manager
        {
            get
            {
                if (!_manager)
                {
                    _manager = this.findInstance<BattleTableManager>();
                }
                return _manager;
            }
        }
        [SerializeField]
        private BattleTableManager _manager;
        [SerializeField]
        string se_name;
        [SerializeField]
        AudioClip clip;
        [SerializeField]
        float _fadeoutTime = 1;
        [SerializeField]
        float _pitch = 1;
        #endregion
    }
}
