using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TouhouCardEngine;
using TouhouCardEngine.Interfaces;
using TouhouHeartstone;
using UnityEngine;
using Card = TouhouCardEngine.Card;

namespace Game.BattleTable
{
    using Arg = DiscardEventDefine;
    /// <summary>
    /// 弃牌动画，这是一个事件动画
    /// </summary>
    class DiscardAnimation : EventAnimation<Arg>
    {
        private CardSnapshot startCardSnapshot;
        public override IEnumerator startUpdate(BattleTableManager table)
        {
            startCardSnapshot = table.getEventCardSnapshot(startData, EVENTVAR.CARD);
            yield break;
        }
        public override IEnumerator logicUpdate(BattleTableManager table)
        {
            var cardId = eventArg.getVar<Card>(EVENTVAR.CARD).id;
            var player = eventArg.getVar<Player>(EVENTVAR.PLAYER);
            AnimAnim anim = null;

            if (table.tryGetServant(cardId, out var servant)) // 有些卡可能会让你在使用一张卡时取消使用并弃掉，此时如果使用的是仆从牌则可能会有随从在战场上。
            {
                var desc = table.getCardDescriptor(cardId);
                var fromField = table.getFieldOfServant(servant);
                var fromPile = table.getFieldPile(fromField);
                var startPos = table.WorldToTablePosition(servant.transform.position);
                var moveAnim = new MoveToHandAnim(desc, startPos, fromPile, startCardSnapshot.pile, startCardSnapshot.position, table.canSeeCard(cardId));
                while (!moveAnim.update(table, _deltaTime))
                    yield return null;
            }

            if (table.tryGetHand(cardId, out var hand))//尝试获取弃牌的手牌UI
            {
                hand.onDiscard.invokeBeforeAnim();
                anim = new AnimAnim(hand.animator, hand.onDiscard.animName);
                var currentPos = hand.positionLerp.getPosition();
                var targetZ = table.table.CenterDisplayTransform.transform.position.z;
                var targetPos = currentPos + Mathf.Sign(targetZ - currentPos.z) * 2 * Vector3.forward;
                hand.positionLerp.setStartPosition(currentPos);
                hand.positionLerp.setTargetPosition(targetPos);
                hand.card.animateFaceup(true);
            }

            while (anim != null && !anim.update(table, _deltaTime))
                yield return null;

            if (table.tryGetHand(cardId, out hand))//尝试获取弃牌的手牌UI
            {
                hand.onDiscard.invokeAfterAnim();
                table.removeHand(cardId);//移除手牌UI
            }
        }

        public override bool continueNextGroup(AnimationGroup nextGroup)
        {
            var parents = getAllParents(eventArg);
            if (nextGroup.All(a => canAnimContinue(a, parents)))
            {
                return true;
            }
            return base.continueNextGroup(nextGroup);
        }
        private bool canAnimContinue(TableAnimation anim, IEnumerable<IEventArg> parents)
        {
            if (anim is DiscardAnimation discard)
            {
                IEventArg parent = discard.eventArg;
                while (parent != null)
                {
                    if (parents.Contains(parent))
                        return true;
                    parent = parent.parent;
                }
            }
            if (anim is MoveCardAnim move)
            {
                IEventArg parent = move.eventArg;
                while (parent != null)
                {
                    if (parents.Contains(parent))
                        return true;
                    parent = parent.parent;
                }
            }
            else if (anim.isInstantAnimation())
            {
                return true;
            }
            return false;
        }
        private IEventArg[] getAllParents(IEventArg arg)
        {
            List<IEventArg> parents = new List<IEventArg>();
            var parent = arg;
            while (parent != null)
            {
                parents.Add(parent);
                parent = parent.parent;
            }
            return parents.ToArray();
        }
    }
}
