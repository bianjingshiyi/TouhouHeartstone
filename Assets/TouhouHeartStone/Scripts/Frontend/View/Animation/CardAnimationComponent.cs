using System;

using UnityEngine;

namespace TouhouHeartstone.Frontend.View.Animation
{
    /// <summary>
    /// 以组件形式存在的卡片动画
    /// </summary>
    public abstract class CardAnimationComponent : MonoBehaviour, ICardAnimation
    {
        public abstract string AnimationName { get; }

        public virtual GameObject Card => gameObject;

        /// <summary>
        /// 播放对应动画
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="args"></param>
        /// <param name="callback"></param>
        public abstract void PlayAnimation(object sender, EventArgs args, GenericAction callback);
    }
}
