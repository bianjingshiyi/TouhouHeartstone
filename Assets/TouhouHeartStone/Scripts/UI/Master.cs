using UnityEngine;
using System;
using UnityEngine.EventSystems;
using UnityEngine.Events;
using TouhouHeartstone;
using System.Linq;
namespace UI
{
    partial class Master : IBeginDragHandler, IDragHandler, IEndDragHandler, IPointerDownHandler
    {
        [Obsolete]
        public TouhouCardEngine.Card card { get; private set; } = null;
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
        public SimpleAnim onTargeted;
        public SimpleAnim onDamage;
        public SimpleAnim onDeath;
        public SimpleAnim onCanAttackTrue;
        public SimpleAnim onCanAttackFalse;
        public SimpleAnim onSelectableTrue;
        public SimpleAnim onSelectableFalse;
    }
}