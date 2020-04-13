using UnityEngine;
using UnityEngine.EventSystems;
using TouhouHeartstone;

namespace UI
{
    partial class Master : IBeginDragHandler, IDragHandler, IEndDragHandler
    {
        public TouhouCardEngine.Card card { get; private set; } = null;
        public void update(TouhouCardEngine.Card card, CardSkinData skin)
        {
            this.card = card;

            Image.sprite = skin.image;
            HpText.text = card.getCurrentLife().ToString();
            //if (card.getCurrentLife() == card.getLife())
            //    HpText.color = Color.white;
            //else
            //    HpText.color = Color.red;
            if (card.getAttack() > 0)
            {
                AttackText.text = card.getAttack().ToString();
                AttackText.enabled = true;
            }
            else
                AttackText.enabled = false;
            if (card.getArmor() > 0)
            {
                ArmorText.text = card.getArmor().ToString();
                ArmorText.enabled = true;
            }
            else
                ArmorText.enabled = false;

            if (card.canAttack())
            {

            }
            else
            {

            }
        }
        [SerializeField]
        float _attackThreshold = 50;
        void IBeginDragHandler.OnBeginDrag(PointerEventData eventData)
        {
            if (!card.canAttack())
                return;
            //拉动距离也应该有一个阈值
            if (Vector2.Distance(rectTransform.position, eventData.position) > _attackThreshold)
            {
                //播放一个变大的动画？
                rectTransform.localScale = Vector3.one * 1.1f;
                //创建指针
            }
            else
            {
                rectTransform.localScale = Vector3.one;
            }
        }

        void IDragHandler.OnDrag(PointerEventData eventData)
        {
            if (!card.canAttack())
                return;
            Table table = GetComponentInParent<Table>();
            //移动指针
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
    }
}
