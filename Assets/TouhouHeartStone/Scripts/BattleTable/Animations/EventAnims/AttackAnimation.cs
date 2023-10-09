using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TouhouCardEngine;
using TouhouHeartstone;
using UI;

namespace Game.BattleTable
{
    using Arg = THHCard.AttackEventArg;
    class AttackAnimation : EventAnimation<Arg>
    {
        bool _attacked = false;
        bool _waitingOtherAnims = false;
        AnimAnim _attackAnim;
        AnimAnim _attackedAnim;
        public override IEnumerator update(BattleTableManager table, EventRecord record, Arg arg)
        {
            yield return pauseWhileNotCompleted();
            yield return stopWhenCanceled();

            while (waitingOtherAnim(table))
            {
                _waitingOtherAnims = true;
                yield return null;
            }
            _waitingOtherAnims = false;


            var card = arg.card;
            var target = arg.target;
            SimpleAnim attackSimpleAnim = null;
            SimpleAnim attackedSimpleAnim = null;

            TableUnit attackObj = null;
            TableUnit attackedObj = null;

            if (table.tryGetMaster(card, out var attackMaster))
            {
                attackObj = attackMaster;
                attackSimpleAnim = attackMaster.onAttack;
            }
            else if (table.tryGetServant(card, out var attackServant))
            {
                attackObj = attackServant;
                attackSimpleAnim = attackServant.onAttack;
            }
            if (table.tryGetMaster(target, out var targetMaster))
            {
                attackedObj = targetMaster;
                attackedSimpleAnim = targetMaster.onAttacked;
            }
            else if (table.tryGetServant(target, out var targetServant))
            {
                attackedObj = targetServant;
                attackedSimpleAnim = targetServant.onAttacked;
            }
            if (attackObj && attackedObj)
            {
                attackObj.positionLerp.setStart();
                attackObj.positionLerp.setTarget(attackedObj.transform, table.TableToWorldDistance(150));
            }


            if (attackObj && attackSimpleAnim != null)
            {
                while (!SimpleAnimHelper.update(table, ref _attackAnim, attackSimpleAnim, attackObj.animator, _deltaTime))
                    yield return null;
            }
            _attacked = true;
            if (attackedObj && attackedSimpleAnim != null)
            {
                while (!SimpleAnimHelper.update(table, ref _attackedAnim, attackedSimpleAnim, attackedObj.animator, _deltaTime))
                    yield return null;
            }
        }
        private bool waitingOtherAnim(BattleTableManager table)
        {
            var group = table.getAnimGroupOfAnim(this);
            var nextGroup = table.getNextGroupOfAnimGroup(group);
            return nextGroup != null && !isAnimGroupDamageOfThis(nextGroup);
        }
        private bool isAnimGroupDamageOfThis(AnimationGroup nextGroup)
        {
            return nextGroup.Any(a => a is DamageAnimation damageAnim && damageAnim.tEventArg.parent == tEventArg);
        }
        public override bool continueNextGroup(AnimationGroup nextGroup)
        {
            if (_waitingOtherAnims && !isAnimGroupDamageOfThis(nextGroup))
                return true;
            if (_attacked)
                return true;
            return base.continueNextGroup(nextGroup);
        }
    }
}