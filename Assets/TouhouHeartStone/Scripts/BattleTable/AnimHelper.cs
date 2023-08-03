using System.Collections.Generic;
using UnityEngine;

namespace Game.BattleTable
{
    public class AnimHelper
    {
        public static bool hasAnim(Animator animator, string animName, out int layer)
        {
            for (int i = 0; i < animator.layerCount; i++)
            {
                if (animator.HasState(i, Animator.StringToHash(animName)))
                {
                    layer = i;
                    return true;
                }
            }
            layer = -1;
            return false;
        }
        /// <summary>
        /// 结果为true时播放结束。
        /// </summary>
        /// <param name="animator"></param>
        /// <param name="animName"></param>
        /// <param name="isPlayed"></param>
        /// <param name="isFinished"></param>
        /// <returns></returns>
        public static bool update(Animator animator, string animName, ref bool isPlayed, ref bool isFinished)
        {
            if (string.IsNullOrEmpty(animName))
                return true;
            if (animator == null)
                return true;
            if (!animator.isActiveAndEnabled)
            {
                UberDebug.LogErrorChannel(animator, "Animation", animator + "被禁用，无法播放动画" + animName);
            }
            if (!animator.gameObject.activeInHierarchy)
            {
                UberDebug.LogErrorChannel(animator, "Animation", animator + "的GameObject被禁用，无法播放动画" + animName);
                return true;
            }
            if (isFinished)
                return true;
            if (!hasAnim(animator, animName, out var layer))
            {
                UberDebug.LogErrorChannel(animator, "Animation", animator + "中不存在动画状态" + animName);
                return true;
            }
            if (!isPlayed)
            {
                isPlayed = true;
                animator.Play(animName, layer, 0);
                return false;
            }
            for (int i = 0; i < animator.layerCount; i++)
            {
                AnimatorStateInfo state = animator.GetCurrentAnimatorStateInfo(i);
                if (state.IsName(animName) && state.normalizedTime < 1)
                    return false;
            }
            isFinished = true;
            return true;
        }
        public static bool isAnyAnimUpdating(BattleTableManager table, float time, params TableAnimation[] anims)
        {
            return isAnyAnimUpdating(anims, table, time);
        }
        public static bool isAnyAnimUpdating(IEnumerable<TableAnimation> anims, BattleTableManager table, float time)
        {
            bool isAny = false;
            foreach (var anim in anims)
            {
                if (anim == null)
                    continue;
                if (!anim.update(table, time))
                    isAny = true;
            }
            return isAny;
        }
    }
}
