using System;
using UnityEngine.EventSystems;
using UnityEngine;
using TouhouHeartstone;
using System.Linq;
using TouhouCardEngine;
using UnityEngine.Events;
using BJSYGameCore;
using BJSYGameCore.UI;
namespace UI
{
    partial class ServantItem : IPointerEnterHandler, IPointerExitHandler, IBeginDragHandler, IDragHandler, IEndDragHandler, IPointerDownHandler
    {
        [Obsolete]
        public TouhouCardEngine.Card card { get; private set; }
        [SerializeField]
        AnimationCurve _attackAnimationCurve = new AnimationCurve(new Keyframe(0, 0), new Keyframe(1, 1));
        public AnimationCurve attackAnimationCurve
        {
            get { return _attackAnimationCurve; }
        }
        //public void update(CardDefine card, CardSkinData skin)
        //{
        //    if (skin != null)
        //    {
        //        Image.sprite = skin.image;
        //    }
        //    AttackTextPropNumber.text.text = card.getAttack().ToString();
        //    HpTextPropNumber.text.text = card.getLife().ToString();
        //}
        [SerializeField]
        float _attackThreshold = 70;
        public float attackThreshold
        {
            get { return _attackThreshold; }
        }
        [SerializeField]
        bool _isHovered = false;
        public ActionEvent<ServantItem, PointerEventData> onPointerEnter { get; set; } = new ActionEvent<ServantItem, PointerEventData>();
        public ActionEvent<ServantItem, PointerEventData> onPointerExit { get; set; } = new ActionEvent<ServantItem, PointerEventData>();
        void IBeginDragHandler.OnBeginDrag(PointerEventData eventData)
        {
        }
        void IDragHandler.OnDrag(PointerEventData eventData)
        {
            //Table table = GetComponentInParent<Table>();
            //if (!table.canControl)//不是你的回合
            //    return;
            //if (card.owner != table.player)//不是你的卡
            //    return;
            //if (!card.canAttack(table.game))
            //    return;
            ////拉动距离也应该有一个阈值
            //if (Vector2.Distance(rectTransform.position, eventData.position) > _attackThreshold)
            //{
            //    //播放一个变大的动画？
            //    rectTransform.localScale = Vector3.one * 1.1f;
            //    //显示指针
            //    table.AttackArrowImage.display();
            //    table.AttackArrowImage.rectTransform.position = rectTransform.position;
            //    //移动指针
            //    table.AttackArrowImage.rectTransform.eulerAngles = new Vector3(0, 0,
            //        Vector2.Angle(rectTransform.position, eventData.position));
            //    table.AttackArrowImage.rectTransform.up = ((Vector3)eventData.position - rectTransform.position).normalized;
            //    table.AttackArrowImage.rectTransform.sizeDelta = new Vector2(
            //        table.AttackArrowImage.rectTransform.sizeDelta.x,
            //        Vector2.Distance(rectTransform.position, eventData.position) / GetComponentInParent<Canvas>().transform.localScale.y);
            //    //高亮标记所有目标
            //    table.selectableTargets = table.getCharacters(table.game.findAllCardsInField(c => card.isAttackable(table.game, table.player, c, out _)));
            //}
            //else
            //{
            //    cancelAttack(table);
            //}
            onDrag.invoke(this, eventData);
        }
        public ActionEvent<ServantItem, PointerEventData> onDrag { get; } = new ActionEvent<ServantItem, PointerEventData>();
        void IEndDragHandler.OnEndDrag(PointerEventData eventData)
        {
            //Table table = GetComponentInParent<Table>();
            //if (!table.canControl)//不是你的回合
            //    return;
            //if (card.owner != table.player)//不是你的卡
            //    return;
            //if (!card.canAttack(table.game))//不能攻击
            //    return;
            ////如果在随从上面
            //if (eventData.pointerCurrentRaycast.gameObject != null)
            //{
            //    if (eventData.pointerCurrentRaycast.gameObject.GetComponentInParent<Servant>() is Servant targetServant)
            //    {
            //        if (card.isAttackable(table.game, table.player, targetServant.card, out var tip))
            //        {
            //            table.player.cmdAttack(table.game, card, targetServant.card);
            //        }
            //        else
            //            table.showTip(tip);
            //    }
            //    else if (eventData.pointerCurrentRaycast.gameObject.GetComponentInParent<Master>() is Master targetMaster)
            //    {
            //        if (card.isAttackable(table.game, table.player, targetMaster.card, out var tip))
            //        {
            //            table.player.cmdAttack(table.game, card, targetMaster.card);
            //        }
            //        else
            //            table.showTip(tip);
            //    }
            //}
            ////取消选中和攻击
            //cancelAttack(table);
            onDragEnd.invoke(this, eventData);
        }
        public ActionEvent<ServantItem, PointerEventData> onDragEnd { get; } = new ActionEvent<ServantItem, PointerEventData>();
        private void cancelAttack(Table table)
        {
            rectTransform.localScale = Vector3.one;
            table.AttackArrowImage.hide();
            table.selectableTargets = null;
        }
        void IPointerEnterHandler.OnPointerEnter(PointerEventData eventData)
        {
            _isHovered = true;
            onPointerEnter.invoke(this, eventData);
        }
        void IPointerExitHandler.OnPointerExit(PointerEventData eventData)
        {
            _isHovered = false;
            onPointerExit.invoke(this, eventData);
        }
        protected void Update()
        {
            if (_isHovered)
            {
                if (!ui.getManager<InputManager>().isPointerOver(rectTransform))
                {
                    _isHovered = false;
                    onPointerExit.invoke(this, null);
                }
            }
        }
        protected void OnDestroy()
        {
            if (_isHovered)
            {
                if (ui.getManager<InputManager>().isPointerOver(rectTransform))
                    onPointerExit.invoke(this, null);
            }
            _isHovered = false;
        }
        public void OnPointerDown(PointerEventData eventData)
        {
            onClick.invoke(this, eventData);
        }

        public new void setBoolTrue(string name)
        {
            servantAnimator.SetBool(name, true);
        }

        public new void setBoolFalse(string name)
        {
            servantAnimator.SetBool(name, false);
        }

        public ActionEvent<ServantItem, PointerEventData> onClick { get; } = new ActionEvent<ServantItem, PointerEventData>();
        public SimpleAnim onAttackUp;
        public SimpleAnim onAttackDown;
        public SimpleAnim onLifeUp;
        public SimpleAnim onLifeDown;
        public SimpleAnim onTargeted;
        public SimpleAnim onAttack;
        public SimpleAnim onAttacked;
        public SimpleAnim onDamage;
        public SimpleAnim onHeal;
        public SimpleAnim onSummon;
        public SimpleAnim onDeath;
        public SimpleAnim onTauntTrue;
        public SimpleAnim onTauntFalse;
        public SimpleAnim onCanAttackTrue;
        public SimpleAnim onCanAttackFalse;
        public SimpleAnim onSelectableTrue;
        public SimpleAnim onSelectableFalse;
        public SimpleAnim onSetDefine;
    }
}