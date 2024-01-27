using System.Collections.Generic;
using TouhouCardEngine;
using TouhouCardEngine.Interfaces;

namespace TouhouHeartstone
{
    public static class EffectHelper
    {
        public static bool isAura(this Effect effect)
        {
            return effect is GeneratedAura;
        }
        public static bool isDeathRattle(this Effect effect)
        {
            return effect is GeneratedPassiveEffect passive && passive.hasTag(EFFECTTAG.PASSIVE_LASTWORD);
        }
        public static bool isTrap(this Effect effect)
        {
            return effect is GeneratedEventEffect passive && passive.hasTag(EFFECTTAG.EVENT_TRAP);
        }
        public static bool isBattlecry(this Effect effect)
        {
            return effect is GeneratedActiveEffect active && active.hasTag(EFFECTTAG.ACTIVE_PLAYED);
        }
        public static bool isSpellActiveEffect(this Effect effect)
        {
            return effect is GeneratedActiveEffect;
        }

        /// <summary>
        /// 这个效果的可选目标
        /// </summary>
        /// <param name="effect"></param>
        /// <param name="game"></param>
        /// <returns></returns>
        public static Card[] getAvaliableTargets(this Effect effect, CardEngine game, Card source)
        {
            List<Card> targetList = new List<Card>();
            if (effect is GeneratedActiveEffect generatedEffect)
            {
                var entryNode = generatedEffect.entryNode;
                if (entryNode != null && entryNode.targetCheckerList != null && entryNode.targetCheckerList.Count > 0)
                {
                    foreach (TargetChecker targetChecker in entryNode.targetCheckerList)
                    {
                        foreach (Player player in game.players)
                        {
                            if (targetChecker.targetType == DEFINE.TARGET_TYPE_CHARACTER)
                            {
                                if (source.isValidTarget(game, player.getMaster(), generatedEffect))
                                    targetList.Add(player.getMaster());
                            }
                            if (targetChecker.targetType == DEFINE.TARGET_TYPE_CHARACTER ||
                                targetChecker.targetType == DEFINE.TARGET_TYPE_SERVANT)
                            {
                                foreach (Card servant in player.getField())
                                {
                                    if (source.isValidTarget(game, servant, generatedEffect))
                                        targetList.Add(servant);
                                }
                            }
                        }
                    }
                }
            }
            return targetList.ToArray();
        }
    }
}