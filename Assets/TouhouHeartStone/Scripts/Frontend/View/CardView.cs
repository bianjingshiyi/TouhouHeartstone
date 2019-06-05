using System;

using UnityEngine;

using TouhouHeartstone.Frontend.ViewModel;
using TouhouHeartstone.Frontend.View.Animation;

using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using IGensoukyo.Utilities;
using TouhouHeartstone.Frontend.Model;

namespace TouhouHeartstone.Frontend.View
{
    /// <summary>
    /// 卡片的View
    /// </summary>
    public class CardView : AnimationPlayerBase, IPointerEnterHandler, IPointerExitHandler, IPointerDownHandler, IPointerUpHandler
    {
        CardViewModel cardVM;

        RectTransform rectTransform => GetComponent<RectTransform>();

        protected void Awake()
        {
            // 注册VM事件
            cardVM = GetComponentInParent<CardViewModel>();
            if (cardVM == null)
                throw new Exception("关联的ViewModel未找到");

            cardVM.OnRecvActionEvent += onRecvAction;

            checker.OnClick += onMouseClick;
            checker.OnDrag += onMouseDrag;
            checker.OnRelease += onMouseRelease;
        }

        bool destroied = false;
        protected void OnDestroy()
        {
            if (!destroied)
            {
                cardVM.OnRecvActionEvent -= onRecvAction;
                destroied = true;
            }
        }

        void destroy()
        {
            Destroy(gameObject);
            OnDestroy();
        }

        private void onRecvAction(object sender, EventArgs args, GenericAction callback)
        {
            if (args is UseCardEventArgs)
            {
                // todo: 重载卡的位置
                onUse(args as UseCardEventArgs, callback);
            }
            if (args is IndexChangeEventArgs)
            {
                CardVM_OnIndexChangeEvent();
            }
            if (args is CardToStackEventArgs)
            {
                var arg = args as CardToStackEventArgs;
                PlayAnimation(this, new CardAnimationEventArgs()
                {
                    AnimationName = "CardToStack",
                    EventArgs = new CardPositionEventArgs(arg.Count, arg.Index)
                }, callback);
            }
        }

        private void onUse(UseCardEventArgs arg1, GenericAction arg2)
        {
            // todo: 使用的动画
            destroy();
            arg2?.Invoke(this, null);
        }

        bool drawed = false;

        private void CardVM_OnIndexChangeEvent()
        {
            if (drawed && this != null)
            {
                switch (CurrentState)
                {
                    case state.center:
                        // 调整中心位置
                        if (Deck.ThrowingCard)
                        {
                            PlayAnimation(this, new CardAnimationEventArgs()
                            {
                                AnimationName = "CardToCenter",
                                EventArgs = new CardPositionEventArgs(Deck.ThrowingCardCount, cardVM.Index)
                            }, null);
                        }
                        break;
                    case state.hand:
                        // 调整手牌位置
                        PlayAnimation(this, new CardAnimationEventArgs()
                        {
                            AnimationName = "CardToHand",
                            EventArgs = new CardPositionEventArgs(Deck.HandCardCount, cardVM.Index)
                        }, null);
                        break;
                }
            }
            else
            {
                // 首次抽卡动画
                PlayAnimation(this, new CardAnimationEventArgs()
                {
                    AnimationName = "DrawCard",
                    EventArgs = new CardPositionEventArgs(Deck.HandCardCount, cardVM.Index)
                }, (a, b) =>
                {
                    drawed = true;
                    cardVM.DoAction(new CardDrewEventArgs());
                });
            }
        }

        #region Card_state_event
        public void OnPointerEnter(PointerEventData eventData)
        {
            if (Deck.IsSelf && drawed)
            {
                GetComponent<CardHighlight>()?.SetHighlight(true);
                if (CurrentState == state.hand)
                    CurrentState = state.hand_hover;
            }
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            if (Deck.IsSelf && drawed)
            {
                GetComponent<CardHighlight>()?.SetHighlight(false);
                if (CurrentState == state.hand_hover)
                    CurrentState = state.hand;
            }
        }

        state currentState = state.hand;
        public state CurrentState
        {
            get { return currentState; }
            set
            {
                if (currentState != value)
                {
                    var last = currentState;
                    currentState = value;
                    onStateChange(last, value);
                }
            }
        }

        void onStateChange(state original, state current)
        {
            if (original == state.free)
            {
                mousePosOffset = null;
            }

            switch (current)
            {
                case state.hand_hover:
                    PlayAnimation(this, new CardAnimationEventArgs()
                    {
                        AnimationName = "CardToPreview",
                        EventArgs = new CardPositionEventArgs(Deck.HandCardCount, cardVM.Index)
                    }, null);
                    break;
                case state.hand:
                    if (!Deck.ThrowingCard || original == state.hand_hover) // 丢卡模式则等待丢卡发事件
                    {
                        PlayAnimation(this, new CardAnimationEventArgs()
                        {
                            AnimationName = "CardToHand",
                            EventArgs = new CardPositionEventArgs(Deck.HandCardCount, cardVM.Index)
                        }, null);
                    }
                    break;
                case state.free:
                    mousePosOffset = Input.mousePosition - transform.position;
                    break;
            }
        }

        Vector2? mousePosOffset;

        DragInputChecker checker = new DragInputChecker();

        int lastRetinuePos = -1;

        protected void Update()
        {
            checker.Update(Time.time);
            if (mousePosOffset != null)
            {
                transform.position = (Vector2)Input.mousePosition - mousePosOffset.Value;

                // 随从生成的预览
                if (cardVM.CardType == CardType.PositionArg || cardVM.CardType == CardType.PositionTargerArg)
                {
                    if (checkTarget())
                    {
                        var cnt = Deck.RetinueCount;
                        var calc = GetComponentInParent<GlobalView>().CardPositionCalculator;
                        var pos = 0;
                        for (pos = 0; pos < cnt; pos++)
                        {
                            var p = calc.GetRetinuePosition(pos, cnt);
                            if (transform.localPosition.x < p.Position.x)
                                break;
                        }
                        if (lastRetinuePos != pos)
                        {
                            lastRetinuePos = pos;
                            cardVM.DoAction(new RetinuePreview(pos));
                        }
                    }
                    else
                    {
                        if (lastRetinuePos >= 0)
                        {
                            lastRetinuePos = -1;
                            cardVM.DoAction(new RetinuePreview(-1));
                        }
                    }
                }
            }
        }

        public void OnPointerDown(PointerEventData eventData)
        {
            if (Deck.IsSelf && drawed)
            {
                checker.PointerDown(Time.time);
            }
        }

        public void OnPointerUp(PointerEventData eventData)
        {
            if (Deck.IsSelf && drawed)
            {
                checker.PointerUp();
            }
        }

        Controller.BoardController _deck;
        Controller.BoardController Deck
        {
            get
            {
                _deck = _deck ?? GetComponentInParent<Controller.BoardController>();
                return _deck;
            }
        }

        void setThrowing(bool state)
        {
            cardVM.DoAction(new PrepareThrowEventArgs(state));
        }

        void onMouseDrag()
        {
            DebugUtils.Trace("鼠标按住");

            if (Deck.ThrowingCard)
            {
                switch (CurrentState)
                {
                    case state.hand:
                    case state.hand_hover:
                        CurrentState = state.center;
                        setThrowing(true);
                        break;
                    case state.free:
                    case state.center:
                        CurrentState = state.hand;
                        setThrowing(false);
                        break;
                }
            }
            else
            {
                switch (CurrentState)
                {
                    case state.hand:
                    case state.hand_hover:
                        CurrentState = state.free;
                        break;
                }
            }
        }

        void onMouseRelease()
        {
            DebugUtils.Trace("鼠标松开");
            if (Deck.ThrowingCard)
            {
                // do nothing
            }
            else
            {
                switch (CurrentState)
                {
                    case state.free:
                        if (checkTarget()) Use();
                        else CurrentState = state.hand;
                        break;
                }
            }
        }

        private void onMouseClick()
        {
            DebugUtils.Trace("鼠标按下");
            if (Deck.ThrowingCard)
            {
                switch (CurrentState)
                {
                    case state.hand:
                    case state.hand_hover:
                        CurrentState = state.center;
                        setThrowing(true);
                        break;
                    case state.center:
                        CurrentState = state.hand;
                        setThrowing(false);
                        break;
                    case state.free:
                        break;
                }
            }
            else
            {
                switch (CurrentState)
                {
                    case state.hand:
                    case state.hand_hover:
                        CurrentState = state.free;
                        break;
                    case state.free:
                        if (checkTarget()) Use();
                        else CurrentState = state.hand;
                        break;
                    case state.center:
                        break;
                }
            }
        }
        #endregion

        bool checkTarget()
        {
            var gv = GetComponentInParent<GlobalView>();
            if (gv.CardPositionCalculator.CardUseThresold < rectTransform.position.y)
            {
                // todo: 做更多的判定
                return true;
            }
            return false;
        }
        void Use()
        {
            // 进入未知状态，等待Core发布使用的指令再移动
            CurrentState = state.undefined;
            if (cardVM.CardType == CardType.PositionArg)
            {
                cardVM.Use(new UseCardWithPositionArgs(lastRetinuePos));
            }
        }

        public enum state
        {
            hand,
            hand_hover,
            free,
            center,
            undefined
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
        const int distanceThresold = 5;

        Vector3 mousePos;

        public void Update(float time)
        {
            if (lastPointerDownTime != null)
            {
                if (time - lastPointerDownTime > dragThresold ||
                    Vector3.Distance(mousePos, Input.mousePosition) > distanceThresold)
                {
                    OnDrag?.Invoke();
                    lastPointerDownTime = null;
                }
            }
        }

        public void PointerDown(float time)
        {
            lastPointerDownTime = time;
            mousePos = Input.mousePosition;
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
