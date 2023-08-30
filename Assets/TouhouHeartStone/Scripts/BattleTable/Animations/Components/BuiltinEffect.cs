using UnityEngine;

namespace Game.Animations
{
    public class BuiltinEffect : MonoBehaviour
    {
        public UnityAnimationDefine toAnimationDefine()
        {
            var anim = new UnityAnimationDefine(0, id, defineName);
            anim.duration = duration;
            anim.blockTime = blockTime;
            anim.prefab = GetComponent<TableEffect>();
            return anim;
        }
        public int id;
        public string defineName;
        public float duration;
        public float blockTime;
    }
}
