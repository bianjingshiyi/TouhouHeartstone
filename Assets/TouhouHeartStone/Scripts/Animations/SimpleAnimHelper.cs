using UI;
using UnityEngine;
using System;
namespace Game
{
    static class SimpleAnimHelper
    {
        public static bool update(TableManager table, ref AnimAnim anim, SimpleAnim simpleAnim, Animator animator, Func<UIAnimation, bool> onBlockAnim = null)
        {
            if (simpleAnim == null)
                return true;
            if (anim == null)
            {
                simpleAnim.beforeAnim.Invoke();
                anim = new AnimAnim(animator, simpleAnim.animName, onBlockAnim);
            }
            if (!anim.update(table))
                return false;
            simpleAnim.afterAnim.Invoke();
            anim = null;
            return true;
        }
    }
}
