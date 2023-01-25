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
            return effect is DeathRattle || effect is GeneratedDeathRattle || (effect is GeneratedEffect gen && gen.tagList.Contains(KEYWORD.DEATHRATTLE));
        }
        public static bool isBattlecry(this IEffect effect)
        {
            return effect is GeneratedBattlecry || (effect is GeneratedEffect gen && gen.tagList.Contains(KEYWORD.BATTLECRY));
        }
    }
}