using UnityEngine;
using BJSYGameCore;

namespace Game.BattleTable
{
    public class TableSEPlayer : MonoBehaviour
    {
        public void Play()
        {
            PlaySE(se_name);
        }
        public void PlaySE(string name)
        {
            manager?.sounds?.PlaySE(name);
        }
        public void PlaySE(AudioClip clip)
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
    }
}
