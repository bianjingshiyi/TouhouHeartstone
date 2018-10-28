using UnityEngine;
using System;

using TouhouHeartstone.Frontend.SimpleAnimationSystem;

namespace TouhouHeartstone.Frontend
{
    /// <summary>
    /// 卡片动画控制器
    /// </summary>
    public class CardAnimationController : MonoBehaviour
    {
        [SerializeField]
        PredefineableAnimator animator;

        [SerializeField]
        Vector3 defaultDrawCardTarget;

        /// <summary>
        /// 抽卡动画
        /// </summary>
        /// <param name="targetPosition"></param>
        public void DrawCard(Vector3 targetPosition, Action finishCallback = null)
        {
            var ani = animator.DupAnimation("drawCard");
            if (ani is KeyframeAnimation)
            {
                var kfani = ani as KeyframeAnimation;
                kfani.SetEndPosition(targetPosition);
                kfani.OnAnimationFinish += finishCallback;
            }
            animator.InstantPlay(ani);
        }

        /// <summary>
        /// 播放抽卡动画
        /// </summary>
        public void DrawCard(Action finishCallback = null)
        {
            DrawCard(defaultDrawCardTarget, finishCallback);
        }

        public void CardMoveToHand(Vector3 position, Vector3 rotation , Action finishCallback = null)
        {
            var ani = animator.DupAnimation("cardToHand");
            if (ani is KeyframeAnimation)
            {
                var kfani = ani as KeyframeAnimation;
                kfani.SetEndPosition(position);
                kfani.SetEndRotation(rotation);

                kfani.OnAnimationFinish += finishCallback;
            }

            animator.QueuePlay(ani);
        }
    }
}
