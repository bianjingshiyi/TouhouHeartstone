using UI;
using UnityEngine;
using System;
namespace Game.BattleTable
{
    static class SimpleAnimHelper
    {
        public static bool update(BattleTableManager table, ref AnimAnim anim, SimpleAnim simpleAnim, Animator animator, float time, Func<UIAnimation, bool> onBlockAnim = null)
        {
            if (simpleAnim == null)
                return true;
            if (anim == null)
            {
                simpleAnim.invokeBeforeAnim();
                anim = new AnimAnim(animator, simpleAnim.animName, onBlockAnim);
            }
            if (!anim.update(table, time))
                return false;
            simpleAnim.invokeAfterAnim();
            anim = null;
            return true;
        }
        public static bool update(BattleTableManager table, ref TableAnimation anim, SimpleAnim simpleAnim, Animator animator, float time, Func<UIAnimation, bool> onBlockAnim = null)
        {
            if (simpleAnim == null)
                return true;
            if (anim == null)
            {
                simpleAnim.invokeBeforeAnim();
                anim = new AnimAnim(animator, simpleAnim.animName, onBlockAnim);
            }
            if (!anim.update(table, time))
                return false;
            simpleAnim.invokeAfterAnim();
            anim = null;
            return true;
        }
    }
}
