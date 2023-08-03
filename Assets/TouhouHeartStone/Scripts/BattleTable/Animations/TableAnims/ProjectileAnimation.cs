using UI;
using UnityEngine;

namespace Game.BattleTable
{
    class ProjectileAnimation : TableAnimation
    {
        Projectile projectile { get; }
        public ProjectileAnimation(Projectile projectile, Transform target)
        {
            this.projectile = projectile;
            projectile.target = target;
        }
        public override bool update(BattleTableManager table, float time)
        {
            if (projectile != null)
                return false;
            return true;
        }
    }
}