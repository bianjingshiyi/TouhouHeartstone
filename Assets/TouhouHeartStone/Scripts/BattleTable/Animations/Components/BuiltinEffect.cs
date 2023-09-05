using UnityEngine;

namespace Game.Animations
{
    public class BuiltinEffect : MonoBehaviour
    {
        public UnityAnimationDefine toAnimationDefine()
        {
            var anim = new UnityAnimationDefine(0, 0, defineName);
            anim.duration = duration;
            anim.blockTime = blockTime;
            anim.prefab = GetComponent<TableEffect>();
            return anim;
        }
        public string defineName;
        public float duration;
        public float blockTime;
    }
}
