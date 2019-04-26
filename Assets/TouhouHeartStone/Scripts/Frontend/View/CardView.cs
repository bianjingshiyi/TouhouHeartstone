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
    public class CardView : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerDownHandler, IPointerUpHandler
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

            checker.OnClick += onMouseClick;
            checker.OnDrag += onMouseDrag;
            checker.OnRelease += onMouseRelease;
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
            set
            {
                if (currentState != value)
                {
                    onStateChange(currentState, value);
                    currentState = value;
                }
            }
        }

        void onStateChange(state original, state current)
        {

        }

        DragInputChecker checker = new DragInputChecker();

        protected void Update()
        {
            checker.Update(Time.time);
        }

        void onMouseDrag()
        {
            DebugUtils.Trace("鼠标按住");
            var deck = GetComponentInParent<Controller.UserDeckController>();

            switch (CurrentState)
            {
                case state.hand_hover:
                    // check,
                    if (!deck.ThrowingCard)
                    {
                        // 进入跟随模式
                        CurrentState = state.free;
                    }
                    break;
                case state.center:
                    // xxx
                    break;
            }
        }

        void onMouseRelease()
        {
            DebugUtils.Trace("鼠标松开");
            switch (CurrentState)
            {
                case state.hand_hover:
                    // check,
                    break;
                case state.center:
                    // xxx
                    break;
            }
        }

        public void OnPointerDown(PointerEventData eventData)
        {
            checker.PointerDown(Time.time);
        }

        public void OnPointerUp(PointerEventData eventData)
        {
            checker.PointerUp();
        }

        private void onMouseClick()
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

    class DragInputChecker
    {
        public event Action OnDrag;
        public event Action OnRelease;
        public event Action OnClick;

        /// <summary>
        /// 上次按下时间。若为空则意味着触发或OnDrag事件了
        /// </summary>
        float? lastPointerDownTime;
        const float dragThresold = 0.25f;

        public void Update(float time)
        {
            if (lastPointerDownTime != null)
            {
                if (time - lastPointerDownTime > dragThresold)
                {
                    OnDrag?.Invoke();
                    lastPointerDownTime = null;
                }
            }
        }

        public void PointerDown(float time)
        {
            lastPointerDownTime = time;
        }

        public void PointerUp()
        {
            if (lastPointerDownTime != null)
            {
                OnClick?.Invoke();
                lastPointerDownTime = null;
            }
            else
            {
                OnRelease?.Invoke();
            }
        }
    }
}
