using TouhouHeartstone;
using BJSYGameCore;
using UnityEngine;
using UI;
namespace Game
{
    class AttackAnimation : EventAnimation<THHCard.AttackEventArg>
    {
        AnimAnim _attackAnim;
        AnimAnim _attackedAnim;
        public override bool update(TableManager table, THHCard.AttackEventArg eventArg)
        {
            if (eventArg.isCanceled)
                return true;
            if (table.tryGetMaster(eventArg.card, out var attackMaster))
            {
                if (table.tryGetMaster(eventArg.target, out var targetMaster))
                {
                    attackMaster.GetComponentInChildren<PositionLerp>().setTarget(targetMaster.rectTransform, targetMaster.rectTransform.sizeDelta.x / 2);
                    if (!SimpleAnimHelper.update(table, ref _attackAnim, attackMaster.onAttack, attackMaster.animator))
                        return false;
                    if (!SimpleAnimHelper.update(table, ref _attackedAnim, targetMaster.onAttacked, targetMaster.animator))
                        return false;
                }
                else if (table.tryGetServant(eventArg.target, out var targetServant))
                {
                    attackMaster.GetComponentInChildren<PositionLerp>().setTarget(targetServant.rectTransform, targetServant.rectTransform.sizeDelta.x / 2);
                    if (!SimpleAnimHelper.update(table, ref _attackAnim, attackMaster.onAttack, attackMaster.animator))
                        return false;
                    if (!SimpleAnimHelper.update(table, ref _attackedAnim, targetServant.onAttacked, targetServant.animator))
                        return false;
                }
                table.setMaster(attackMaster, eventArg.card);
            }
            else if (table.getServant(eventArg.card) is Servant attackServant)
            {
                if (table.tryGetMaster(eventArg.target, out var targetMaster))
                {
                    attackServant.GetComponentInChildren<PositionLerp>().setTarget(targetMaster.rectTransform, targetMaster.rectTransform.sizeDelta.x / 2);
                    if (!SimpleAnimHelper.update(table, ref _attackAnim, attackServant.onAttack, attackServant.animator))
                        return false;
                    if (!SimpleAnimHelper.update(table, ref _attackedAnim, targetMaster.onAttacked, targetMaster.animator))
                        return false;
                }
                else if (table.tryGetServant(eventArg.target, out var targetServant))
                {
                    attackServant.GetComponentInChildren<PositionLerp>().setTarget(targetServant.rectTransform, targetServant.rectTransform.sizeDelta.x / 2);
                    if (!SimpleAnimHelper.update(table, ref _attackAnim, attackServant.onAttack, attackServant.animator))
                        return false;
                    if (!SimpleAnimHelper.update(table, ref _attackedAnim, targetServant.onAttacked, targetServant.animator))
                        return false;
                }
                table.setServant(attackServant, eventArg.card);
            }
            return true;
        }
        public override bool blockAnim(UIAnimation nextAnim)
        {
            if (/*_timer2.isStarted &&*/ _attackAnim == null && nextAnim is DamageAnimation)
                return false;
            return base.blockAnim(nextAnim);
        }
    }
}