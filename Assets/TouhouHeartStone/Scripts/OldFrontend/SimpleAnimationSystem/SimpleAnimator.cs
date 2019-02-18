using UnityEngine;
using System;
using System.Collections.Generic;

namespace TouhouHeartstone.OldFrontend.SimpleAnimationSystem
{
    /// <summary>
    /// 简易的Animator实现
    /// </summary>
    public class SimpleAnimator : MonoBehaviour
    {
        /// <summary>
        /// 动画存储
        /// </summary>
        protected Queue<SimpleAnimationBase> animations = new Queue<SimpleAnimationBase>();

        /// <summary>
        /// 最后一个动画播放完毕后的事件
        /// </summary>
        public event Action OnQueueAnimationFinish;

        protected void Update()
        {
            if (animations.Count > 0)
            {
                var ani = animations.Peek();
                ani.UpdateAnimation(Time.time);

                if (ani.isFinish)
                {
                    animations.Dequeue();
                    if (animations.Count > 0)
                    {
                        animations.Peek().SetStartTime(Time.time);
                    }
                    else
                    {
                        OnQueueAnimationFinish?.Invoke();
                        OnQueueAnimationFinish = null;
                    }
                }
            }
        }

        /// <summary>
        /// 队列播放
        /// </summary>
        /// <param name="ani"></param>
        /// <param name="onAnimationFinishCallback">当这条动画播放完毕后的回调</param>
        public void QueuePlay(SimpleAnimationBase ani, Action onAnimationFinishCallback = null)
        {
            ani.SetTarget(this);
            if (animations.Count == 0) ani.SetStartTime(Time.time);
            animations.Enqueue(ani);
            OnQueueAnimationFinish += onAnimationFinishCallback;
        }

        /// <summary>
        /// 立即播放
        /// </summary>
        /// <param name="ani"></param>
        public void InstantPlay(SimpleAnimationBase ani)
        {
            animations.Clear();
            QueuePlay(ani);
        }

        public void Clear()
        {
            animations.Clear();
        }
    }
}