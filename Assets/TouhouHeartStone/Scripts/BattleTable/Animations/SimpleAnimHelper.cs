using System;
using System.Collections.Generic;
using UI;
using UnityEngine;
namespace Game.BattleTable
{
    static class SimpleAnimHelper
    {
        public static bool update(BattleTableManager table, ref AnimAnim anim, SimpleAnim simpleAnim, Animator animator, float time, Func<IEnumerable<TableAnimation>, bool> onBlockAnim = null)
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
        public static bool update(BattleTableManager table, ref TableAnimation anim, SimpleAnim simpleAnim, Animator animator, float time, Func<IEnumerable<TableAnimation>, bool> onBlockAnim = null)
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
