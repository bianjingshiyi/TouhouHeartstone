using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TouhouCardEngine;
using TouhouCardEngine.Interfaces;
using TouhouHeartstone;
using UnityEngine;
using Card = TouhouCardEngine.Card;

namespace Game
{
    public class AI
    {
        CardEngine game { get; }
        Player player { get; }

        private bool dummy { get; }

        public AI(CardEngine game, Player player, bool autoRun = true, bool dummy = false)
        {
            this.game = game;
            this.player = player;
            this.dummy = dummy;
            if (autoRun)
                run(game);
        }

        public void run(CardEngine game)
        {
            game.answers.onRequest += Answers_onRequest;
        }

        public void stop(CardEngine game)
        {
            game.answers.onRequest -= Answers_onRequest;
        }

        private async void Answers_onRequest(IRequest request)
        {
            if (game.answers == null)
                return;
            if (!request.playersId.Contains(player.id))
            {
                return;
            }

            await Task.Delay(1000);
            switch (request)
            {
                case InitReplaceRequest:
                    IResponse resp = new InitReplaceResponse()
                    {
                        cardsId = new int[0]
                    };
                    game.logPlayerAction(player.id, resp);
                    _ = game.answers.answer(player.id, resp);
                    break;
                case FreeActRequest:
                    resp = dummy ? new TurnEndResponse() : calcNextAction(game, player);
                    game.logPlayerAction(player.id, resp);
                    _ = game.answers.answer(player.id, resp);
                    break;
                case DiscoverRequest discover:
                    resp = discover.getDefaultResponse(game, player.id);
                    game.logPlayerAction(player.id, resp);
                    _ = game.answers.answer(player.id, resp);
                    break;
                default:
                    UberDebug.LogChannel("AI", $"AI未处理的询问：{request}");
                    break;
            }
        }

        public Response calcNextAction(CardEngine game, Player player)
        {
            try
            {
                var actions = calcActions(game, player).Where(p => p.Value > 0);
                if (actions.Any())
                    return actions.OrderByDescending(p => p.Value).First().Key;
                else
                    return new TurnEndResponse();
            }
            catch (Exception e)
            {
                game.logger.logError("AI", $"AI只能空过了，因为它触发了异常：{e}");
                return new TurnEndResponse();
            }
        }

        /// <summary>
        /// 计算出可以选择的行动，不包括结束回合。
        /// </summary>
        /// <param name="game"></param>
        /// <param name="player"></param>
        /// <returns></returns>
        public Dictionary<Response, float> calcActions(CardEngine game, Player player)
        {
            Dictionary<Response, float> responseDic = new Dictionary<Response, float>();
            Player opponent = game.getOpponent(player);
            // 技能
            var selfSkill = player.getSkill();
            if (selfSkill.isUsable(game, player, out _))
            {
                addUseResponse(selfSkill);
            }

            // 手牌
            foreach (var card in player.getHand())
            {
                if (!card.isUsable(game, player, out _))
                    continue;
                if (card.isServant())
                {
                    for (int i = 0; i < player.getField().count + 1; i++)
                    {
                        addUseResponse(card, i);
                    }
                }
                else
                {
                    addUseResponse(card);
                }
            }

            // 英雄攻击。
            var selfMaster = player.getMaster();
            addAttackResponse(selfMaster);

            // 仆从攻击。
            foreach (var servant in player.getField())
            {
                addAttackResponse(servant);
            }

            return responseDic;

            // 内嵌方法
            void addUseResponse(Card card, int position = 0)
            {
                if (card.isNeedTarget(game))
                {
                    foreach (var target in card.getAvaliableTargets(game).Where(t => card.isValidTarget(game, t)))
                    {
                        var use = new UseResponse() { cardId = card.id, position = position, targetsId = new int[] { target.id } };
                        responseDic.Add(use, calcUseValue(use, game, player, game.getOpponent(player)));
                    }
                }
                else
                {
                    var use = new UseResponse() { cardId = card.id, position = position };
                    responseDic.Add(use, calcUseValue(use, game, player, game.getOpponent(player)));
                }
            }
            void addAttackResponse(Card card)
            {
                if (!card.canAttack(game))
                    return;
                var enemyMaster = opponent.getMaster();
                var validTargets = new List<Card>();
                // 攻击敌方英雄。
                if (card.isAttackable(game, enemyMaster, out _))
                {
                    validTargets.Add(enemyMaster);
                }

                // 攻击敌方仆从。
                foreach (var target in opponent.getField())
                {
                    if (!card.isAttackable(game, target, out _))
                        continue;
                    validTargets.Add(target);
                }
                var attackEarns = validTargets.Select(t => (calcAttackValue(card, t), t));
                var bestAttackTarget = attackEarns.OrderByDescending(p => p.Item1).FirstOrDefault();
                var attackRes = new AttackResponse() { cardId = card.id, targetId = bestAttackTarget.Item2.id };
                responseDic.Add(attackRes, bestAttackTarget.Item1);
            }
        }

        #region 使用优先级
        float calcUseValue(Response response, CardEngine game, Player player, Player opponent)
        {
            if (response is UseResponse use)
            {
                Card card = game.getCard(use.cardId);
                if (card.define.cardPoolId == CardManager.STANDARD_POOL_ID) // 是经典卡池的
                {
                    float value = 0;
                    Card target = use.targetsId.Length > 0 ? game.getCard(use.targetsId[0]) : null;
                    if (calcNeutralValue(game, player, card, opponent, target, out value))
                        return value;
                    if (calcPatchouliValue(game, player, card, opponent, target, out value))
                        return value;
                    if (calcYoumuValue(game, player, card, opponent, target, out value))
                        return value;
                    if (calcReisenValue(game, player, card, opponent, target, out value))
                        return value;
                    if (calcSanaeValue(game, player, card, opponent, target, out value))
                        return value;
                    if (calcBattlecryValue(game, player, card, opponent, target, out value))
                        return value;
                }
                if (card.isServant())
                {
                    return calcServantValue(card.getAttack(game), card.getLife(game));
                }
                else if (card.isItem())
                {
                    return calcWeaponValue(card.getAttack(game), card.getLife(game));
                }
                return card.getCost(game) + 1;
            }
            return 0;
        }

        bool calcNeutralValue(CardEngine game, Player player, Card card, Player oppo, Card target, out float value)
        {
            switch (card.define.id)
            {
                case ClassicCardID.LuckyCoin:
                    value = 1;
                    return true;
                case ClassicCardID.Neutral_73:
                    value = 1;
                    return true;
                case ClassicCardID.Neutral_74:
                    value = calcHealingValue(game, player, oppo, target, 2);
                    return true;
                default:
                    value = 0;
                    return false;
            }
        }

        bool calcYoumuValue(CardEngine game, Player player, Card card, Player oppo, Card target, out float value)
        {
            switch (card.define.id)
            {
                case ClassicCardID.StrangeHalfbody:
                    value = 2;
                    return true;
                case ClassicCardID.SlashOfPresentWorld:
                    value = card.isCharged(game)
                        ? calcBuffValue(game, player, oppo, player.getMaster(), 3, 0)
                        : calcBuffValue(game, player, oppo, player.getMaster(), 2, 0);
                    return true;
                case ClassicCardID.Meditation:
                    value = card.isCharged(game) ? 3 : 2;
                    return true;
                case ClassicCardID.FantasyOfEnteringEnlightenment:
                    value = card.isCharged(game)
                        ? calcBuffValue(game, player, oppo, player.getMaster(), 0, 6)
                        : 0;
                    return true;
                case ClassicCardID.HalfPhantonSlam:
                    value = calcDamageValue(game, player, oppo, target, player.getMaster().getArmor(game));
                    return true;
                case ClassicCardID.Roukanken:
                    value = player.getItem() == null
                        ? calcWeaponValue(game, card)
                        : calcWeaponValue(game, card) - calcWeaponValue(player.getItem().getAttack(game),
                            player.getItem().getCurrentLife(game));
                    return true;
                case ClassicCardID.TwoHundredYojanaOneSlash:
                    value = card.isCharged(game) ? 4 : 2;
                    return true;
                case ClassicCardID.CherryBlossomFlashing:
                    value = calcAOEDamageValue(game, player, oppo, oppo.getField(), player.getMaster().getAttack(game));
                    return true;
                case ClassicCardID.FiveSignsOfTheDyingDeva:
                    value = card.isCharged(game)
                        ? calcBuffValue(game, player, oppo, player.getMaster(), 7, 0)
                        : calcBuffValue(game, player, oppo, player.getMaster(), 5, 0);
                    return true;
                case ClassicCardID.Hakurouken:
                    value = player.getItem() == null
                        ? calcWeaponValue(game, card)
                        : calcBuffValue(game, player, oppo, player.getItem(), 0, card.getLife(game));
                    return true;
                default:
                    value = 0;
                    return false;
            }
        }

        bool calcPatchouliValue(CardEngine game, Player player, Card card, Player opponent, Card target,
            out float value)
        {
            switch (card.define.id)
            {
                case ClassicCardID.SummerFire:
                    value = calcDamageValue(game, player, opponent, target, 1);
                    return true;
                case ClassicCardID.AutumnEdge:
                    value = calcRandomDamageValue(game, player, opponent, opponent.getField(), 2);
                    return true;
                case ClassicCardID.SpringWind:
                    value = calcBuffValue(game, player, opponent, target, 0, 2);
                    return true;
                case ClassicCardID.WinterElement:
                    value = calcFreezeValue(game, player, opponent, target);
                    return true;
                case ClassicCardID.SummerRed:
                    value = calcBuffValue(game, player, opponent, target, 2, 0);
                    return true;
                case ClassicCardID.DoyouSpear:
                    value = calcServantValue(1, 1);
                    return true;
                case ClassicCardID.MultiCast:
                    value = player.getHand().Where(c => c.isSpell() && c.getCost(game) > 1).Count() > 0 ? 2 : 0;
                    return true;
                case ClassicCardID.TheGreatLibrary:
                    value = 1;
                    return true;
                case ClassicCardID.BestMagic:
                    value = 5;
                    return true;
                case ClassicCardID.ArcaneKnowledge:
                    value = 4;
                    return true;
                case ClassicCardID.PhilosopherStone:
                    value = 2;
                    return true;
                case ClassicCardID.MetalFatigue:
                    value = calcAOEDamageValue(game, player, opponent, opponent.getField(), 3);
                    return true;
                case ClassicCardID.SylphyHorn:
                    value = calcBuffValue(game, player, opponent, target, 3, 6);
                    return true;
                case ClassicCardID.PrincessUndine:
                    value = calcAOEDamageValue(game, player, opponent, player.getField(), 3);
                    return true;
                case ClassicCardID.AgniShine:
                    value = calcDamageValue(game, player, opponent, target, 7);
                    return true;
                case ClassicCardID.TrilithonShake:
                    value = calcServantValue(3, 9);
                    return true;
                case ClassicCardID.RoyalFlare:
                    value = calcDamageValue(game, player, opponent, opponent.getMaster(), 15);
                    return true;
                case ClassicCardID.SilentSelene:
                    value = -calcServantsValue(game, player, opponent, opponent.getField());
                    return true;
                case ClassicCardID.ElementalHarvester:
                    value = calcAOEDamageValue(game, player, opponent, opponent.getField(), 2);
                    return true;
                case ClassicCardID.BurgeoningRise:
                    value = calcBuffValue(game, player, opponent, target, 3, 3) +
                        calcServantValue(game, player, opponent, target) - 1;
                    return true;
                case ClassicCardID.PhlogisticRain:
                    value = calcRandomDamageValue(game, player, opponent,
                                opponent.getField().Append(opponent.getMaster()), 1) *
                            6;
                    return true;
                case ClassicCardID.StElmoPillar:
                    value = calcDamageValue(game, player, opponent, target, 4) +
                            calcDamageValue(game, player, opponent, opponent.getMaster(), 4);
                    return true;
                case ClassicCardID.NoachianDeluge:
                    value = calcServantValue(2, 2) + 1;
                    return true;
                case ClassicCardID.MercuryPoison:
                    value = calcAOEDamageValue(game, player, opponent, opponent.getField(), 3);
                    return true;
                case ClassicCardID.ForestBlaze:
                    value = calcBuffAllValue(game, player, opponent, player.getField(), 2, 2) +
                            calcRandomDamageValue(game, player, opponent, opponent.getField(), 1) *
                            player.getField().count;
                    return true;
                case ClassicCardID.WaterElf:
                    value = calcHealingValue(game, player, opponent, target,
                                target.getLife(game) - target.getCurrentLife(game)) +
                            calcBuffValue(target.getOwner() == player, target.isMaster(), target.getAttack(game),
                                target.getLife(game), 3, 6);
                    return true;
                case ClassicCardID.LavaCromlech:
                    value = (calcServantValue(2, 2) +
                             calcRandomDamageValue(game, player, opponent, opponent.getField(), 2)) * 3;
                    return true;
                case ClassicCardID.GingerGust:
                    value = (calcServantValue(3, 1) + 1) * opponent.getField().count;
                    return true;
                default:
                    value = 0;
                    return false;
            }
        }

        bool calcReisenValue(CardEngine game, Player player, Card card, Player opponent, Card target,
            out float value)
        {
            switch (card.define.id)
            {
                case ClassicCardID.LunaticShoot:
                    value = calcDamageValue(game, player, opponent, opponent.getMaster(), 2);
                    return true;
                case ClassicCardID.LunaticShoot_MindDropping:
                    value = calcDamageValue(game, player, opponent, target, 2);
                    return true;
                case ClassicCardID.XWave:
                    value = 2 + opponent.getField().count;
                    return true;
                case ClassicCardID.MindStopper:
                    value = 2 + opponent.getField().count == 0 ? 1 : 0;
                    return true;
                case ClassicCardID.MindShaker:
                    value = 2 + opponent.getField().count;
                    return true;
                case ClassicCardID.DisorderEye:
                    value = 2 + opponent.getField().count == 0 ? 0 : 1;
                    return true;
                case ClassicCardID.VisionaryTuning:
                    value = 2 + player.getField().count + player.getMaster().getCurrentLife(game) < 15 ? 2 : 0;
                    return true;
                case ClassicCardID.FieldUltraViolet:
                    value = 1;
                    return true;
                case ClassicCardID.PeerlessPatriotsElixir:
                    value = calcBuffValue(game, player, opponent, target, 3, 3) - calcServantValue(game, player,
                        opponent, target);
                    return true;
                case ClassicCardID.MindExplosion:
                    value = calcDamageValue(game, player, opponent, opponent.getMaster(), 5);
                    return true;
                case ClassicCardID.MindDropping:
                    value = 1;
                    return true;
                case ClassicCardID.LunaticGun:
                    value = player.getItem() == null ? calcWeaponValue(game, card) : 0;
                    return true;
                case ClassicCardID.DisbeliefAspect:
                    value = 1;
                    return true;
                default:
                    value = 0;
                    return false;
            }
        }

        bool calcSanaeValue(CardEngine game, Player player, Card card, Player opponent, Card target,
            out float value)
        {
            switch (card.define.id)
            {
                case ClassicCardID.WindStopMiracle:
                    value = calcHealingValue(game, player, opponent, target, 2);
                    return true;
                case ClassicCardID.StarRitual:
                    value = 0.5f;
                    return true;
                case ClassicCardID.GrayThaumaturgy:
                    value = calcHealingValue(game, player, opponent, target, 10);
                    return true;
                case ClassicCardID.BumperCrop:
                    value = 5 * calcRandomHealingValue(game, player, opponent, game.getAllAllys(player), 1);
                    return true;
                case ClassicCardID.GuestStarsMiracle:
                    value = calcAOEDamageValue(game, player, opponent, opponent.getField(), 2);
                    return true;
                case ClassicCardID.OpenSeaMiracle:
                    value = calcDamageValue(game, player, opponent, target, 2);
                    return true;
                case ClassicCardID.GodWind:
                    value = -calcServantsValue(game, player, opponent, opponent.getField()) -
                            calcServantsValue(game, player, opponent, player.getField());
                    return true;
                case ClassicCardID.ForgottenRecrption:
                    value = 1;
                    return true;
                case ClassicCardID.OmikujiBomb:
                    value = 1;
                    return true;
                case ClassicCardID.Bomb1:
                    value = calcDamageValue(game, player, opponent, target, 6);
                    return true;
                case ClassicCardID.Bomb2:
                    value = calcDamageValue(game, player, opponent, target, 4);
                    return true;
                case ClassicCardID.Bomb3:
                    value = calcRandomDamageValue(game, player, opponent, opponent.getField(), 4);
                    return true;
                case ClassicCardID.Bomb4:
                    value = 6 * calcRandomDamageValue(game, player, opponent,
                        opponent.getField().Concat(player.getField()), 1);
                    return true;
                case ClassicCardID.GodSummon:
                    value = 1;
                    return true;
                case ClassicCardID.PlagueOfMishaguchi:
                    value = calcDamageValue(game, player, opponent, player.getMaster(), target.getLife(game)) +
                            calcServantValue(target.getAttack(game), target.getCurrentLife(game));
                    return true;
                case ClassicCardID.PlagueOfFrogs:
                    value = opponent.getField().count * calcServantValue(2, 2);
                    return true;
                case ClassicCardID.PlagueOfHibernation:
                    value = 1;
                    return true;
                case ClassicCardID.PlagueOfCrops:
                    value = 1;
                    return true;
                default:
                    value = 0;
                    return false;
            }
        }

        bool calcBattlecryValue(CardEngine game, Player player, Card card, Player opponent, Card target,
            out float value)
        {
            value = calcServantValue(game, player, opponent, card);
            switch (card.define.id)
            {
                case ClassicCardID.YagokoroEirin_N:
                    value += calcHealingValue(game, player, opponent, target,
                        Math.Min(target.getCurrentLife(game), 15));
                    return true;
                case ClassicCardID.FlameFairy:
                    value += calcRandomDamageValue(game, player, opponent, opponent.getField(), 1);
                    return true;
                case ClassicCardID.Cirno_N_YYM:
                    value += calcFreezeValue(game, player, opponent, target);
                    return true;
                case ClassicCardID.Cirno_N_HMX:
                    foreach (var neighbor in target.getNearbyCards())
                    {
                        value += calcFreezeValue(game, player, opponent, neighbor);
                    }
                    return true;
                case ClassicCardID.Chen_N:
                    value += calcBuffValue(game, player, opponent, card, 2, 0);
                    // + calcRushValue
                    return true;
                case ClassicCardID.Koakuma_N:
                    value += calcDrawCardsValue(game, player, opponent, 1);
                    return true;
                case ClassicCardID.PatchouliKnowledge_N:
                    value += calcDrawCardsValue(game, player, opponent, 2);
                    return true;
                case ClassicCardID.FlandreScarlet_N:
                    value = calcRandomServantsValue(game, player, opponent, opponent.getField(), 1);
                    return true;
                case ClassicCardID.IzayoiSakuya_N:
                    value += calcDrawCardsValue(game, player, opponent, 1) + 4;
                    return true;
                case ClassicCardID.YakumoRan_N:
                    value += calcSummonValue(player, 4, 3);
                    return true;
                case ClassicCardID.SaigyoujiYuyuko_N:
                    value -= calcServantValue(game, player, opponent, target);
                    return true;
                case ClassicCardID.YakumoYukari_N:
                    value -= calcServantsValue(game, player, opponent, opponent.getField()) +
                             calcServantsValue(game, player, opponent, player.getField());
                    // 加个回手value
                    return true;
                case ClassicCardID.LunasaPrismriver_N:
                    value += calcSummonValue(player, (1, 2), (2, 1));
                    return true;
                case ClassicCardID.MerlinPrismriver_N:
                    value += calcSummonValue(player, (2, 2), (2, 1));
                    return true;
                case ClassicCardID.LyricaPrismriver_N:
                    value += calcSummonValue(player, (2, 2), (1, 2));
                    return true;
                case ClassicCardID.MystiaLorelei_N:
                    value += calcBuffValue(game, player, opponent, target, 1 - target.getAttack(game), 0);
                    return true;
                case ClassicCardID.ReisenUdongeinInaba_N:
                    value += calcDamageValue(game, player, opponent, opponent.getMaster(), 4);
                    return true;
                case ClassicCardID.TewiInaba:
                    value += calcDrawCardsValue(game, player, opponent, 1);
                    return true;
                case ClassicCardID.YoukaiSmith:
                    value += player.getItem()?.getAttack(game) ?? 0;
                    return true;
                case ClassicCardID.Koakuma:
                    value += calcDrawCardsValue(game, player, opponent, 1);
                    return true;
                case ClassicCardID.YasakaKanako:
                    value += 10;
                    return true;
                case ClassicCardID.RemiliaScarlet_N:
                case ClassicCardID.TewiInaba_N:
                case ClassicCardID.KamishirasawaKeine_Human_N:
                case ClassicCardID.KamishirasawaKeine_Beast_N:
                    return true;
                default:
                    value = 0;
                    return false;
            }
        }
        #endregion

        #region 攻击优先级
        float calcAttackValue(Card attacker, Card defender)
        {
            // 攻击本身就能产生收益。
            float earn = 1;
            float penalty = 0;
            var enemyDamage = defender.getAttack(game);


            if (willAttackKill(attacker, defender))
            {
                // 如果能击杀目标。
                if (defender.isMaster())
                {
                    // 攻击英雄。
                    // 击杀敌方英雄，收益为999。
                    earn += 999;
                }
                else
                {
                    // 攻击仆从。
                    // 击杀敌方仆从，收益为这个仆从的价值。
                    earn += calcServantValue(defender.getAttack(game), defender.getLife(game));
                }
            }
            else
            {
                // 否则收益为有效的伤害。
                earn += getEffectiveDamageValue(attacker.getAttack(game), defender);
            }

            // 伤害惩罚。
            int takeDamage = getEffectiveDamageValue(enemyDamage, attacker);
            int damagedLife = getDamagedLife(enemyDamage, attacker);
            if (attacker.isMaster())
            {
                // 英雄受到伤害惩罚。
                // 受到的伤害越大，惩罚越大。
                penalty = takeDamage * 0.5f;
                // 自己被攻击造成伤害后剩余的血量越低，惩罚越大，最高为4倍。
                penalty *= Mathf.Lerp(4, 1, damagedLife / 30f);
                if (damagedLife <= 0)
                {
                    // 致死则不可能攻击。
                    penalty = earn * 4;
                }
            }
            else if (attacker.isServant())
            {
                // 仆从受到伤害惩罚。
                var servantValue = calcServantValue(attacker.getAttack(game), attacker.getCurrentLife(game)) * 0.5f;
                // 致命。
                bool enemyLethal = defender.isPoisonous(game);
                if (enemyLethal && canBeKilledByLethal(enemyDamage, defender))
                {
                    penalty = servantValue;
                }
                else
                {
                    // 受伤后剩余的血量越少，惩罚越大。最高不超过这个仆从本身的价值的一半。
                    penalty = Mathf.Lerp(servantValue, 0, (float)damagedLife / attacker.getLife(game));
                }
            }
            else
            {
                return 0;
            }
            return earn - penalty;
        }
        /// <summary>
        /// 获取一张卡牌的预估受到伤害。
        /// </summary>
        /// <param name="damage"></param>
        /// <param name="defender"></param>
        /// <returns></returns>
        int getEffectiveDamage(int damage, Card defender)
        {
            if (defender.isImmunity(game))
                return 0;
            if (defender.isShield(game))
                return 0;

            return damage - defender.getDamageReduce(game);
        }
        /// <summary>
        /// 获取伤害打在一个卡牌上的有效价值。
        /// </summary>
        /// <param name="damage"></param>
        /// <param name="defender"></param>
        /// <returns></returns>
        int getEffectiveDamageValue(int damage, Card defender)
        {
            if (defender.isShield(game))
                return 1;
            return getEffectiveDamage(damage, defender);
        }
        /// <summary>
        /// 获取一张卡牌在受到伤害后的预估剩余生命值。
        /// </summary>
        /// <param name="damage"></param>
        /// <param name="defender"></param>
        /// <returns></returns>
        int getDamagedLife(int damage, Card defender)
        {
            return defender.getCurrentLife(game) + defender.getArmor(game) - getEffectiveDamage(damage, defender);
        }
        /// <summary>
        /// 伤害能否击杀目标。
        /// </summary>
        /// <param name="damage"></param>
        /// <param name="defender"></param>
        /// <returns></returns>
        bool willDamageKill(int damage, Card defender)
        {
            return getDamagedLife(damage, defender) <= 0;
        }
        bool willAttackKill(Card attacker, Card defender)
        {
            var attack = attacker.getAttack(game);
            if (attacker.isPoisonous(game) && canBeKilledByLethal(attack, defender))
            {
                return true;
            }
            return willDamageKill(attack, defender);
        }
        bool canBeKilledByLethal(int damage, Card defender)
        {
            if (defender.isMaster())
                return false;
            if (defender.isImmunity(game))
                return false;
            if (defender.isShield(game))
                return false;
            if (damage - defender.getDamageReduce(game) <= 0)
                return false;
            return true;
        }
        #endregion

        float calcSummonValue(Player player, int attack, int life)
        {
            if (player.getField().isFull)
            {
                return 0;
            }
            return calcServantValue(attack, life);
        }
        float calcSummonValue(Player player, params (int attack, int life)[] summons)
        {
            var field = player.getField();
            float value = 0;
            for (int i = field.count; i < field.maxCount; i++)
            {
                var summon = summons[i];
                value += calcServantValue(summon.attack, summon.life);
            }
            return value;
        }

        /// <summary>
        /// AI估算随从价值的基础公式
        /// </summary>
        /// <param name="attack"></param>
        /// <param name="life"></param>
        /// <returns></returns>
        float calcServantValue(int attack, int life)
        {
            return Math.Max(attack, 0) * Math.Max(life, 0) + (life > 0 ? 1 : 0);
        }
        /// <summary>
        /// 估算场上某个随从的价值，已区分敌我，召唤使用+，消灭使用-
        /// </summary>
        /// <param name="game"></param>
        /// <param name="player"></param>
        /// <param name="opponent"></param>
        /// <param name="servant"></param>
        /// <returns></returns>
        float calcServantValue(CardEngine game, Player player, Player opponent, Card servant)
        {
            var value = calcServantValue(servant.getAttack(game), servant.getCurrentLife(game));
            return player == servant.getOwner() ? value : -value;
        }

        /// <summary>
        /// 估算场上某些随从的价值，已区分敌我，召唤使用+，消灭使用-
        /// </summary>
        /// <param name="game"></param>
        /// <param name="player"></param>
        /// <param name="opponent"></param>
        /// <param name="servants"></param>
        /// <returns></returns>
        float calcServantsValue(CardEngine game, Player player, Player opponent, IEnumerable<Card> servants)
        {
            return servants.Sum(s => calcServantValue(game, player, opponent, s));
        }

        /// <summary>
        /// 估算随从即时身材变化价值，已区分敌我
        /// </summary>
        /// <param name="isFriendly"></param>
        /// <param name="atkBfr"></param>
        /// <param name="lifeBfr"></param>
        /// <param name="atkAft"></param>
        /// <param name="lifeAft"></param>
        /// <returns></returns>
        float calcInstantServantIntPropChangeValue(bool isFriendly, int atkBfr, int lifeBfr, int atkAft, int lifeAft)
        {
            var value = calcServantValue(atkAft, lifeAft) - calcServantValue(atkBfr, lifeBfr);
            return isFriendly ? value : -value;
        }

        /// <summary>
        /// 估算英雄即时身材变化价值，已区分敌我
        /// </summary>
        /// <param name="isFriendly"></param>
        /// <param name="atkBfr"></param>
        /// <param name="lifeBfr"></param>
        /// <param name="atkAft"></param>
        /// <param name="lifeAft"></param>
        /// <returns></returns>
        float calcInstantMasterIntPropChangeValue(bool isFriendly, int atkBfr, int lifeBfr, int atkAft, int lifeAft)
        {
            var value = calcMasterLifeWeight(lifeAft) - calcMasterLifeWeight(lifeBfr);
            if (isFriendly)
            {
                return value;
            }

            return -value;
        }

        /// <summary>
        /// 估算角色即时身材变化价值，已区分敌我
        /// </summary>
        /// <param name="isFriendly"></param>
        /// <param name="isMaster"></param>
        /// <param name="atkBfr"></param>
        /// <param name="lifeBfr"></param>
        /// <param name="atkAft"></param>
        /// <param name="lifeAft"></param>
        /// <returns></returns>
        float calcInstantIntPropChangeValue(bool isFriendly, bool isMaster, int atkBfr, int lifeBfr, int atkAft,
            int lifeAft)
        {
            return isMaster
                ? calcInstantMasterIntPropChangeValue(isFriendly, atkBfr, lifeBfr, atkAft, lifeAft)
                : calcInstantServantIntPropChangeValue(isFriendly, atkBfr, lifeBfr, atkAft, lifeAft);
        }

        float calcWeaponValue(CardEngine game, Card card)
        {
            int attack = card.getAttack(game);
            int life = card.getLife(game);
            return calcWeaponValue(attack, life);
        }

        float calcWeaponValue(int attack, int life)
        {
            return attack * life;
        }

        float calcBuffAllValue(CardEngine game, Player player, Player opponent, IEnumerable<Card> targets,
            int attack, int life)
        {
            return targets.Sum(c => calcBuffValue(game, player, opponent, c, attack, life));
        }

        float calcBuffValue(CardEngine game, Player player, Player opponent, Card target, int atk, int life)
        {
            var atkBfr = target.getAttack(game);
            var lifeBfr = target.getCurrentLife(game);
            bool isFriendly = target.getOwner() == player;
            float _ = calcBuffValue(isFriendly, target.isMaster(), atkBfr, lifeBfr, atk, life);
            return _;
        }

        /// <summary>
        /// 计算即时buff价值，已区分敌我
        /// </summary>
        /// <param name="isFriendly"></param>
        /// <param name="isMaster"></param>
        /// <param name="atkBfr"></param>
        /// <param name="lifeBfr"></param>
        /// <param name="atkBuff"></param>
        /// <param name="lifeBuff"></param>
        /// <returns></returns>
        float calcBuffValue(bool isFriendly, bool isMaster, int atkBfr, int lifeBfr, int atkBuff, int lifeBuff)
        {
            return calcInstantIntPropChangeValue(isFriendly, isMaster, atkBfr, lifeBfr, atkBfr + atkBuff,
                lifeBfr + lifeBuff);
        }

        /// <summary>
        /// 估算即时伤害价值，已区分敌我
        /// </summary>
        /// <param name="game"></param>
        /// <param name="player"></param>
        /// <param name="opponent"></param>
        /// <param name="target"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        float calcDamageValue(CardEngine game, Player player, Player opponent, Card target, int value)
        {
            var atk = target.getAttack(game);
            var life = target.getCurrentLife(game);
            bool isFriendly = target.getOwner() == player;
            float _ = calcInstantIntPropChangeValue(isFriendly, target.isMaster(), atk, life, atk, life - value);
            return _;
        }

        float calcAOEDamageValue(CardEngine game, Player player, Player opponent, IEnumerable<Card> targets,
            int damage)
        {
            return targets.Sum(c => calcDamageValue(game, player, opponent, c, damage));
        }

        float calcRandomDamageValue(CardEngine game, Player player, Player opponent, IEnumerable<Card> targets,
            int damage)
        {
            float value = 0;
            foreach (var target in targets)
            {
                value += calcDamageValue(game, player, opponent, target, damage);
            }

            value /= targets.Count();
            return value;
        }

        float calcRandomServantsValue(CardEngine game, Player player, Player opponent, IEnumerable<Card> targets,
            int num)
        {
            float value = 0;
            foreach (var target in targets)
            {
                value += calcServantValue(game, player, opponent, target);
            }

            value /= targets.Count();
            value *= num;
            return value;
        }

        float calcRandomFreezeValue(CardEngine game, Player player, Player opponent, IEnumerable<Card> targets)
        {
            float value = 0;
            foreach (var target in targets)
            {
                value += calcFreezeValue(game, player, opponent, target);
            }

            value /= targets.Count();
            return value;
        }

        float calcFreezeValue(CardEngine game, Player player, Player opponent, Card target)
        {
            if (target == opponent.getMaster() || opponent.getField().Contains(target))
                return target.getAttack(game);
            return -target.getAttack(game);
        }

        float calcHealingValue(CardEngine game, Player player, Player opponent, Card target, int value)
        {
            var life = target.getLife(game);
            var lifeBfr = target.getCurrentLife(game);
            var atk = target.getAttack(game);
            var lifeAft = Math.Min(life, lifeBfr + value);
            var isFriendly = target.getOwner() == player;
            return calcInstantIntPropChangeValue(isFriendly, target.isMaster(), atk, lifeBfr, atk, lifeAft);
        }

        float calcRandomHealingValue(CardEngine game, Player player, Player opponent, IEnumerable<Card> targets,
            int value)
        {
            float _ = 0;
            foreach (var target in targets)
            {
                _ += calcHealingValue(game, player, opponent, target, value);
            }

            _ /= targets.Count();
            return _;
        }

        float calcMasterLifeWeight(int life)
        {
            return (float)Math.Log(Math.Max(life, 0)) * 100;
        }

        float calcStealthValue(CardEngine game, Player player, Player opponent, Card target)
        {
            if (player.getField().Contains(target))
                return target.getAttack(game) - target.getCurrentLife(game);
            if (target.isStealth(game))
                return target.getAttack(game) - target.getLife(game);
            if (opponent.getField().Contains(target) && target.hasKeyword(game, KEYWORD.TAUNT))
                return target.getCurrentLife(game);
            return 0;
        }

        /// <summary>
        /// 待补充
        /// </summary>
        /// <param name="game"></param>
        /// <param name="player"></param>
        /// <param name="opponent"></param>
        /// <param name="num"></param>
        /// <returns></returns>
        float calcDrawCardsValue(CardEngine game, Player player, Player opponent, int num)
        {
            return num;
        }
    }
}