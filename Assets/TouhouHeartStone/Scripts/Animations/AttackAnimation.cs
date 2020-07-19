using TouhouHeartstone;
using BJSYGameCore;
using BJSYGameCore.UI;
using UnityEngine;
using UI;
namespace Game
{
    class ProjectileAnimation : UIAnimation
    {
        Projectile projectile { get; }
        public ProjectileAnimation(Projectile projectile, UIObject target)
        {
            this.projectile = projectile;
            projectile.target = target.rectTransform;
        }
        public override bool update(Table table)
        {
            if (projectile != null)
                return false;
            return true;
        }
    }
    class AttackAnimation : EventAnimation<THHCard.AttackEventArg>
    {
        Timer _timer1 = new Timer() { duration = .5f };
        Timer _timer2 = new Timer() { duration = .5f };
        AnimAnim _attackAnim;
        AnimAnim _attackedAnim;
        public override bool update(TableManager table, THHCard.AttackEventArg eventArg)
        {
            if (table.tryGetMaster(eventArg.card, out var attackMaster))
            {
                if (table.tryGetMaster(eventArg.target, out var targetMaster))
                {
                    attackMaster.GetComponentInChildren<PositionLerp>().targetTransofrm = targetMaster.rectTransform;
                    if (!SimpleAnimHelper.update(table, ref _attackAnim, attackMaster.onAttack, attackMaster.animator))
                        return false;
                    if (!SimpleAnimHelper.update(table, ref _attackedAnim, targetMaster.onAttacked, targetMaster.animator))
                        return false;
                }
                else if (table.tryGetServant(eventArg.target, out var targetServant))
                {
                    attackMaster.GetComponentInChildren<PositionLerp>().targetTransofrm = targetServant.rectTransform;
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
                    attackServant.GetComponentInChildren<PositionLerp>().targetTransofrm = targetMaster.rectTransform;
                    if (!SimpleAnimHelper.update(table, ref _attackAnim, attackServant.onAttack, attackServant.animator))
                        return false;
                    if (!SimpleAnimHelper.update(table, ref _attackedAnim, targetMaster.onAttacked, targetMaster.animator))
                        return false;
                }
                else if (table.tryGetServant(eventArg.target, out var targetServant))
                {
                    attackServant.GetComponentInChildren<PositionLerp>().targetTransofrm = targetServant.rectTransform;
                    if (!SimpleAnimHelper.update(table, ref _attackAnim, attackServant.onAttack, attackServant.animator))
                        return false;
                    if (!SimpleAnimHelper.update(table, ref _attackedAnim, targetServant.onAttacked, targetServant.animator))
                        return false;
                }
                //if (table.tryGetMaster(eventArg.target, out var targetMaster))
                //{
                //    if (!_timer1.isStarted)
                //        _timer1.start();
                //    attackServant.getChild("Root").position =
                //        Vector3.Lerp(attackServant.rectTransform.position, targetMaster.rectTransform.position,
                //        attackServant.attackAnimationCurve.Evaluate(_timer1.progress));
                //    if (!_timer1.isExpired())
                //        return false;
                //    if (!_timer2.isStarted)
                //        _timer2.start();
                //    attackServant.getChild("Root").position =
                //        Vector3.Lerp(targetMaster.rectTransform.position, attackServant.rectTransform.position,
                //        attackServant.attackAnimationCurve.Evaluate(_timer2.progress));
                //    if (!_timer2.isExpired())
                //        return false;
                //}
                //else if (table.getServant(eventArg.target) is Servant targetServant)
                //{
                //    if (!_timer1.isStarted)
                //        _timer1.start();
                //    attackServant.getChild("Root").position =
                //        Vector3.Lerp(attackServant.rectTransform.position, targetServant.rectTransform.position,
                //        attackServant.attackAnimationCurve.Evaluate(_timer1.progress));
                //    if (!_timer1.isExpired())
                //        return false;
                //    if (!_timer2.isStarted)
                //        _timer2.start();
                //    attackServant.getChild("Root").position =
                //        Vector3.Lerp(targetServant.rectTransform.position, attackServant.rectTransform.position,
                //        attackServant.attackAnimationCurve.Evaluate(_timer2.progress));
                //    if (!_timer2.isExpired())
                //        return false;
                //}
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