using System;

using UnityEngine;

using TouhouHeartstone.Frontend.ViewModel;
using UnityEngine.EventSystems;
using TouhouHeartstone.Frontend.Model;
using TouhouHeartstone.Frontend.View.Animation;

namespace TouhouHeartstone.Frontend.View
{
    public class RetinueView : AnimationPlayerBase, IPointerEnterHandler, IPointerExitHandler, IPointerDownHandler, IPointerUpHandler
    {
        Controller.UserDeckController _deck;
        Controller.UserDeckController Deck
        {
            get
            {
                _deck = _deck ?? GetComponentInParent<Controller.UserDeckController>();
                return _deck;
            }
        }

        DragInputChecker checker = new DragInputChecker();
        CardViewModel cardVM;

        bool drawed = false;

        void Awake()
        {
            // 注册VM事件
            cardVM = GetComponentInParent<CardViewModel>();
            if (cardVM == null)
                throw new Exception("关联的ViewModel未找到");

            cardVM.OnRecvActionEvent += onRecvAction;
            checker.OnClick += onClick;
            checker.OnDrag += onDrag;
            checker.OnRelease += onRelease;
        }

        void Start()
        {
            // 注册完事件后就消失吧
            gameObject.SetActive(false);
        }

        protected void OnDestroy()
        {
            cardVM.OnRecvActionEvent -= onRecvAction;
        }

        private void onRecvAction(object sender, EventArgs args, GenericAction callback)
        {
            if (args is IndexChangeEventArgs && drawed)
            {
                var arg = args as IndexChangeEventArgs;
                PlayAnimation(this, new CardAnimationEventArgs()
                {
                    AnimationName = "RetinueMove",
                    EventArgs = new CardPositionEventArgs() { GroupCount = arg.Count > 0 ? arg.Count : Deck.RetinueCount, GroupID = arg.Index }
                }, callback);
            }

            if (args is RetinueSummonEventArgs)
            {
                callback += (a, b) => { drawed = true; };
                var arg = args as RetinueSummonEventArgs;
                PlayAnimation(this, new CardAnimationEventArgs()
                {
                    AnimationName = "RetinueSummon",
                    EventArgs = new CardPositionEventArgs() { GroupCount = Deck.RetinueCount, GroupID = cardVM.Index }
                }, callback);
            }
        }

        protected void Update()
        {
            checker.Update(Time.time);
        }

        #region mouse_events

        private void onRelease()
        {
            // 
        }

        private void onDrag()
        {
            // 
        }

        private void onClick()
        {
            // 
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            if (Deck.IsSelf && drawed)
            {
                GetComponent<CardHighlight>()?.SetHighlight(true);
            }
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            if (Deck.IsSelf && drawed)
            {
                GetComponent<CardHighlight>()?.SetHighlight(false);
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

        #endregion
    }
}
