using System;
using System.Collections.Generic;
using System.Linq;
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
        public static bool isActiveEffect(this Effect effect)
        {
            return effect is GeneratedActiveEffect;
        }

        public static Effect[] getAuras(this IEnumerable<Effect> effects)
        {
            return effects.Where(e => e.isAura()).OrderByDescending(e => e.getPriority()).ToArray();
        }
        public static Effect[] getBattlecries(this IEnumerable<Effect> effects)
        {
            return effects.Where(e => e.isBattlecry()).OrderByDescending(e => e.getPriority()).ToArray();
        }
        public static Effect[] getActiveEffects(this IEnumerable<Effect> effects)
        {
            return effects.Where(e => e.isActiveEffect()).OrderByDescending(e => e.getPriority()).ToArray();
        }
        public static bool hasBattlecry(this IEnumerable<Effect> effects)
        {
            return effects.Any(e => e.isBattlecry());
        }
        public static Effect[] getDeathRattles(this IEnumerable<Effect> effects)
        {
            return effects.Where(e => e.isDeathRattle()).OrderByDescending(e => e.getPriority()).ToArray();
        }
        public static bool hasDeathRattles(this IEnumerable<Effect> effects)
        {
            return effects.Any(e => e.isDeathRattle());
        }
        public static bool hasTrap(this IEnumerable<Effect> effects)
        {
            return effects.Any(e => e.isTrap());
        }


        /// <summary>
        /// 根据已经选择的目标卡牌，获取一个卡牌效果接下来可以选择什么卡牌。
        /// </summary>
        /// <param name="effect"></param>
        /// <param name="game"></param>
        /// <param name="source"></param>
        /// <param name="buff"></param>
        /// <param name="selectedTargets"></param>
        /// <returns></returns>
        public static Card[] getAvaliableTargets(this Effect effect, CardEngine game, Card source, Buff buff, params Card[] selectedTargets)
        {
            if (effect is not GeneratedActiveEffect generatedEffect)
                return Array.Empty<Card>();

            var targetIndex = selectedTargets?.Length ?? 0;
            if (!generatedEffect.hasTargetIndex(targetIndex))
                return Array.Empty<Card>();

            var targetChecker = generatedEffect.targetCheckers.ElementAtOrDefault(targetIndex);
            if (targetChecker == null)
                return Array.Empty<Card>();

            List<Card> targetList = new List<Card>();
            foreach (Player player in game.players)
            {
                if (targetChecker.targetType == DEFINE.TARGET_TYPE_CHARACTER)
                {
                    if (source.isValidTarget(game, buff, player.getMaster(), generatedEffect, selectedTargets))
                        targetList.Add(player.getMaster());
                }
                if (targetChecker.targetType == DEFINE.TARGET_TYPE_CHARACTER || targetChecker.targetType == DEFINE.TARGET_TYPE_SERVANT)
                {
                    foreach (Card servant in player.getField())
                    {
                        if (source.isValidTarget(game, buff, servant, generatedEffect, selectedTargets))
                            targetList.Add(servant);
                    }
                }
            }
            return targetList.ToArray();
        }
        /// <summary>
        /// 检测一个卡牌效果能否完全选择所有目标。
        /// </summary>
        /// <param name="effect"></param>
        /// <param name="game"></param>
        /// <param name="source"></param>
        /// <param name="buff"></param>
        /// <returns></returns>
        public static bool hasFullTargets(this Effect effect, CardEngine game, Card source, Buff buff)
        {
            var avaliableTargets = effect.getAvaliableTargets(game, source, buff);
            var targetCheckerCount = effect.getTargetCheckerCount();
            var chains = new List<List<Card>>();
            for (int i = 0; i < avaliableTargets.Length; i++)
            {
                var chain = new List<Card>();
                var target = avaliableTargets[i];
                effect.getPossibleTargetChain(game, source, target, buff, chain, chains);

                if (chains.Count > 0)
                    return true;
            }
            return false;
        }
        /// <summary>
        /// 获得一个效果的最大目标选择数量。
        /// </summary>
        /// <param name="effect"></param>
        /// <returns></returns>
        public static int getTargetCheckerCount(this Effect effect)
        {
            if (effect is not GeneratedActiveEffect activeEffect)
                return 0;
            return activeEffect.targetCheckers?.Count() ?? 0;
        }
        private static void getPossibleTargetChain(this Effect effect, CardEngine game, Card source, Card target, Buff buff, List<Card> currentChain, List<List<Card>> chains)
        {
            currentChain.Add(target);
            int currentIndex = currentChain.Count;
            if (currentIndex >= effect.getTargetCheckerCount())
            {
                // 没有子级了
                chains.Add(currentChain);
                return;
            }

            // 还有子级
            var avaliableTargets = effect.getAvaliableTargets(game, source, buff, currentChain.ToArray());
            for (int i = 0; i < avaliableTargets.Length; i++)
            {
                var newChain = new List<Card>(currentChain);
                var newTarget = avaliableTargets[i];
                effect.getPossibleTargetChain(game, source, newTarget, buff, newChain, chains);
            }
        }
    }
}