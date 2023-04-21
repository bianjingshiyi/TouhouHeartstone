using TouhouCardEngine;
using TouhouCardEngine.Interfaces;

namespace TouhouHeartstone
{
    public static class EffectHelper
    {
        public static bool isAura(this IEffect effect)
        {
            return effect is GeneratedAura;
        }
        public static bool isDeathRattle(this IEffect effect)
        {
            return effect is DeathRattle || (effect is GeneratedPassiveEffect passive && passive.hasTag(EFFECTTAG.PASSIVE_LASTWORD));
        }
        public static bool isBattlecry(this IEffect effect)
        {
            return effect is GeneratedActiveEffect active && active.hasTag(EFFECTTAG.ACTIVE_PLAYED);
        }
    }
}