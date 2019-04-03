using System;

using UnityEngine;

namespace TouhouHeartstone.Frontend.View.Animation
{
    /// <summary>
    /// 卡片动画
    /// </summary>
    public abstract class CardAnimation : ICardAnimation
    {
        GameObject card;

        public GameObject Card => card;

        public virtual void SetGameObject(GameObject card)
        {
            this.card = card;
        }

        public abstract string AnimationName { get; }

        /// <summary>
        /// 播放对应动画
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="args"></param>
        /// <param name="callback"></param>
        public virtual void PlayAnimation(object sender, EventArgs args, GenericAction callback)
        {
            PlayAnimation(sender, args as CardAnimationEventArgs, callback);
        }

        /// <summary>
        /// 播放对应动画
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="args"></param>
        /// <param name="callback"></param>
        public abstract void PlayAnimation(object sender, CardAnimationEventArgs args, GenericAction callback);
    }
 }
