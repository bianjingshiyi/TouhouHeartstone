using System.Collections;
using TouhouCardEngine;
using TouhouHeartstone;
using UI;

namespace Game.BattleTable
{
    using Arg = THHCard.AttackEventArg;
    class AttackAnimation : EventAnimation<Arg>
    {
        AnimAnim _attackAnim;
        AnimAnim _attackedAnim;
        public override IEnumerator update(BattleTableManager table, EventRecord record, Arg arg)
        {
            yield return pauseWhileNotCompleted();
            yield return stopWhenCanceled();

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
                attackObj.positionLerp.setTarget(attackedObj.transform, 150);
            }


            if (attackObj && attackSimpleAnim != null)
            {
                while (!SimpleAnimHelper.update(table, ref _attackAnim, attackSimpleAnim, attackObj.animator, _deltaTime))
                    yield return null;
            }
            if (attackedObj && attackedSimpleAnim != null)
            {
                while (!SimpleAnimHelper.update(table, ref _attackedAnim, attackedSimpleAnim, attackedObj.animator, _deltaTime))
                    yield return null;
            }
        }
        public override bool blockAnim(UIAnimation nextAnim)
        {
            if (_attackAnim == null && nextAnim is DamageAnimation)
                return false;
            return base.blockAnim(nextAnim);
        }
    }
}