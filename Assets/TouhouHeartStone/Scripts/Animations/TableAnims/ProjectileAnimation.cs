using BJSYGameCore.UI;
using UI;
using UnityEngine;
namespace Game
{
    class ProjectileAnimation : TableAnimation
    {
        Projectile projectile { get; }
        public ProjectileAnimation(Projectile projectile, UIObject target)
        {
            this.projectile = projectile;
            projectile.target = target.rectTransform;
        }
        public override bool update(TableManager table)
        {
            if (projectile != null)
                return false;
            return true;
        }
    }
}