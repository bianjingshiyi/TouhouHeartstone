using UnityEngine;
using System;


namespace TouhouHeartstone.OldFrontend.SimpleAnimationSystem
{
    /// <summary>
    /// 包括预定义动画的Animator
    /// </summary>
    public class PredefineableAnimator : SimpleAnimator
    {
        [Serializable]
        public struct AnimationWithName
        {
            public SimpleAnimationBase animation;
            public string Name;
        }

        [SerializeField]
        AnimationWithName[] predifinedAnimations;

        public AnimationWithName[] PredefinedAnimations => predifinedAnimations;

        /// <summary>
        /// 根据名称查找并复制动画
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public SimpleAnimationBase DupAnimation(string name)
        {
            foreach (var item in predifinedAnimations)
            {
                if (item.Name == name)
                {
                    var cp = Instantiate(item.animation, this.transform);
                    return cp;
                }
            }
            return null;
        }

        /// <summary>
        /// 队列播放指定名称的动画
        /// </summary>
        /// <param name="name"></param>
        public void QueuePlay(string name)
        {
            var ani = DupAnimation(name);
            if (ani != null) QueuePlay(ani);
        }

        /// <summary>
        /// 立即播放指定名称的动画
        /// </summary>
        /// <param name="name"></param>
        public void InstantPlay(string name)
        {
            var ani = DupAnimation(name);
            if (ani != null) InstantPlay(ani);
        }
    }
}