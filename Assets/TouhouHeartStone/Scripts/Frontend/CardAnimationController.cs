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
            PlayAnimation("drawCard", targetPosition, true, finishCallback);
        }

        /// <summary>
        /// 播放抽卡动画
        /// </summary>
        public void DrawCard(Action finishCallback = null)
        {
            DrawCard(defaultDrawCardTarget, finishCallback);
        }

        public void PlayAnimation(string name, bool instant = false, Action finishCallback = null)
        {
            var ani = animator.DupAnimation(name);
            if (ani is KeyframeAnimation)
            {
                var kfani = ani as KeyframeAnimation;
                kfani.OnAnimationFinish += finishCallback;
            }

            if (instant) animator.InstantPlay(ani);
            else animator.QueuePlay(ani);
        }

        public void PlayAnimation(string name, Vector3 position, bool instant = false, Action finishCallback = null)
        {
            var ani = animator.DupAnimation(name);
            if (ani is KeyframeAnimation)
            {
                var kfani = ani as KeyframeAnimation;
                kfani.SetEndPosition(position);
                kfani.OnAnimationFinish += finishCallback;
            }
            if (instant) animator.InstantPlay(ani);
            else animator.QueuePlay(ani);
        }

        public void PlayAnimation(string name, Vector3 position, Vector3 rotation, bool instant = false, Action finishCallback = null)
        {
            var ani = animator.DupAnimation(name);
            if (ani is KeyframeAnimation)
            {
                var kfani = ani as KeyframeAnimation;
                kfani.SetEndPosition(position);
                kfani.SetEndRotation(rotation);
                kfani.OnAnimationFinish += finishCallback;
            }
            if (instant) animator.InstantPlay(ani);
            else animator.QueuePlay(ani);
        }

        /// <summary>
        /// 移动到手牌的动画
        /// </summary>
        /// <param name="position"></param>
        /// <param name="rotation"></param>
        /// <param name="finishCallback"></param>
        public void CardMoveToHand(Vector3 position, Vector3 rotation, Action finishCallback = null)
        {
            PlayAnimation("cardToHand", position, rotation, false, finishCallback);
        }

        /// <summary>
        /// 微调位置的动画
        /// </summary>
        /// <param name="position"></param>
        /// <param name="rotation"></param>
        /// <param name="finishCallback"></param>
        public void TweakPosition(Vector3 position, Vector3 rotation, Action finishCallback = null)
        {
            PlayAnimation("tweakPos", position, rotation, false, finishCallback);
        }

        /// <summary>
        /// 显示卡片详细信息的动画
        /// </summary>
        /// <param name="position"></param>
        /// <param name="rotation"></param>
        /// <param name="finishCallback"></param>
        public void ShowCard(Vector3 position, Action finishCallback = null)
        {
            var ani = animator.DupAnimation("showCard");
            if (ani is KeyframeAnimation)
            {
                var kfani = ani as KeyframeAnimation;
                for (int i = 0; i < kfani.keyframes.Length; i++)
                {
                    kfani.keyframes[i].position.x = position.x;
                }
                kfani.OnAnimationFinish += finishCallback;
            }

            animator.InstantPlay(ani);
        }

        /// <summary>
        /// 卡片详细信息回到手牌的动画
        /// </summary>
        /// <param name="position"></param>
        /// <param name="rotation"></param>
        /// <param name="finishCallback"></param>
        public void UnShowCard(Vector3 position, Vector3 rotation, Action finishCallback = null)
        {
            var ani = animator.DupAnimation("unshowCard");
            if (ani is KeyframeAnimation)
            {
                var kfani = ani as KeyframeAnimation;
                for (int i = 0; i < kfani.keyframes.Length; i++)
                {
                    kfani.keyframes[i].position.x = position.x;
                    kfani.keyframes[i].position.z = position.z;
                    kfani.keyframes[i].rotation = rotation;
                }

                kfani.keyframes[0].position.y = position.y + 0.1f;
                kfani.SetEndPosition(position);

                kfani.OnAnimationFinish += finishCallback;
            }

            animator.InstantPlay(ani);
        }
    }
}
