using System;
using System.Collections.Generic;

using UnityEngine;

namespace TouhouHeartstone.Frontend.View.Animation
{
    /// <summary>
    /// 卡片的View
    /// </summary>
    public class CardView : MonoBehaviour
    {
        #region animation_base
        Dictionary<string, ICardAnimation> cardAnimations = new Dictionary<string, ICardAnimation>();

        /// <summary>
        /// 播放卡片动画
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="args"></param>
        /// <param name="callback"></param>
        public void PlayAnimation(object sender, EventArgs args, GenericAction callback)
        {
            if (!(args is CardAnimationEventArgs))
                throw new WrongArumentTypeException(typeof(CardAnimationEventArgs), args.GetType());

            CardAnimationEventArgs aniArgs = args as CardAnimationEventArgs;
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
            else
            {
                // 在库里面的普通类
                var ca = CardAnimationDynamicLibrary.CreateAnimation(aniArgs.AnimationName);
                ca.SetGameObject(gameObject);
                ani = ca;
            }

            ani.PlayAnimation(sender, aniArgs.EventArgs, callback);
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

        protected void Awake()
        {
            reloadAnimationList();
        }

        protected void Start()
        {
            // test: something
            PlayAnimation(this, new CardAnimationEventArgs() { AnimationName = "Disappear", EventArgs = new EventArgs() }, null);
        }
    }
}
