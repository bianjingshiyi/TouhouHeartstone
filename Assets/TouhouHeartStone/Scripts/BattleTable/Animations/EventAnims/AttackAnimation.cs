using System.Collections;
using System.Linq;
using TouhouCardEngine;
using TouhouCardEngine.Interfaces;
using TouhouHeartstone;
using UI;
using UnityEngine;

namespace Game.BattleTable
{
    using Arg = THHCard.AttackEventArg;
    class AttackAnimation : EventAnimation<Arg>
    {
        public override IEnumerator startUpdate(BattleTableManager table, EventRecord record, Arg arg)
        {
            var card = arg.card;
            var target = arg.target;
            TableCharacter attackObj = getCharacterUI(table, card);
            TableCharacter attackedObj = getCharacterUI(table, target);
            SimpleAnim attackStartSimpleAnim = attackObj?.onAttackStart;

            if (attackObj && attackedObj)
            {
                attackObj.positionLerp.setStartLocalPosition(Vector3.zero);
                attackObj.positionLerp.setTargetTransform(attackedObj.transform, table.TableToWorldDistance(150));
            }

            if (attackObj && attackStartSimpleAnim != null)
            {
                while (!SimpleAnimHelper.update(table, ref _attackAnim, attackStartSimpleAnim, attackObj.animator, _deltaTime))
                    yield return null;
            }
        }
        public override IEnumerator logicUpdate(BattleTableManager table, EventRecord record, Arg arg)
        {
            var card = arg.card;
            var target = arg.target;
            TableCharacter attackObj = getCharacterUI(table, card);
            TableCharacter attackedObj = getCharacterUI(table, target);
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
                while (!SimpleAnimHelper.update(table, ref _attackAnim, attackSimpleAnim, attackObj.animator, _deltaTime)) 
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
                while (!SimpleAnimHelper.update(table, ref _attackedAnim, attackedSimpleAnim, attackedObj.animator, _deltaTime))
                    yield return null;
            }

            // 返回。
            yield return returnFromAttack(table, arg);
        }
        public override IEnumerator canceledUpdate(BattleTableManager table, EventRecord record, Arg arg)
        {
            yield return returnFromAttack(table, arg);
        }
        public override bool continueNextGroup(AnimationGroup nextGroup)
        {
            if (isAnimGroupDamageOfThis(nextGroup))
                return _attacked;
            return base.continueNextGroup(nextGroup);
        }
        private IEnumerator returnFromAttack(BattleTableManager table, Arg arg)
        {
            var card = arg.card;
            var target = arg.target;
            TableCharacter attackObj = getCharacterUI(table, card);
            SimpleAnim attackEndSimpleAnim = attackObj?.onAttackEnd;

            if (attackObj)
            {
                attackObj.positionLerp.setStartLocalPosition(Vector3.zero);
                attackObj.positionLerp.setTargetPositionToCurrent();
            }

            if (attackObj && attackEndSimpleAnim != null)
            {
                while (!SimpleAnimHelper.update(table, ref _attackAnim, attackEndSimpleAnim, attackObj.animator, _deltaTime))
                    yield return null;
            }
        }
        private TableCharacter getCharacterUI(BattleTableManager table, ICardData card)
        {
            if (table.tryGetMaster(card, out var attackMaster))
            {
                return attackMaster;
            }
            else if (table.tryGetServant(card, out var attackServant))
            {
                return attackServant;
            }
            return null;
        }
        private bool isAnimGroupDamageOfThis(AnimationGroup nextGroup)
        {
            return nextGroup.Any(a => {
                if (a is DamageAnimation damageAnim)
                {
                    // 伤害动画的事件的父事件是该攻击动画的攻击事件。
                    return damageAnim.tEventArg.parent == tEventArg;
                }
                else if (a is PropChangeAnim propAnim)
                {
                    // 属性变化动画的事件，是该攻击事件的伤害事件的子事件。
                    var evt = propAnim.eventArg;
                    while (evt != null)
                    {
                        if (evt.parent is THHCard.DamageEventArg damageEvent && damageEvent.parent == tEventArg)
                            return true;
                        evt = evt.parent;
                    }
                }
                return false;
            });
        }
        bool _attacked = false;
        AnimAnim _attackAnim;
        AnimAnim _attackedAnim;
    }
}