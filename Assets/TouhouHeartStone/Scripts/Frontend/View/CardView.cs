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
            if (CurrentState == state.hand)
                CurrentState = state.hand_hover;
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            GetComponent<CardHighlight>()?.SetHighlight(false);
            if (CurrentState == state.hand_hover)
                CurrentState = state.hand;
        }

        state currentState = state.hand;
        public state CurrentState
        {
            get { return currentState; }
            set { currentState = value; }
        }

        public void OnPointerDown(PointerEventData eventData)
        {
            DebugUtils.Trace("鼠标按下");
            switch(CurrentState) {
                case state.hand_hover:
                    // check,
                    break;
                case state.center:
                    // xxx
                    break;
            }
        }

        public void OnPointerUp(PointerEventData eventData)
        {
            DebugUtils.Trace("鼠标松开");
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            DebugUtils.Trace("鼠标按下");
            var deck = GetComponentInParent<Controller.UserDeckController>();
            switch (CurrentState)
            {
                case state.hand_hover:
                    // check,
                    if (deck.ThrowingCard)
                    {
                        deck.PrepareThrowCard(this.GetComponent<CardFaceViewModel>(), true);
                        CurrentState = state.center;
                    }
                    break;
                case state.center:
                    if (deck.ThrowingCard)
                    {
                        deck.PrepareThrowCard(this.GetComponent<CardFaceViewModel>(), false);
                        CurrentState = state.hand_hover;
                    }
                    break;
            }
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
