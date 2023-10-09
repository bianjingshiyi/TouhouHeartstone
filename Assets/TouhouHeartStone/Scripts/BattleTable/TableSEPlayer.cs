using BJSYGameCore;
using UnityEngine;

namespace Game.BattleTable
{
    public class TableSEPlayer : MonoBehaviour
    {
        private void OnEnable()
        {
            if (playOnEnable)
            {
                Play();
            }
        }
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
    }
}
