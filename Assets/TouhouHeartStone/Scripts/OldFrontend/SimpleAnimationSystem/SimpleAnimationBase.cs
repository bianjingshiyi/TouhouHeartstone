using System;
using UnityEngine;

namespace TouhouHeartstone.OldFrontend.SimpleAnimationSystem
{
    /// <summary>
    /// 简易的动画
    /// </summary>
    [Serializable]
    public abstract class SimpleAnimationBase : MonoBehaviour
    {
        /// <summary>
        /// 动画目标
        /// </summary>
        protected SimpleAnimator target;

        /// <summary>
        /// 设置动画目标
        /// </summary>
        /// <param name="animator"></param>
        public void SetTarget(SimpleAnimator animator)
        {
            target = animator;
        }

        /// <summary>
        /// 是否播放完毕
        /// </summary>
        public abstract bool isFinish { get; }

        /// <summary>
        /// 设置开始时间
        /// </summary>
        /// <param name="time"></param>
        public abstract void SetStartTime(float time);

        /// <summary>
        /// 更新动画状态
        /// </summary>
        /// <param name="time"></param>
        public abstract void UpdateAnimation(float time);
    }
}