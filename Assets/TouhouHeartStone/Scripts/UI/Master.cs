using UnityEngine;
using System;
using UnityEngine.EventSystems;
using UnityEngine.Events;
using TouhouHeartstone;
using System.Linq;
namespace UI
{
    interface ITargetedAnim
    {
        string targetedAnimName { get; }
    }
    partial class Master : IBeginDragHandler, IDragHandler, IEndDragHandler, IPointerDownHandler, ITargetedAnim
    {
        [Obsolete]
        public TouhouCardEngine.Card card { get; private set; } = null;
        public void update(Table table, THHPlayer player, TouhouCardEngine.Card card, CardSkinData skin)
        {
            this.card = card;

            Image.sprite = skin.image;
            LifePropNumber.asText.text = card.getCurrentLife(null).ToString();
            //if (card.getCurrentLife() == card.getLife())
            //    HpText.color = Color.white;
            //else
            //    HpText.color = Color.red;
            if (card.getAttack(null) > 0)
            {
                AttackPropNumber.asText.text = card.getAttack(null).ToString();
                AttackPropNumber.asText.enabled = true;
            }
            else
                AttackPropNumber.asText.enabled = false;
            if (card.getArmor(null) > 0)
            {
                ArmorPropNumber.asText.text = card.getArmor(null).ToString();
                ArmorPropNumber.asText.enabled = true;
            }
            else
                ArmorPropNumber.asText.enabled = false;

            if (table.selectableTargets != null && table.selectableTargets.Contains(this))
            {
                // HighlightController = Highlight.Yellow;
                onHighlightControllerYellow?.Invoke();
            }
            else if (table.player == player && table.game.currentPlayer == player && card.canAttack(table.game))
            {
                // HighlightController = Highlight.Green;
                onHighlightControllerGreen?.Invoke();
            }
            else
            {
                // HighlightController = Highlight.None;
                onHighlightControllerNone?.Invoke();
            }
        }
        [SerializeField]
        float _attackThreshold = 50;
        void IBeginDragHandler.OnBeginDrag(PointerEventData eventData)
        {
            //if (!card.canAttack())
            //    return;
            ////拉动距离也应该有一个阈值
            //if (Vector2.Distance(rectTransform.position, eventData.position) > _attackThreshold)
            //{
            //    //播放一个变大的动画？
            //    rectTransform.localScale = Vector3.one * 1.1f;
            //    //创建指针
            //}
            //else
            //{
            //    rectTransform.localScale = Vector3.one;
            //}
        }

        void IDragHandler.OnDrag(PointerEventData eventData)
        {
            //if (!card.canAttack())
            //    return;
            //Table table = GetComponentInParent<Table>();
        }

        void IEndDragHandler.OnEndDrag(PointerEventData eventData)
        {
            //if (!card.canAttack())
            //    return;
            ////如果在随从上面
            //if (eventData.pointerCurrentRaycast.gameObject != null &&
            //    eventData.pointerCurrentRaycast.gameObject.GetComponent<Servant>() is Servant target)
            //{
            //    Table table = GetComponentInParent<Table>();
            //    if (card.isAttackable(table.game, table.player, target.card, out var tip))
            //    {
            //        table.player.cmdAttack(table.game, card, target.card);
            //    }
            //    else
            //    {
            //        table.showTip(tip);
            //        //取消选中和攻击
            //        rectTransform.localScale = Vector3.one;
            //    }
            //}
        }
        public void OnPointerDown(PointerEventData eventData)
        {
            onClick.invoke(this, eventData);
        }
        public ActionEvent<Master, PointerEventData> onClick { get; } = new ActionEvent<Master, PointerEventData>();
        [SerializeField]
        string _targetedAnimName = "Targetd";
        public string targetedAnimName => _targetedAnimName;
        [SerializeField]
        private UnityEvent _onHighlightControllerNone;
        public UnityEvent onHighlightControllerNone => _onHighlightControllerNone;

        [SerializeField]
        private UnityEvent _onHighlightControllerYellow;
        public UnityEvent onHighlightControllerYellow => _onHighlightControllerYellow;

        [SerializeField]
        private UnityEvent _onHighlightControllerGreen;
        public UnityEvent onHighlightControllerGreen => _onHighlightControllerGreen;
        public SimpleAnim onDamage;
        public SimpleAnim onDeath;
    }
}