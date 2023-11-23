using System.Collections;
using TouhouCardEngine;
using TouhouHeartstone;
using UI;
using UnityEngine;

namespace Game.BattleTable
{
    class MoveToHandAnim : CardMoveAnim
    {
        public MoveToHandAnim(CardDescriptor cardDesc, Vector3 startPos, Pile fromPile, Pile toPile, int position) : base(cardDesc, startPos, fromPile, toPile)
        {
            _position = position;
        }
        public override IEnumerator startUpdate(BattleTableManager table, float deltaTime)
        {
            // 1. 如果HandItem已经存在，什么也不播放。
            if (table.tryGetHand(_cardDesc.id, out _))
                yield break;

            // 2. 移除源牌堆的卡牌。
            removeCard(table);
            // 3 .创建HandItem，从源位置将HandItem飞到中间位置。
            var hand = table.createHand(_cardDesc, _to.owner, _position);
            _handItem = hand;
            hand.ignoreAlign = true;
            var positionLerp = hand.positionLerp;
            var worldPos = table.TableToWorldPosition(_startTablePos);
            var targetPos = hand.transform.position;

            updateStartFaceup(table, hand.card);

            var targetPosition = getMiddleTargetPosition(table, worldPos, targetPos);

            SimpleAnim simpleAnim;
            // 4. 如果源牌堆是不会显示的牌堆，动画会有所改变。
            if (!isVisiblePile(_from))
            {
                simpleAnim = hand.onCreatedStart;
            }
            else if (isDeckPile(_from) && (_to?.owner == table.player || _from?.owner == table.player))
            {
                simpleAnim = hand.onSelfDrawStart;
            }
            else
            {
                simpleAnim = hand.onLinearDrawStart;
            }

            hand.transform.position = worldPos;
            positionLerp.setPosition(worldPos);
            positionLerp.setStartPosition(worldPos);
            positionLerp.setTargetPosition(targetPosition);

            // 播放手牌移动动画。
            AnimAnim anim = null;
            while (!SimpleAnimHelper.update(table, ref anim, simpleAnim, hand.animator, deltaTime))
            {
                yield return null;
            }
        }
        public override IEnumerator logicUpdate(BattleTableManager table, float deltaTime)
        {
            if (!table.tryGetHand(_cardDesc.id, out var hand))
            {
                // 创建移动手牌。
                hand = table.createHand(_cardDesc, _to?.owner);
                _handItem = hand;
            }

            // 1. 使用HandItem飞到目标。
            updateEndFaceup(table, hand.card);

            SimpleAnim simpleAnim;
            if (!isVisiblePile(_from))
            {
                simpleAnim = hand.onCreated;
            }
            else if (isDeckPile(_from) && (_to?.owner == table.player || _from?.owner == table.player))
            {
                simpleAnim = hand.onSelfDraw;
            }
            else
            {
                simpleAnim = hand.onLinearDraw;
            }

            var positionLerp = hand.positionLerp;
            var worldPos = hand.transform.position;
            var handList = table.getHandList(_to);
            var handIndex = handList.indexOf(hand);
            var targetPos = handList.getHandTargetPosition(handIndex);
            positionLerp.setPosition(worldPos);
            positionLerp.setStartPosition(worldPos);
            positionLerp.setTargetPosition(targetPos);

            // 播放手牌移动动画。
            AnimAnim anim = null;
            while (!SimpleAnimHelper.update(table, ref anim, simpleAnim, hand.animator, deltaTime))
            {
                yield return null;
                targetPos = handList.getHandTargetPosition(handIndex);
                positionLerp.setTargetPosition(targetPos);
            }
            hand.ignoreAlign = false;
        }
        public override Vector3 getCurrentPosition(BattleTableManager table)
        {
            return table.WorldToTablePosition(_handItem.transform.position);
        }
        private static bool isDeckPile(Pile pile)
        {
            switch (pile?.name)
            {
                case PILE.DECK:
                case PILE.GRAVE:
                    return true;
            }
            return false;
        }
        private static bool isVisiblePile(Pile pile)
        {
            if (isDeckPile(pile))
                return true;
            switch (pile?.name)
            {
                case PILE.FIELD:
                case PILE.MASTER:
                case PILE.HEAD:
                case PILE.SKILL:
                case PILE.ITEM:
                    return true;
            }
            return false;
        }
        private int _position;
        private HandItem _handItem;
    }
}
