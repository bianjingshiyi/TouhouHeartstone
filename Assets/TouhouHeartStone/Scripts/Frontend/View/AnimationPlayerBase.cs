using System;
using System.Collections.Generic;

using UnityEngine;
using TouhouHeartstone.Frontend.View.Animation;

namespace TouhouHeartstone.Frontend.View
{
    public class AnimationPlayerBase : MonoBehaviour
    {
        #region animation_base
        Dictionary<string, ICardAnimation> cardAnimations = new Dictionary<string, ICardAnimation>();

        /// <summary>
        /// 播放卡片动画
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="args"></param>
        /// <param name="callback"></param>
        public void PlayAnimation(object sender, EventArgs args, GenericAction callback = null)
        {
            CardAnimationEventArgs aniArgs = Utilities.CheckType<CardAnimationEventArgs>(args);
            ICardAnimation ani;

            if (cardAnimations.ContainsKey(aniArgs.AnimationName))
            {
                // 在卡片本体上的
                ani = cardAnimations[aniArgs.AnimationName];
            }
            else if (CardAnimationComponentLibrary.AnimationExists(aniArgs.AnimationName))
            {
                // 在库里面可以实例化调用的
                var c = CardAnimationComponentLibrary.CreateAnimation(aniArgs.AnimationName, gameObject);
                ani = c;

                cardAnimations.Add(aniArgs.AnimationName, c);
            }
            else if (CardAnimationDynamicLibrary.ContainsAnimation(aniArgs.AnimationName))
            {
                // 在库里面的普通类
                var ca = CardAnimationDynamicLibrary.CreateAnimation(aniArgs.AnimationName);
                ca.SetGameObject(gameObject);
                ani = ca;
            }
            else
            {
                ani = null;
                Debug.LogError($"没有找到动画: {aniArgs.AnimationName}");
            }

            ani.PlayAnimation(sender, aniArgs.EventArgs, callback);
        }

        /// <summary>
        /// 播放动画
        /// </summary>
        /// <param name="sender">发送者</param>
        /// <param name="aniName">动画名称</param>
        /// <param name="args">动画参数</param>
        /// <param name="callback">回调</param>
        public void PlayAnimation(object sender, string aniName, EventArgs args = null, GenericAction callback = null)
        {
            PlayAnimation(sender, new CardAnimationEventArgs(aniName, args));
        }

        /// <summary>
        /// 播放动画
        /// </summary>
        /// <param name="aniName">动画名称</param>
        /// <param name="args">参数</param>
        /// <param name="callback">回调</param>
        public void PlayAnimation(string aniName, EventArgs args = null, GenericAction callback = null)
        {
            PlayAnimation(this, aniName, args, callback);
        }


        /// <summary>
        /// 重载动画组件列表
        /// </summary>
        void reloadAnimationList()
        {
            cardAnimations.Clear();

            var cards = gameObject.GetComponents<ICardAnimation>();
            foreach (var item in cards)
            {
                cardAnimations.Add(item.AnimationName, item);
            }
        }
        #endregion
    }
}
