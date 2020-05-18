using UnityEngine;
using UnityEngine.EventSystems;
using TouhouHeartstone;
using System.Linq;
namespace UI
{
    partial class Master : IBeginDragHandler, IDragHandler, IEndDragHandler
    {
        public TouhouCardEngine.Card card { get; private set; } = null;
        public void update(Table table, THHPlayer player, TouhouCardEngine.Card card, CardSkinData skin)
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

            if (table.selectableTargets != null && table.selectableTargets.Contains(this))
                HighlightController = Highlight.Yellow;
            else if (table.player == player && table.game.currentPlayer == player && card.canAttack(table.game))
                HighlightController = Highlight.Green;
            else
                HighlightController = Highlight.None;
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
    }
}