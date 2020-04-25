using System;
using UnityEngine.EventSystems;
using UnityEngine;
using TouhouHeartstone;
using System.Linq;
using TouhouCardEngine;
namespace UI
{
    partial class Servant : IPointerEnterHandler, IPointerExitHandler, IBeginDragHandler, IDragHandler, IEndDragHandler
    {
        public TouhouCardEngine.Card card { get; private set; }
        [SerializeField]
        AnimationCurve _attackAnimationCurve = new AnimationCurve(new Keyframe(0, 0), new Keyframe(1, 1));
        public AnimationCurve attackAnimationCurve
        {
            get { return _attackAnimationCurve; }
        }
        public void update(THHPlayer player, TouhouCardEngine.Card card, CardSkinData skin)
        {
            this.card = card;

            Table table = GetComponentInParent<Table>();
            if (skin != null)
            {
                Image.sprite = skin.image;
            }
            AttackText.text = card.getAttack().ToString();
            HpText.text = card.getCurrentLife().ToString();

            if (table.selectableTargets != null && table.selectableTargets.Contains(this))
                HighlightController = Highlight.Yellow;
            else if (table.player == player && table.game.currentPlayer == player && card.canAttack())
                HighlightController = Highlight.Green;
            else
                HighlightController = Highlight.None;
        }
        public void update(CardDefine card, CardSkinData skin)
        {
            if (skin != null)
            {
                Image.sprite = skin.image;
            }
            AttackText.text = card.getAttack().ToString();
            HpText.text = card.getLife().ToString();
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
            if (card.owner != table.player)//不是你的卡。
                return;
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
                table.AttackArrowImage.rectTransform.up = ((Vector3)eventData.position - rectTransform.position).normalized;
                table.AttackArrowImage.rectTransform.sizeDelta = new Vector2(
                    table.AttackArrowImage.rectTransform.sizeDelta.x,
                    Vector2.Distance(rectTransform.position, eventData.position) / GetComponentInParent<Canvas>().transform.localScale.y);
                //高亮标记所有目标
                //highlightAllTargets(target => card.isAttackable(table.game, table.player, target));
            }
            else
            {
                cancelAttack(table);
            }
        }

        private void highlightAllTargets(Func<TouhouCardEngine.Card, bool> filter, bool isGreen = true)
        {
            Table table = GetComponentInParent<Table>();
            table.EnemyMaster.HighlightController =
                    filter != null && filter(table.EnemyMaster.card) ?
                    isGreen ? Master.Highlight.Green : Master.Highlight.Yellow : Master.Highlight.None;
            table.SelfMaster.HighlightController =
                    filter != null && filter(table.SelfMaster.card) ?
                    isGreen ? Master.Highlight.Green : Master.Highlight.Yellow : Master.Highlight.None;
            foreach (var servant in table.EnemyFieldList)
            {
                servant.HighlightController =
                        filter != null && filter(servant.card) ?
                        isGreen ? Highlight.Green : Highlight.Yellow : Highlight.None;
            }
            foreach (var servant in table.SelfFieldList)
            {
                servant.HighlightController =
                        filter != null && filter(servant.card) ?
                        isGreen ? Highlight.Green : Highlight.Yellow : Highlight.None;
            }
        }

        void IEndDragHandler.OnEndDrag(PointerEventData eventData)
        {
            if (!card.canAttack())
                return;
            Table table = GetComponentInParent<Table>();
            //如果在随从上面
            if (eventData.pointerCurrentRaycast.gameObject != null)
            {
                if (eventData.pointerCurrentRaycast.gameObject.GetComponentInParent<Servant>() is Servant targetServant)
                {
                    if (card.isAttackable(table.game, table.player, targetServant.card, out var tip))
                    {
                        table.player.cmdAttack(table.game, card, targetServant.card);
                    }
                    else
                        table.showTip(tip);
                }
                else if (eventData.pointerCurrentRaycast.gameObject.GetComponentInParent<Master>() is Master targetMaster)
                {
                    if (card.isAttackable(table.game, table.player, targetMaster.card, out var tip))
                    {
                        table.player.cmdAttack(table.game, card, targetMaster.card);
                    }
                    else
                        table.showTip(tip);
                }
            }
            //取消选中和攻击
            cancelAttack(table);
        }
        private void cancelAttack(Table table)
        {
            rectTransform.localScale = Vector3.one;
            table.AttackArrowImage.hide();
            //highlightAllTargets(null);
        }
        void IPointerEnterHandler.OnPointerEnter(PointerEventData eventData)
        {
            if (card == null)
                return;
            displayLargeCard(eventData.position.x < Screen.width / 2);
        }
        private void displayLargeCard(bool isRight)
        {
            Table table = GetComponentInParent<Table>();
            if (isRight)
                table.LargeCard.rectTransform.localPosition = new Vector3(250, 0);
            else
                table.LargeCard.rectTransform.localPosition = new Vector3(-250, 0);
            table.LargeCard.display();
            table.LargeCard.update(card, table.getSkin(card));
        }
        void IPointerExitHandler.OnPointerExit(PointerEventData eventData)
        {
            hideLargeCard();
        }
        private void hideLargeCard()
        {
            Table table = GetComponentInParent<Table>();
            table.LargeCard.hide();
        }
    }
}