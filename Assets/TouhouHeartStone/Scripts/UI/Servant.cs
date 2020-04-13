using UnityEngine.EventSystems;
using UnityEngine;
using TouhouHeartstone;

namespace UI
{
    partial class Servant : IPointerEnterHandler, IPointerExitHandler, IBeginDragHandler, IDragHandler, IEndDragHandler
    {
        public TouhouCardEngine.Card card { get; private set; }
        public void update(THHPlayer player, TouhouCardEngine.Card card, CardSkinData skin)
        {
            this.card = card;

            Table table = GetComponentInParent<Table>();
            if (skin != null)
            {
                Image.sprite = skin.image;
            }
            if (table.player == player && card.canAttack())
                CanAttackController = CanAttack.True;
            else
                CanAttackController = CanAttack.False;
        }
        [SerializeField]
        float _attackThreshold = 70;
        void IBeginDragHandler.OnBeginDrag(PointerEventData eventData)
        {
            if (!card.canAttack())
                return;
        }
        void IDragHandler.OnDrag(PointerEventData eventData)
        {
            if (!card.canAttack())
                return;
            Table table = GetComponentInParent<Table>();
            //拉动距离也应该有一个阈值
            if (Vector2.Distance(rectTransform.position, eventData.position) > _attackThreshold)
            {
                //播放一个变大的动画？
                rectTransform.localScale = Vector3.one * 1.1f;
                //显示指针
                table.AttackArrowImage.display();
                table.AttackArrowImage.rectTransform.position = rectTransform.position;
                //移动指针
                table.AttackArrowImage.rectTransform.eulerAngles = new Vector3(0, 0,
                    Vector2.Angle(rectTransform.position, eventData.position));
                table.AttackArrowImage.rectTransform.sizeDelta = new Vector2(
                    table.AttackArrowImage.rectTransform.sizeDelta.x,
                    Vector2.Distance(rectTransform.position, eventData.position));
                //高亮标记所有敌人
                THHPlayer opponent = table.game.getOpponent(table.player);
                if (card.isAttackable(table.game, table.player, opponent.master))
                {

                }
                else
                {

                }
                foreach (var servant in table.EnemyFieldList)
                {
                    if (card.isAttackable(table.game, table.player, servant.card))
                    {

                    }
                    else
                    {

                    }
                }
            }
            else
            {
                rectTransform.localScale = Vector3.one;
            }
        }
        void IEndDragHandler.OnEndDrag(PointerEventData eventData)
        {
            if (!card.canAttack())
                return;
            //如果在随从上面
            if (eventData.pointerCurrentRaycast.gameObject != null &&
                eventData.pointerCurrentRaycast.gameObject.GetComponent<Servant>() is Servant target)
            {
                Table table = GetComponentInParent<Table>();
                if (card.isAttackable(table.game, table.player, target.card))
                {
                    table.player.cmdAttack(table.game, card, target.card);
                }
                else
                {
                    //取消选中和攻击
                    rectTransform.localScale = Vector3.one;
                }
            }
        }
        void IPointerEnterHandler.OnPointerEnter(PointerEventData eventData)
        {
            Table table = GetComponentInParent<Table>();
            if (eventData.position.x < Screen.width / 2)
                table.LargeCard.rectTransform.localPosition = new Vector3(250, 0);
            else
                table.LargeCard.rectTransform.localPosition = new Vector3(-250, 0);
            table.LargeCard.display();
            table.LargeCard.update(card, table.getSkin(card));
        }
        void IPointerExitHandler.OnPointerExit(PointerEventData eventData)
        {
            Table table = GetComponentInParent<Table>();
            table.LargeCard.hide();
        }
    }
}
