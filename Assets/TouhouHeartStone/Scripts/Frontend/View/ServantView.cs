using System;

using UnityEngine;

using TouhouHeartstone.Frontend.ViewModel;
using UnityEngine.EventSystems;
using TouhouHeartstone.Frontend.Model;
using TouhouHeartstone.Frontend.View.Animation;

namespace TouhouHeartstone.Frontend.View
{
    public class ServantView : AnimationPlayerBase, IPointerEnterHandler, IPointerExitHandler, IPointerDownHandler, IPointerUpHandler, IBeginDragHandler, IDragHandler, IEndDragHandler
    {
        [SerializeField]
        Controller.BoardController _board;
        [SerializeField]
        CardViewModel _cardVM = null;
        [SerializeField]
        RectTransform _targetArrow = null;
        [SerializeField]
        RectTransform _targetCircle = null;
        DragInputChecker checker = new DragInputChecker();
        [Obsolete("请使用CardVM.board替代")]
        Controller.BoardController board
        {
            get
            {
                _board = _board ?? GetComponentInParent<Controller.BoardController>();
                return _board;
            }
        }
        public CardViewModel cardVM
        {
            get
            {
                if (_cardVM == null)
                    _cardVM = GetComponentInParent<CardViewModel>();
                return _cardVM;
            }
        }
        RectTransform targetArrow
        {
            get { return _targetArrow; }
        }
        RectTransform targetCircle
        {
            get { return _targetCircle; }
        }
        bool drawed = false;

        void Awake()
        {
            // 注册VM事件
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
            if (args is OnAttackEventArgs)
            {
                var arg = args as OnAttackEventArgs;
                UberDebug.LogChannel(this, "Frontend", "ServantView收到攻击事件");
                var targetServant = cardVM.board.Deck.GetCardByRID(arg.TargetRID);

                PlayAnimation("ServantAttack", new ServantAttackEventArgs(
                    cardVM.Index, 
                    cardVM.board.RetinueCount, 
                    targetServant.Index, 
                    targetServant.board.RetinueCount)
                    ,callback);
            }

            if (args is IndexChangeEventArgs && drawed)
            {
                var arg = args as IndexChangeEventArgs;
                PlayAnimation(this, new CardAnimationEventArgs()
                {
                    AnimationName = "RetinueMove",
                    EventArgs = new CardPositionEventArgs() { GroupCount = arg.Count > 0 ? arg.Count : cardVM.board.RetinueCount, GroupID = arg.Index }
                }, callback);
            }

            if (args is RetinueSummonEventArgs)
            {
                callback += (a, b) => { drawed = true; };
                var arg = args as RetinueSummonEventArgs;
                PlayAnimation(this, new CardAnimationEventArgs()
                {
                    AnimationName = "RetinueSummon",
                    EventArgs = new CardPositionEventArgs() { GroupCount = cardVM.board.RetinueCount, GroupID = cardVM.Index }
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
            if (cardVM.board.IsSelf && drawed)
            {
                GetComponent<CardHighlight>()?.SetHighlight(true);
            }
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            if (cardVM.board.IsSelf && drawed)
            {
                GetComponent<CardHighlight>()?.SetHighlight(false);
            }
        }

        public void OnPointerDown(PointerEventData eventData)
        {
            if (cardVM.board.IsSelf && drawed)
            {
                checker.PointerDown(Time.time);
            }
        }

        public void OnPointerUp(PointerEventData eventData)
        {
            if (cardVM.board.IsSelf && drawed)
            {
                checker.PointerUp();
            }
        }

        void IBeginDragHandler.OnBeginDrag(PointerEventData eventData)
        {
            //激活箭头
            targetArrow.gameObject.SetActive(true);
            targetArrow.sizeDelta = new Vector2(targetArrow.sizeDelta.x, Vector3.Distance(Input.mousePosition, targetArrow.position) - 1);
            targetArrow.up = Input.mousePosition - targetArrow.position;
        }
        void IDragHandler.OnDrag(PointerEventData eventData)
        {
            //拖动箭头
            targetArrow.sizeDelta = new Vector2(targetArrow.sizeDelta.x, Vector3.Distance(Input.mousePosition, targetArrow.position) - 1);
            targetArrow.up = Input.mousePosition - targetArrow.position;
            //如果在目标上，显示目标。
            if (eventData.pointerCurrentRaycast.gameObject != null)
            {
                CardViewModel targetCard = eventData.pointerCurrentRaycast.gameObject.GetComponentInParent<CardViewModel>();
                //有目标，不是自己，并且是别人的。
                if (targetCard != null && targetCard != cardVM && targetCard.board != cardVM.board)
                    targetCircle.gameObject.SetActive(true);
                else
                    targetCircle.gameObject.SetActive(false);
            }
        }
        void IEndDragHandler.OnEndDrag(PointerEventData eventData)
        {
            //禁用箭头
            targetArrow.gameObject.SetActive(false);
            targetCircle.gameObject.SetActive(false);
            //如果在目标上，发布攻击命令
            if (eventData.pointerCurrentRaycast.gameObject != null)
            {
                CardViewModel targetCard = eventData.pointerCurrentRaycast.gameObject.GetComponentInParent<CardViewModel>();
                //有目标，不是自己，并且是别人的。
                if (targetCard != null && targetCard != cardVM && targetCard.board != cardVM.board)
                {
                    AutoPlayerCardEventArgs args = new AutoPlayerCardEventArgs("attack", cardVM.board.SelfID, cardVM.RuntimeID, targetCard.RuntimeID);
                    Debug.Log(targetCard);
                    cardVM.DoAction(args);
                }
            }
        }
        #endregion
    }
}