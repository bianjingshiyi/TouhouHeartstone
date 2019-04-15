using System;
using System.Collections.Generic;

using UnityEngine;

using TouhouHeartstone.Frontend.ViewModel;
using TouhouHeartstone.Frontend.View.Animation;

using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using IGensoukyo.Utilities;

namespace TouhouHeartstone.Frontend.View
{
    /// <summary>
    /// 卡片的View
    /// </summary>
    public class CardView : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerDownHandler, IPointerUpHandler, IPointerClickHandler
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
            else
            {
                // 在库里面的普通类
                var ca = CardAnimationDynamicLibrary.CreateAnimation(aniArgs.AnimationName);
                ca.SetGameObject(gameObject);
                ani = ca;
            }

            ani.PlayAnimation(sender, aniArgs.EventArgs, callback);
            Debug.Log($"播放动画：{aniArgs.AnimationName}");
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

        CardFaceViewModel cardVM;

        protected void Awake()
        {
            // 注册VM事件
            cardVM = GetComponent<CardFaceViewModel>();
            if (cardVM == null)
                throw new Exception("关联的ViewModel未找到");

            cardVM.OnAnimationPlay += PlayAnimation;
        }
        #region Card_state_event
        public void OnPointerEnter(PointerEventData eventData)
        {
            GetComponent<CardHighlight>()?.SetHighlight(true);
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            GetComponent<CardHighlight>()?.SetHighlight(false);
        }

        public void OnPointerDown(PointerEventData eventData)
        {
            DebugUtils.Log("鼠标按下");
        }

        public void OnPointerUp(PointerEventData eventData)
        {
            DebugUtils.Log("鼠标松开");
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            DebugUtils.Log("鼠标点击");
        }
        #endregion

        public enum state
        {
            hand,
            hand_hover,
            free,
            center
        }
    }
}
