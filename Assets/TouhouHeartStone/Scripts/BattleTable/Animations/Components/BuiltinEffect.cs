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
            anim.isPersistent = isPersistent;
            var comp = GetComponent<TableEffect>();
            anim.prefab = comp;
            switch (comp)
            {
                case AreaEffect:
                    anim.type = AnimationType.Area;
                    break;
                case CardEffect:
                    anim.type = AnimationType.Card;
                    break;
                case VectorEffect:
                    anim.type = AnimationType.Vector;
                    break;
                case ProjectileEffect:
                    anim.type = AnimationType.Projectile;
                    break;
            }
            return anim;
        }
        public string defineName;
        public float duration;
        public float blockTime;
        public bool isPersistent;
    }
}
