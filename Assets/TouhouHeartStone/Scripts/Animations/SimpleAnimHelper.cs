using UI;
using UnityEngine;
namespace Game
{
    static class SimpleAnimHelper
    {
        public static bool update(TableManager table, ref AnimAnim anim, SimpleAnim simpleAnim, Animator animator)
        {
            simpleAnim.beforeAnim.Invoke();
            if (!string.IsNullOrEmpty(simpleAnim.animName))
            {
                if (anim == null)
                    anim = new AnimAnim(animator, simpleAnim.animName);
                if (!anim.update(table))
                    return false;
            }
            simpleAnim.afterAnim.Invoke();
            return true;
        }
    }
}
