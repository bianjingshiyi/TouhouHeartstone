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
            this.findInstance<SEManager>().PlaySE(se_name);
        }
    }
}
