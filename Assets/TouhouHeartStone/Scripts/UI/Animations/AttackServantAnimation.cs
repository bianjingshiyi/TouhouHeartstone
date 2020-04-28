using TouhouHeartstone;
using BJSYGameCore;
using BJSYGameCore.UI;
using UnityEngine;
namespace UI
{
    class ProjectileAnimation : Animation
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
    class AttackServantAnimation : Animation<THHCard.AttackEventArg>
    {
        public override THHCard.AttackEventArg eventArg { get; }
        public AttackServantAnimation(THHCard.AttackEventArg eventArg)
        {
            this.eventArg = eventArg;
        }
        Timer _timer1 = new Timer() { duration = .5f };
        Timer _timer2 = new Timer() { duration = .5f };
        public override bool update(Table table)
        {
            if (!_timer1.isStarted)
                _timer1.start();
            if (!_timer1.isExpired())
            {
                if (table.getServant(eventArg.card) is Servant attackServant)
                {
                    if (table.getMaster(eventArg.target) is Master targetMaster)
                    {
                        attackServant.getChild("Root").position =
                            Vector3.Lerp(attackServant.rectTransform.position, targetMaster.rectTransform.position,
                            attackServant.attackAnimationCurve.Evaluate(_timer1.progress));
                    }
                    else if (table.getServant(eventArg.target) is Servant targetServant)
                    {
                        attackServant.getChild("Root").position =
                            Vector3.Lerp(attackServant.rectTransform.position, targetServant.rectTransform.position,
                            attackServant.attackAnimationCurve.Evaluate(_timer1.progress));
                    }
                }
                return false;
            }
            if (!_timer2.isStarted)
                _timer2.start();
            if (!_timer2.isExpired())
            {
                if (table.getServant(eventArg.card) is Servant attackServant)
                {
                    if (table.getMaster(eventArg.target) is Master targetMaster)
                    {
                        attackServant.getChild("Root").position =
                            Vector3.Lerp(targetMaster.rectTransform.position, attackServant.rectTransform.position,
                            attackServant.attackAnimationCurve.Evaluate(_timer2.progress));
                    }
                    else if (table.getServant(eventArg.target) is Servant targetServant)
                    {
                        attackServant.getChild("Root").position =
                            Vector3.Lerp(targetServant.rectTransform.position, attackServant.rectTransform.position,
                            attackServant.attackAnimationCurve.Evaluate(_timer2.progress));
                    }
                }
                return false;
            }
            return true;
        }
    }
}
