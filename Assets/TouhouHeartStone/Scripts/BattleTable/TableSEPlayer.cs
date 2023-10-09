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
            else
                PlaySE(se_name);
        }
        public void PlaySE(string name)
        {
            manager?.sounds?.PlaySE(name);
        }
        public void PlaySEClip(AudioClip clip)
        {
            manager?.sounds?.playSE(clip);
        }
        public void PlaySEClipLoop(AudioClip clip)
        {
            manager?.sounds?.playSE(clip, true);
        }
        public void stopSEClip(AudioClip clip)
        {
            manager?.sounds?.stopSE(clip);
        }
        public void fadeoutSEClip(AudioClip clip)
        {
            manager?.sounds?.fadeoutSE(clip, _fadeoutTime);
        }
        #endregion
        #region 私有方法
        private void OnEnable()
        {
            if (playOnEnable)
            {
                Play();
            }
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
        bool playOnEnable;
        [SerializeField]
        float _fadeoutTime = 1;
        #endregion
    }
}
