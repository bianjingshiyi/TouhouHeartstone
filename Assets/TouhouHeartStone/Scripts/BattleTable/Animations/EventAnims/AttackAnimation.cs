using System.Collections;
using System.Linq;
using TouhouCardEngine.Histories;
using TouhouHeartstone;
using UI;
using UnityEngine;
using Card = TouhouCardEngine.Card;

namespace Game.BattleTable
{
    using Arg = THHCard.AttackEventArg;
    class AttackAnimation : EventAnimation<Arg>
    {
        public override IEnumerator startUpdate(BattleTableManager table)
        {
            var card = table.getEventCardSnapshot(startData, EVENTVAR.CARD);
            var target = table.getEventCardSnapshot(startData, EVENTVAR.TARGET_CARD);
            TableCharacter attackObj = getCharacterUI(table, card.id);
            TableCharacter attackedObj = getCharacterUI(table, target.id);
            SimpleAnim attackStartSimpleAnim = attackObj?.onAttackStart;

            if (attackObj && attackedObj)
            {
                attackObj.positionLerp.setStartPositionToCurrent();
                attackObj.positionLerp.setTargetTransform(attackedObj.transform, table.TableToWorldDistance(150));
            }

            if (attackObj && attackStartSimpleAnim != null)
            {
                AnimAnim attackAnim = null;
                while (!SimpleAnimHelper.update(table, ref attackAnim, attackStartSimpleAnim, attackObj.animator, _deltaTime))
                    yield return null;
            }
        }
        public override IEnumerator logicUpdate(BattleTableManager table)
        {
            var card = tEventArg.getVar<Card>(EVENTVAR.CARD);
            var target = tEventArg.getVar<Card>(EVENTVAR.TARGET_CARD);
            TableCharacter attackObj = getCharacterUI(table, card.id);
            TableCharacter attackedObj = getCharacterUI(table, target.id);
            SimpleAnim attackSimpleAnim = attackObj?.onAttack;
            SimpleAnim attackedSimpleAnim = attackedObj?.onAttacked;

            // 攻击。
            if (attackObj && attackedObj)
            {
                attackObj.positionLerp.setStartPositionToCurrent();
                attackObj.positionLerp.setTargetTransform(attackedObj.transform, table.TableToWorldDistance(150));
            }

            if (attackObj && attackSimpleAnim != null)
            {
                AnimAnim attackAnim = null;
                while (!SimpleAnimHelper.update(table, ref attackAnim, attackSimpleAnim, attackObj.animator, _deltaTime))
                {
                    if (attackObj.isAttackHit())
                    {
                        _attacked = true;
                    }
                    yield return null;
                }
                attackObj.resetAttackHit();
            }
            _attacked = true;
            if (attackedObj && attackedSimpleAnim != null)
            {
                AnimAnim attackAnim = null;
                while (!SimpleAnimHelper.update(table, ref attackAnim, attackedSimpleAnim, attackedObj.animator, _deltaTime))
                    yield return null;
            }

            // 返回。
            yield return returnFromAttack(table, card.id);
        }
        public override IEnumerator canceledUpdate(BattleTableManager table)
        {
            var card = tEventArg.getVar<Card>(EVENTVAR.CARD);
            yield return returnFromAttack(table, card.id);
        }
        public override bool continueNextGroup(AnimationGroup nextGroup)
        {
            if (isAnimGroupDamageOfThis(nextGroup))
                return _attacked;
            return base.continueNextGroup(nextGroup);
        }
        private IEnumerator returnFromAttack(BattleTableManager table, int cardId)
        {
            TableCharacter attackObj = getCharacterUI(table, cardId);
            SimpleAnim attackEndSimpleAnim = attackObj?.onAttackEnd;

            if (attackObj)
            {
                attackObj.positionLerp.setStartLocalPosition(Vector3.zero);
                attackObj.positionLerp.setTargetPositionToCurrent();
            }

            if (attackObj && attackEndSimpleAnim != null)
            {
                AnimAnim attackAnim = null;
                while (!SimpleAnimHelper.update(table, ref attackAnim, attackEndSimpleAnim, attackObj.animator, _deltaTime))
                    yield return null;
            }
        }
        private TableCharacter getCharacterUI(BattleTableManager table, int id)
        {
            if (table.tryGetMaster(id, out var attackMaster))
            {
                return attackMaster;
            }
            else if (table.tryGetServant(id, out var attackServant))
            {
                return attackServant;
            }
            return null;
        }
        private bool isAnimGroupDamageOfThis(AnimationGroup nextGroup)
        {
            return nextGroup.Any(a =>
            {
                if (a is DamageAnimation damageAnim)
                {
                    // 伤害动画的事件的父事件是该攻击动画的攻击事件。
                    return damageAnim.tEventArg.parent == tEventArg && damageAnim.tEventArg.target == tEventArg.target;
                }
                else if (a is PropChangeAnim propAnim)
                {
                    // 属性变化动画的事件，是该攻击事件的伤害事件的子事件。
                    var evt = propAnim.eventArg;
                    while (evt != null)
                    {
                        if (evt.parent is THHCard.DamageEventArg damageEvent && damageEvent.parent == tEventArg && damageEvent.target == tEventArg.target)
                            return true;
                        evt = evt.parent;
                    }
                }
                return false;
            });
        }
        bool _attacked = false;
    }
}