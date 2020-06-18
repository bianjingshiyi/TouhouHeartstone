using TouhouHeartstone;
using BJSYGameCore;
using BJSYGameCore.UI;
using UnityEngine;
namespace UI
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
    class ServantAttackAnimation : UIAnimation<THHCard.AttackEventArg>
    {
        public ServantAttackAnimation(THHCard.AttackEventArg eventArg) : base(eventArg)
        {
        }
        Timer _timer1 = new Timer() { duration = .5f };
        Timer _timer2 = new Timer() { duration = .5f };
        public override bool update(Table table)
        {
            if (table.getServant(eventArg.card) is Servant attackServant)
            {
                if (table.getMaster(eventArg.target) is Master targetMaster)
                {
                    if (!_timer1.isStarted)
                        _timer1.start();
                    attackServant.getChild("Root").position =
                        Vector3.Lerp(attackServant.rectTransform.position, targetMaster.rectTransform.position,
                        attackServant.attackAnimationCurve.Evaluate(_timer1.progress));
                    if (!_timer1.isExpired())
                        return false;
                    if (!_timer2.isStarted)
                        _timer2.start();
                    attackServant.getChild("Root").position =
                        Vector3.Lerp(targetMaster.rectTransform.position, attackServant.rectTransform.position,
                        attackServant.attackAnimationCurve.Evaluate(_timer2.progress));
                    if (!_timer2.isExpired())
                        return false;
                }
                else if (table.getServant(eventArg.target) is Servant targetServant)
                {
                    if (!_timer1.isStarted)
                        _timer1.start();
                    attackServant.getChild("Root").position =
                        Vector3.Lerp(attackServant.rectTransform.position, targetServant.rectTransform.position,
                        attackServant.attackAnimationCurve.Evaluate(_timer1.progress));
                    if (!_timer1.isExpired())
                        return false;
                    if (!_timer2.isStarted)
                        _timer2.start();
                    attackServant.getChild("Root").position =
                        Vector3.Lerp(targetServant.rectTransform.position, attackServant.rectTransform.position,
                        attackServant.attackAnimationCurve.Evaluate(_timer2.progress));
                    if (!_timer2.isExpired())
                        return false;
                }
                attackServant.update(attackServant.card.owner as THHPlayer, attackServant.card, table.getSkin(attackServant.card));
            }
            return true;
        }
        public override bool blockAnim(UIAnimation nextAnim)
        {
            if (_timer2.isStarted && nextAnim is DamageAnimation)
                return false;
            return base.blockAnim(nextAnim);
        }
    }
}