using System.Collections.Generic;
using System.Threading.Tasks;
using TouhouHeartstone;
using TouhouCardEngine.Interfaces;
using System.Linq;
using Card = TouhouCardEngine.Card;
using TouhouHeartstone.Builtin;
using System;
using TouhouCardEngine;

namespace Game
{
    public class AI
    {
        THHGame game { get; }
        THHPlayer player { get; }

        public AI(THHGame game, THHPlayer player, bool autoRun = true)
        {
            this.game = game;
            this.player = player;
            if (autoRun)
                run(game);
        }

        public void run(THHGame game)
        {
            game.answers.onRequest += Answers_onRequest;
        }

        public void stop(THHGame game)
        {
            game.answers.onRequest -= Answers_onRequest;
        }

        private async void Answers_onRequest(IRequest request)
        {
            if (game.answers == null)
                return;
            await Task.Delay(1000);
            switch (request)
            {
                case InitReplaceRequest _:
                    IResponse resp = new InitReplaceResponse()
                    {
                        cardsId = new int[0]
                    };
                    game.logPlayerAction(player.id, resp);
                    _ = game.answers.answer(player.id, resp);
                    break;
                case FreeActRequest _:
                    resp = calcNextAction(game, player);
                    game.logPlayerAction(player.id, resp);
                    _ = game.answers.answer(player.id, resp);
                    break;
                case DiscoverRequest discover:
                    resp = discover.getDefaultResponse(game, player.id);
                    game.logPlayerAction(player.id, resp);
                    _ = game.answers.answer(player.id, resp);
                    break;
                default:
                    UberDebug.LogChannel("AI", "AI未处理的询问：" + request);
                    break;
            }
        }

        public Response calcNextAction(THHGame game, THHPlayer player)
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
                game.logger.logError("AI", "AI只能空过了，因为它触发了异常：" + e);
                return new TurnEndResponse();
            }
        }

        /// <summary>
        /// 计算出可以选择的行动，不包括结束回合。
        /// </summary>
        /// <param name="game"></param>
        /// <param name="player"></param>
        /// <returns></returns>
        public Dictionary<Response, float> calcActions(THHGame game, THHPlayer player)
        {
            Dictionary<Response, float> responseDic = new Dictionary<Response, float>();
            UseResponse use;
            if (player.getSkill().isUsable(game, player, out _))
            {
                if (player.getSkill().isNeedTarget(game, out var targets))
                {
                    foreach (var target in targets.Where(t => player.getSkill().isValidTarget(game, t)))
                    {
                        use = new UseResponse() {cardId = player.getSkill().id, targetsId = new int[] {target.id}};
                        responseDic.Add(use, calcActionValue(use, game, player, game.getOpponent(player)));
                    }
                }
                else
                {
                    use = new UseResponse() {cardId = player.getSkill().id};
                    responseDic.Add(use, calcActionValue(use, game, player, game.getOpponent(player)));
                }
            }

            foreach (var card in player.getHand())
            {
                if (!card.isUsable(game, player, out _))
                    continue;
                if (card.define is ServantCardDefine && card.define.getActiveEffect() == null)
                {
                    for (int i = 0; i < player.getField().count + 1; i++)
                    {
                        if (card.isNeedTarget(game, out var targets) && targets.Length > 0)
                        {
                            foreach (var target in targets.Where(t => card.isValidTarget(game, t)))
                            {
                                responseDic.Add(
                                    new UseResponse()
                                        {cardId = card.id, position = i, targetsId = new int[] {target.id}},
                                    calcServantValue(card.getAttack(game), card.getLife(game)));
                            }
                        }
                        else
                        {
                            responseDic.Add(new UseResponse() {cardId = card.id, position = i},
                                calcServantValue(card.getAttack(game), card.getLife(game)));
                        }
                    }
                }
                else
                {
                    if (card.isNeedTarget(game, out var targets))
                    {
                        foreach (var target in targets.Where(t => card.isValidTarget(game, t)))
                        {
                            use = new UseResponse() {cardId = card.id, targetsId = new int[] {target.id}};
                            responseDic.Add(use, calcActionValue(use, game, player, game.getOpponent(player)));
                        }
                    }
                    else
                    {
                        use = new UseResponse() {cardId = card.id};
                        responseDic.Add(use, calcActionValue(use, game, player, game.getOpponent(player)));
                    }
                }
            }

            if (player.getMaster().canAttack(game))
            {
                THHPlayer opponent = game.getOpponent(player);
                if (player.getMaster().isAttackable(game, player, opponent.getMaster(), out _))
                {
                    int value = player.getMaster().getAttack(game) > opponent.getMaster().getCurrentLife(game)
                        ? opponent.getMaster().getLife(game)
                        : player.getMaster().getAttack(game);
                    responseDic.Add(new AttackResponse() {cardId = player.getMaster().id, targetId = opponent.getMaster().id},
                        value);
                }

                foreach (var target in opponent.getField())
                {
                    if (!player.getMaster().isAttackable(game, player, target, out _))
                        continue;
                    int value = player.getMaster().getAttack(game) > target.getCurrentLife(game)
                        ? target.getAttack(game) + target.getCurrentLife(game)
                        : player.getMaster().getAttack(game);
                    value += player.getMaster().getCurrentLife(game) > target.getAttack(game)
                        ? -target.getAttack(game)
                        : -player.getMaster().getLife(game);
                    responseDic.Add(new AttackResponse() {cardId = player.getMaster().id, targetId = target.id},
                        value);
                }
            }

            foreach (var servant in player.getField())
            {
                if (!servant.canAttack(game))
                    continue;
                THHPlayer opponent = game.getOpponent(player);
                if (servant.isAttackable(game, player, opponent.getMaster(), out _))
                {
                    int value = servant.getAttack(game) > opponent.getMaster().getCurrentLife(game)
                        ? opponent.getMaster().getLife(game)
                        : servant.getAttack(game);
                    responseDic.Add(new AttackResponse() {cardId = servant.id, targetId = opponent.getMaster().id}, value);
                }

                foreach (var target in opponent.getField())
                {
                    if (!servant.isAttackable(game, player, target, out _))
                        continue;
                    int value = servant.getAttack(game) > target.getCurrentLife(game)
                        ? target.getAttack(game) + target.getCurrentLife(game)
                        : servant.getAttack(game);
                    value += servant.getCurrentLife(game) > target.getAttack(game)
                        ? -target.getAttack(game)
                        : -servant.getLife(game);
                    responseDic.Add(new AttackResponse() {cardId = servant.id, targetId = target.id}, value);
                }
            }

            return responseDic;
        }

        float calcActionValue(Response response, THHGame game, THHPlayer player, THHPlayer opponent)
        {
            float value = 0;
            if (response is UseResponse use)
            {
                Card card = game.getCard(use.cardId);
                Card target = use.targetsId.Length > 0 ? game.getCard(use.targetsId[0]) : null;
                if (calcNeutralValue(game, player, card, opponent, target, out value))
                    return value;
                if (calcPatchouliValue(game, player, card, opponent, target, out value))
                    return value;
                if (calcYoumuValue(game, player, card, opponent, target, out value))
                    return value;
                if (calcReisenValue(game, player, card, opponent, target, out value))
                    return value;
                if (calcBattlecryValue(game, player, card, opponent, target, out value))
                    return value;
                if (calcSanaeValue(game, player, card, opponent, target, out value))
                    return value;
                else
                    value = card.getCost(game);
            }

            return value;
        }

        bool calcNeutralValue(THHGame game, THHPlayer player, Card card, THHPlayer oppo, Card target, out float value)
        {
            switch (card.define)
            {
                case LuckyCoin _:
                    value = 1;
                    return true;
                case Neutral_73 _:
                    value = 1;
                    return true;
                case Neutral_74 _:
                    value = calcHealingValue(game, player, oppo, target, 2);
                    return true;
                default:
                    value = 0;
                    return false;
            }
        }

        bool calcYoumuValue(THHGame game, THHPlayer player, Card card, THHPlayer oppo, Card target, out float value)
        {
            switch (card.define)
            {
                case StrangeHalfbody _:
                    value = 2;
                    return true;
                case SlashOfPresentWorld _:
                    value = Youmu.isPreHeated(game, card)
                        ? calcBuffValue(game, player, oppo, player.getMaster(), 3, 0)
                        : calcBuffValue(game, player, oppo, player.getMaster(), 2, 0);
                    return true;
                case Meditation _:
                    value = Youmu.isPreHeated(game, card) ? 3 : 2;
                    return true;
                case FantasyOfEnteringEnlightenment _:
                    value = Youmu.isPreHeated(game, card) ? calcBuffValue(game, player, oppo, player.getMaster(), 0, 6) : 0;
                    return true;
                case HalfPhantonSlam _:
                    value = calcDamageValue(game, player, oppo, target, player.getMaster().getArmor(game));
                    return true;
                case Roukanken _:
                    value = player.getItem() == null
                        ? calcWeaponValue(game, card)
                        : calcWeaponValue(game, card) - calcWeaponValue(player.getItem().getAttack(game),
                            player.getItem().getCurrentLife(game));
                    return true;
                case TwoHundredYojanaOneSlash _:
                    value = Youmu.isPreHeated(game, card) ? 4 : 2;
                    return true;
                case CherryBlossomFlashing _:
                    value = calcAOEDamageValue(game, player, oppo, oppo.getField(), player.getMaster().getAttack(game));
                    return true;
                case FiveSignsOfTheDyingDeva _:
                    value = Youmu.isPreHeated(game, card)
                        ? calcBuffValue(game, player, oppo, player.getMaster(), 7, 0)
                        : calcBuffValue(game, player, oppo, player.getMaster(), 5, 0);
                    return true;
                case Hakurouken _:
                    value = player.getItem() == null
                        ? calcWeaponValue(game, card)
                        : calcBuffValue(game, player, oppo, player.getItem(), 0, card.getLife(game));
                    return true;
                default:
                    value = 0;
                    return false;
            }
        }

        bool calcPatchouliValue(THHGame game, THHPlayer player, Card card, THHPlayer opponent, Card target,
            out float value)
        {
            switch (card.define)
            {
                case SummerFire _:
                    value = calcDamageValue(game, player, opponent, target, 1);
                    return true;
                case AutumnEdge _:
                    value = calcRandomDamageValue(game, player, opponent, opponent.getField(), 2);
                    return true;
                case SpringWind _:
                    value = calcBuffValue(game, player, opponent, target, 0, 2);
                    return true;
                case WinterElement _:
                    value = calcFreezeValue(game, player, opponent, target);
                    return true;
                case SummerRed _:
                    value = calcBuffValue(game, player, opponent, target, 2, 0);
                    return true;
                case DoyouSpear _:
                    value = calcServantValue(1, 1);
                    return true;
                case MultiCast _:
                    value = player.getHand().Where(c => c.isSpell() && c.getCost(game) > 1).Count() > 0 ? 2 : 0;
                    return true;
                case TheGreatLibrary _:
                    value = 1;
                    return true;
                case BestMagic _:
                    value = 5;
                    return true;
                case ArcaneKnowledge _:
                    value = 4;
                    return true;
                case PhilosopherStone _:
                    value = 2;
                    return true;
                case MetalFatigue _:
                    value = calcAOEDamageValue(game, player, opponent, opponent.getField(), 3);
                    return true;
                case SylphyHorn _:
                    value = calcBuffValue(game, player, opponent, target, 3, 6);
                    return true;
                case PrincessUndine _:
                    value = calcAOEDamageValue(game, player, opponent, player.getField(), 3);
                    return true;
                case AgniShine _:
                    value = calcDamageValue(game, player, opponent, target, 7);
                    return true;
                case TrilithonShake _:
                    value = calcServantValue(3, 9);
                    return true;
                case RoyalFlare _:
                    value = calcDamageValue(game, player, opponent, opponent.getMaster(), 15);
                    return true;
                case SilentSelene _:
                    value = -calcServantsValue(game, player, opponent, opponent.getField());
                    return true;
                case ElementalHarvester _:
                    value = calcAOEDamageValue(game, player, opponent, opponent.getField(), 2);
                    return true;
                case BurgeoningRise _:
                    value = calcBuffValue(game, player, opponent, target, 3, 3) +
                        calcServantValue(game, player, opponent, target) - 1;
                    return true;
                case PhlogisticRain _:
                    value = calcRandomDamageValue(game, player, opponent, opponent.getField().Append(opponent.getMaster()), 1) *
                            6;
                    return true;
                case StElmoPillar _:
                    value = calcDamageValue(game, player, opponent, target, 4) +
                            calcDamageValue(game, player, opponent, opponent.getMaster(), 4);
                    return true;
                case NoachianDeluge _:
                    value = calcServantValue(2, 2) + 1;
                    return true;
                case MercuryPoison _:
                    value = calcAOEDamageValue(game, player, opponent, opponent.getField(), 3);
                    return true;
                case ForestBlaze _:
                    value = calcBuffAllValue(game, player, opponent, player.getField(), 2, 2) +
                            calcRandomDamageValue(game, player, opponent, opponent.getField(), 1) * player.getField().count;
                    return true;
                case WaterElf _:
                    value = calcHealingValue(game, player, opponent, target,
                                target.getLife(game) - target.getCurrentLife(game)) +
                            calcBuffValue(target.getOwner() == player, target.isMaster(), target.getAttack(game),
                                target.getLife(game), 3, 6);
                    return true;
                case LavaCromlech _:
                    value = (calcServantValue(2, 2) +
                             calcRandomDamageValue(game, player, opponent, opponent.getField(), 2)) * 3;
                    return true;
                case GingerGust _:
                    value = (calcServantValue(3, 1) + 1) * opponent.getField().count;
                    return true;
                default:
                    value = 0;
                    return false;
            }
        }

        bool calcReisenValue(THHGame game, THHPlayer player, Card card, THHPlayer opponent, Card target,
            out float value)
        {
            switch (card.define)
            {
                case LunaticShoot _:
                    value = calcDamageValue(game, player, opponent, opponent.getMaster(), 2);
                    return true;
                case LunaticShoot_MindDropping _:
                    value = calcDamageValue(game, player, opponent, target, 2);
                    return true;
                case XWave _:
                    value = 2 + opponent.getField().count;
                    return true;
                case MindStopper _:
                    value = 2 + opponent.getField().count == 0 ? 1 : 0;
                    return true;
                case MindShaker _:
                    value = 2 + opponent.getField().count;
                    return true;
                case DisorderEye _:
                    value = 2 + opponent.getField().count == 0 ? 0 : 1;
                    return true;
                case VisionaryTuning _:
                    value = 2 + player.getField().count + player.getMaster().getCurrentLife(game) < 15 ? 2 : 0;
                    return true;
                case FieldUltraViolet _:
                    value = 1;
                    return true;
                case PeerlessPatriotsElixir _:
                    value = calcBuffValue(game, player, opponent, target, 3, 3) - calcServantValue(game, player,
                        opponent, target);
                    return true;
                case MindExplosion _:
                    value = calcDamageValue(game, player, opponent, opponent.getMaster(), 5);
                    return true;
                case MindDropping _:
                    value = 1;
                    return true;
                case LunaticGun _:
                    value = player.getItem() == null ? calcWeaponValue(game, card) : 0;
                    return true;
                case DisbeliefAspect _:
                    value = 1;
                    return true;
                default:
                    value = 0;
                    return false;
            }
        }

        bool calcSanaeValue(THHGame game, THHPlayer player, Card card, THHPlayer opponent, Card target,
            out float value)
        {
            switch (card.define)
            {
                case WindStopMiracle _:
                    value = calcHealingValue(game, player, opponent, target, 2);
                    return true;
                case StarRitual _:
                    value = 0.5f;
                    return true;
                case GrayThaumaturgy _:
                    value = calcHealingValue(game, player, opponent, target, 10);
                    return true;
                case BumperCrop _:
                    value = 5 * calcRandomHealingValue(game, player, opponent, game.getAllAllys(player), 1);
                    return true;
                case GuestStarsMiracle _:
                    value = calcAOEDamageValue(game, player, opponent, opponent.getField(), 2);
                    return true;
                case OpenSeaMiracle _:
                    value = calcDamageValue(game, player, opponent, target, 2);
                    return true;
                case GodWind _:
                    value = -calcServantsValue(game, player, opponent, opponent.getField()) -
                            calcServantsValue(game, player, opponent, player.getField());
                    return true;
                case ForgottenRecrption _:
                    value = 1;
                    return true;
                case OmikujiBomb _:
                    value = 1;
                    return true;
                case Bomb1 _:
                    value = calcDamageValue(game, player, opponent, target, 6);
                    return true;
                case Bomb2 _:
                    value = calcDamageValue(game, player, opponent, target, 4);
                    return true;
                case Bomb3 _:
                    value = calcRandomDamageValue(game, player, opponent, opponent.getField(), 4);
                    return true;
                case Bomb4 _:
                    value = 6 * calcRandomDamageValue(game, player, opponent, opponent.getField().Concat(player.getField()), 1);
                    return true;
                case GodSummon _:
                    value = 1;
                    return true;
                case PlagueOfMishaguchi _:
                    value = calcDamageValue(game, player, opponent, player.getMaster(), target.getLife(game)) +
                            calcServantValue(target.getAttack(game), target.getCurrentLife(game));
                    return true;
                case PlagueOfFrogs _:
                    value = opponent.getField().count * calcServantValue(2, 2);
                    return true;
                case PlagueOfHibernation _:
                    value = 1;
                    return true;
                case PlagueOfCrops _:
                    value = 1;
                    return true;
                default:
                    value = 0;
                    return false;
            }
        }

        bool calcBattlecryValue(THHGame game, THHPlayer player, Card card, THHPlayer opponent, Card target,
            out float value)
        {
            value = calcServantValue(game, player, opponent, card);
            switch (card.define)
            {
                case RifleHunter _:
                    value += calcDamageValue(game, player, opponent, target, 2);
                    return true;
                case FlameFairy _:
                    value += calcRandomDamageValue(game, player, opponent, opponent.getField(), 1);
                    return true;
                case LunaChild _:
                    value += calcStealthValue(game, player, opponent, target);
                    return true;
                case BeerFairy _:
                    value -= calcServantValue(game, player, opponent, target);
                    value += target.define.getActiveEffect() != null ? 5 : 0;
                    return true;
                case Cirno _:
                    value += calcRandomFreezeValue(game, player, opponent, game.getAllCharacters().Append(card));
                    return true;
                case Cirno_IceFall _:
                    foreach (var neighbor in target.getNearbyCards())
                    {
                        value += calcFreezeValue(game, player, opponent, neighbor);
                    }

                    return true;
                case Clownpiece _:
                    value += calcBuffAllValue(game, player, opponent, game.getAllServants(), 2, 0);
                    return true;
                case EternityLarva _:
                    //这？
                    return true;
                case LilyWhite _:
                    value += calcHealingValue(game, player, opponent, player.getMaster(), 4) +
                             calcHealingValue(game, player, opponent, opponent.getMaster(), 4) +
                             calcDrawCardsValue(game, player, opponent, 2);
                    return true;
                case Cirno_N_YYM _:
                    value += calcFreezeValue(game, player, opponent, target);
                    return true;
                case Cirno_N_HMX _:
                    foreach (var neighbor in target.getNearbyCards())
                    {
                        value += calcFreezeValue(game, player, opponent, neighbor);
                    }

                    return true;
                case Chen_N _:
                    value += calcBuffValue(game, player, opponent, card, 2, 0);
                    // + calcRushValue
                    return true;
                case Koakuma_N _:
                    value += calcDrawCardsValue(game, player, opponent, 1);
                    return true;
                case PatchouliKnowledge_N _:
                    value += calcDrawCardsValue(game, player, opponent, 2);
                    return true;
                case RemiliaScarlet_N _:
                    return true;
                case FlandreScarlet_N _:
                    value = calcRandomServantsValue(game, player, opponent, opponent.getField(), 1);
                    return true;
                case IzayoiSakuya_N _:
                    value += calcDrawCardsValue(game, player, opponent, 1) + 4;
                    return true;
                case YakumoRan_N _:
                    value += calcServantValue(4, 3);
                    return true;
                case SaigyoujiYuyuko_N _:
                    value -= calcServantValue(game, player, opponent, target);
                    return true;
                case YakumoYukari_N _:
                    value -= calcServantsValue(game, player, opponent, opponent.getField()) +
                             calcServantsValue(game, player, opponent, player.getField());
                    // 加个回手value
                    return true;
                case LunasaPrismriver_N _:
                    value += calcServantValue(1, 2) +
                             calcServantValue(2, 1);
                    return true;
                case MerlinPrismriver_N _:
                    value += calcServantValue(2, 2) +
                             calcServantValue(2, 1);
                    return true;
                case LyricaPrismriver_N _:
                    value += calcServantValue(2, 2) +
                             calcServantValue(1, 2);
                    return true;
                case MystiaLorelei_N _:
                    value += calcBuffValue(game, player, opponent, target, 1 - target.getAttack(game), 0);
                    return true;
                case ReisenUdongeinInaba_N _:
                    value += calcDamageValue(game, player, opponent, opponent.getMaster(), 4);
                    return true;
                case TewiInaba_N _:
                    return true;
                case KamishirasawaKeine_Human_N _:
                    return true;
                case KamishirasawaKeine_Beast_N _:
                    return true;
                case MedicineSellman _:
                    value += calcHealingValue(game, player, opponent, target, 4);
                    return true;
                case TewiInaba _:
                    value += calcDrawCardsValue(game, player, opponent, 1);
                    return true;
                case YoukaiSmith _:
                    value += player.getItem()?.getAttack(game) ?? 0;
                    return true;
                case Koakuma _:
                    value += calcDrawCardsValue(game, player, opponent, 1);
                    return true;
                case OrochiOfAncientTimes _:
                    value += calcServantValue(target.getAttack(game), target.getCurrentLife(game));
                    return true;
                case YasakaKanako _:
                    value += 10;
                    return true;
                default:
                    value = 0;
                    return false;
            }
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
        float calcServantValue(THHGame game, THHPlayer player, THHPlayer opponent, Card servant)
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
        float calcServantsValue(THHGame game, THHPlayer player, THHPlayer opponent, IEnumerable<Card> servants)
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

        float calcWeaponValue(THHGame game, Card card)
        {
            int attack = card.getAttack(game);
            int life = card.getLife(game);
            return calcWeaponValue(attack, life);
        }

        float calcWeaponValue(int attack, int life)
        {
            return attack * life;
        }

        float calcBuffAllValue(THHGame game, THHPlayer player, THHPlayer opponent, IEnumerable<Card> targets,
            int attack, int life)
        {
            return targets.Sum(c => calcBuffValue(game, player, opponent, c, attack, life));
        }

        float calcBuffValue(THHGame game, THHPlayer player, THHPlayer opponent, Card target, int atk, int life)
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
        float calcDamageValue(THHGame game, THHPlayer player, THHPlayer opponent, Card target, int value)
        {
            var atk = target.getAttack(game);
            var life = target.getCurrentLife(game);
            bool isFriendly = target.getOwner() == player;
            float _ = calcInstantIntPropChangeValue(isFriendly, target.isMaster(), atk, life, atk, life - value);
            return _;
        }

        float calcAOEDamageValue(THHGame game, THHPlayer player, THHPlayer opponent, IEnumerable<Card> targets,
            int damage)
        {
            return targets.Sum(c => calcDamageValue(game, player, opponent, c, damage));
        }

        float calcRandomDamageValue(THHGame game, THHPlayer player, THHPlayer opponent, IEnumerable<Card> targets,
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

        float calcRandomServantsValue(THHGame game, THHPlayer player, THHPlayer opponent, IEnumerable<Card> targets,
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

        float calcRandomFreezeValue(THHGame game, THHPlayer player, THHPlayer opponent, IEnumerable<Card> targets)
        {
            float value = 0;
            foreach (var target in targets)
            {
                value += calcFreezeValue(game, player, opponent, target);
            }

            value /= targets.Count();
            return value;
        }

        float calcFreezeValue(THHGame game, THHPlayer player, THHPlayer opponent, Card target)
        {
            if (target == opponent.getMaster() || opponent.getField().Contains(target))
                return target.getAttack(game);
            return -target.getAttack(game);
        }

        float calcHealingValue(THHGame game, THHPlayer player, THHPlayer opponent, Card target, int value)
        {
            var life = target.getLife(game);
            var lifeBfr = target.getCurrentLife(game);
            var atk = target.getAttack(game);
            var lifeAft = Math.Min(life, lifeBfr + value);
            var isFriendly = target.getOwner() == player;
            return calcInstantIntPropChangeValue(isFriendly, target.isMaster(), atk, lifeBfr, atk, lifeAft);
        }

        float calcRandomHealingValue(THHGame game, THHPlayer player, THHPlayer opponent, IEnumerable<Card> targets,
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
            return (float) Math.Log(Math.Max(life, 0)) * 100;
        }

        float calcStealthValue(THHGame game, THHPlayer player, THHPlayer opponent, Card target)
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
        float calcDrawCardsValue(THHGame game, THHPlayer player, THHPlayer opponent, int num)
        {
            return num;
        }
    }
}