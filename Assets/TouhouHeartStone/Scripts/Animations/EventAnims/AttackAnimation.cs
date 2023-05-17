using System.Collections;
using BJSYGameCore.UI;
using TouhouCardEngine;
using TouhouHeartstone;
using UI;

namespace Game
{
    using Arg = THHCard.AttackEventArg;
    class AttackAnimation : EventAnimation<Arg>
    {
        AnimAnim _attackAnim;
        AnimAnim _attackedAnim;
        public override IEnumerator update(TableManager table, EventRecord record, Arg arg)
        {
            while (!record.isCompleted)
                yield return null;
            if (record.isCanceled)
                yield break;

            var card = arg.card;
            var target = arg.target;
            SimpleAnim attackSimpleAnim = null;
            SimpleAnim attackedSimpleAnim = null;

            UIObject attackObj = null;
            UIObject attackedObj = null;

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
                attackObj.GetComponentInChildren<PositionLerp>().setTarget(attackedObj.rectTransform, attackedObj.rectTransform.sizeDelta.x / 2);


            if (attackObj && attackSimpleAnim != null)
            {
                while (!SimpleAnimHelper.update(table, ref _attackAnim, attackSimpleAnim, attackObj.animator, _time))
                    yield return null;
            }
            if (attackedObj && attackedSimpleAnim != null)
            {
                while (!SimpleAnimHelper.update(table, ref _attackedAnim, attackedSimpleAnim, attackedObj.animator, _time))
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