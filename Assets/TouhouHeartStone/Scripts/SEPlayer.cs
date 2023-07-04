using UnityEngine;
using BJSYGameCore;

namespace Game
{
    public class SEPlayer : MonoBehaviour
    {
        [SerializeField]
        string se_name;
        public void Play()
        {
            PlaySE(se_name);
        }
        public void PlaySE(string name)
        {
            this.findInstance<SEManager>()?.PlaySE(name);
        }
        public void PlaySE(AudioClip clip)
        {
            this.findInstance<SEManager>()?.playSE(clip);
        }
    }
}
