using System.Collections.Generic;
using System.Threading.Tasks;
using TouhouHeartstone;
using TouhouCardEngine.Interfaces;
using System.Linq;
using Card = TouhouCardEngine.Card;
using TouhouHeartstone.Builtin;
using System;

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
                if (actions.Count() > 0)
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
            if (player.skill.isUsable(game, player, out _))
            {
                if (player.skill.isNeedTarget(game, out var targets))
                {
                    foreach (var target in targets)
                    {
                        use = new UseResponse() { cardId = player.skill.id, targetsId = new int[] { target.id } };
                        responseDic.Add(use, calcActionValue(use, game, player, game.getOpponent(player)));
                    }
                }
                else
                {
                    use = new UseResponse() { cardId = player.skill.id };
                    responseDic.Add(use, calcActionValue(use, game, player, game.getOpponent(player)));
                }
            }
            foreach (var card in player.hand)
            {
                if (!card.isUsable(game, player, out _))
                    continue;
                if (card.define is ServantCardDefine)
                {
                    for (int i = 0; i < player.field.count + 1; i++)
                    {
                        if (card.isNeedTarget(game, out var targets) && targets.Length > 0)
                        {
                            foreach (var target in targets)
                            {
                                responseDic.Add(new UseResponse() { cardId = card.id, position = i, targetsId = new int[] { target.id } },
                                    card.getAttack(game) + card.getLife(game));
                            }
                        }
                        else
                        {
                            responseDic.Add(new UseResponse() { cardId = card.id, position = i },
                                    card.getAttack(game) + card.getLife(game));
                        }
                    }
                }
                else
                {
                    if (card.isNeedTarget(game, out var targets))
                    {
                        foreach (var target in targets)
                        {
                            use = new UseResponse() { cardId = card.id, targetsId = new int[] { target.id } };
                            responseDic.Add(use, calcActionValue(use, game, player, game.getOpponent(player)));
                        }
                    }
                    else
                    {
                        use = new UseResponse() { cardId = card.id };
                        responseDic.Add(use, calcActionValue(use, game, player, game.getOpponent(player)));
                    }
                }
            }
            if (player.master.canAttack(game))
            {
                THHPlayer opponent = game.getOpponent(player);
                if (player.master.isAttackable(game, player, opponent.master, out _))
                {
                    int value = player.master.getAttack(game) > opponent.master.getCurrentLife(game) ?
                        opponent.master.getLife(game) : player.master.getAttack(game);
                    responseDic.Add(new AttackResponse() { cardId = player.master.id, targetId = opponent.master.id },
                        value);
                }
                foreach (var target in opponent.field)
                {
                    if (!player.master.isAttackable(game, player, target, out _))
                        continue;
                    int value = player.master.getAttack(game) > target.getCurrentLife(game) ?
                        target.getAttack(game) + target.getCurrentLife(game) : player.master.getAttack(game);
                    value += player.master.getCurrentLife(game) > target.getAttack(game) ?
                        -target.getAttack(game) : -player.master.getLife(game);
                    responseDic.Add(new AttackResponse() { cardId = player.master.id, targetId = target.id },
                        value);
                }
            }
            foreach (var servant in player.field)
            {
                if (!servant.canAttack(game))
                    continue;
                THHPlayer opponent = game.getOpponent(player);
                if (servant.isAttackable(game, player, opponent.master, out _))
                {
                    int value = servant.getAttack(game) > opponent.master.getCurrentLife(game) ?
                        opponent.master.getLife(game) : servant.getAttack(game);
                    responseDic.Add(new AttackResponse() { cardId = servant.id, targetId = opponent.master.id }, value);
                }
                foreach (var target in opponent.field)
                {
                    if (!servant.isAttackable(game, player, target, out _))
                        continue;
                    int value = servant.getAttack(game) > target.getCurrentLife(game) ?
                        target.getAttack(game) + target.getCurrentLife(game) : servant.getAttack(game);
                    value += servant.getCurrentLife(game) > target.getAttack(game) ?
                        -target.getAttack(game) : -servant.getLife(game);
                    responseDic.Add(new AttackResponse() { cardId = servant.id, targetId = target.id }, value);
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
                else
                    value = card.getCost(game);
            }
            return value;
        }
        bool calcNeutralValue(THHGame game, THHPlayer player, Card card, THHPlayer oppo, Card target, out float value)
        {
            if (card.define is LuckyCoin)
            {
                value = 1;
                return true;
            }
            value = 0;
            return false;
        }
        bool calcYoumuValue(THHGame game, THHPlayer player, Card card, THHPlayer oppo, Card target, out float value)
        {
            switch (card.define)
            {
                case StrangeHalfbody _:
                    value = 2; return true;
                case SlashOfPresentWorld _:
                    value = Youmu.isPreHeated(game, card) ?
                        calcBuffValue(game, player, oppo, player.master, 3, 0) :
                        calcBuffValue(game, player, oppo, player.master, 2, 0);
                    return true;
                case Meditation _:
                    value = Youmu.isPreHeated(game, card) ? 3 : 2; return true;
                case FantasyOfEnteringEnlightenment _:
                    value = Youmu.isPreHeated(game, card) ?
                        calcBuffValue(game, player, oppo, player.master, 0, 6) :
                        0;
                    return true;
                case HalfPhantonSlam _:
                    value = calcDamageValue(game, player, oppo, target, player.master.getArmor(game)); return true;
                case Roukanken _:
                    value = 6; return true;
                case TwoHundredYojanaOneSlash _:
                    value = Youmu.isPreHeated(game, card) ? 4 : 2; return true;
                case CherryBlossomFlashing _:
                    value = calcAOEDamageValue(game, player, oppo, oppo.field, player.master.getAttack(game)); return true;
                case FiveSignsOfTheDyingDeva _:
                    value = Youmu.isPreHeated(game, card) ?
                        calcBuffValue(game, player, oppo, player.master, 7, 0) :
                        calcBuffValue(game, player, oppo, player.master, 5, 0);
                    return true;
                case Hakurouken _:
                    value = player.item == null ?
                        calcWeaponValue(game, card) :
                        calcBuffValue(game, player, oppo, player.item, 0, card.getLife(game));
                    return true;
                default:
                    value = 0; return false;
            }
        }
        float calcWeaponValue(THHGame game, Card card)
        {
            return card.getAttack(game) * card.getLife(game);
        }
        bool calcPatchouliValue(THHGame game, THHPlayer player, Card card, THHPlayer opponent, Card target, out float value)
        {
            switch (card.define)
            {
                case SummerFire _:
                    value = calcDamageValue(game, player, opponent, target, 1);
                    return true;
                case AutumnEdge _:
                    value = calcRandomDamageValue(game, player, opponent, opponent.field, 2); return true;
                case SpringWind _:
                    value = calcBuffValue(game, player, opponent, target, 0, 2); return true;
                case WinterElement _:
                    value = calcFreezeValue(game, player, opponent, target); return true;
                case SummerRed _:
                    value = calcBuffValue(game, player, opponent, target, 2, 0); return true;
                case DoyouSpear _:
                    value = calcServantValue(game, player, opponent, 1, 1); return true;
                case MultiCast _:
                    value = player.hand.Where(c => c.isSpell() && c.getCost(game) > 1).Count() > 0 ? 2 : 0;
                    return true;
                case TheGreatLibrary _:
                    value = 1;
                    return true;
                case BestMagic _:
                    value = 5;
                    return true;
                case ArcaneKnowledge _:
                    value = 4; return true;
                case PhilosopherStone _:
                    value = 2; return true;
                case MetalFatigue _:
                    value = calcAOEDamageValue(game, player, opponent, opponent.field, 3); return true;
                case SylphyHorn _:
                    value = calcBuffValue(game, player, opponent, target, 3, 6); return true;
                case PrincessUndine _:
                    value = calcAOEDamageValue(game, player, opponent, player.field, 3); return true;
                case AgniShine _:
                    value = calcDamageValue(game, player, opponent, target, 7); return true;
                case TrilithonShake _:
                    value = calcServantValue(game, player, opponent, 3, 9); return true;
                case RoyalFlare _:
                    value = calcDamageValue(game, player, opponent, opponent.master, 15); return true;
                case SilentSelene _:
                    value = calcServantsValue(game, player, opponent, opponent.field); return true;
                case ElementalHarvester _:
                    value = calcAOEDamageValue(game, player, opponent, opponent.field, 2); return true;
                case BurgeoningRise _:
                    value = calcBuffValue(game, player, opponent, target, 3, 3) + (target.getOwner() == player ? calcServantValue(game, player, opponent, target) - 1 : 0); return true;
                case PhlogisticRain _:
                    value = calcRandomDamageValue(game, player, opponent, opponent.field.Append(opponent.master), 1) * 6; return true;
                case StElmoPillar _:
                    value = calcDamageValue(game, player, opponent, target, 6); return true;
                case NoachianDeluge _:
                    value = calcServantValue(game, player, opponent, 2, 2) + 1; return true;
                case MercuryPoison _:
                    value = calcAOEDamageValue(game, player, opponent, opponent.field, 3); return true;
                case ForestBlaze _:
                    value = calcBuffAllValue(game, player, opponent, player.field, 2, 2) + calcRandomDamageValue(game, player, opponent, opponent.field, 1) * player.field.count; return true;
                case WaterElf _:
                    value = calcBuffValue(game, player, opponent, target, 3, 6 + target.getLife(game) - target.getCurrentLife(game)) + 1; return true;
                case LavaCromlech _:
                    value = (calcServantValue(game, player, opponent, 2, 2) + calcRandomDamageValue(game, player, opponent, opponent.field, 2)) * 3; return true;
                case GingerGust _:
                    value = (calcServantValue(game, player, opponent, 3, 1) + 1) * opponent.field.count; return true;
                default:
                    value = 0;
                    return false;
            }
        }

        float calcBuffAllValue(THHGame game, THHPlayer player, THHPlayer opponent, IEnumerable<Card> targets, int attack, int life)
        {
            return targets.Sum(c => calcBuffValue(game, player, opponent, c, attack, life));
        }
        float calcBuffValue(THHGame game, THHPlayer player, THHPlayer opponent, Card target, int attack, int life)
        {
            float value = 0;
            if (target == player.master)
                value = attack * target.getCurrentLife(game) + life;
            else if (player.field.Contains(target))
                value = calcServantValue(game, player, opponent, target.getAttack(game) + attack, target.getCurrentLife(game) + life)
                    - calcServantValue(game, player, opponent, target.getAttack(game), target.getLife(game));
            return value;
        }
        float calcAOEDamageValue(THHGame game, THHPlayer player, THHPlayer opponent, IEnumerable<Card> targets, int damage)
        {
            return targets.Sum(c => calcDamageValue(game, player, opponent, c, damage));
        }
        float calcRandomDamageValue(THHGame game, THHPlayer player, THHPlayer opponent, IEnumerable<Card> targets, int damage)
        {
            float value = 0;
            foreach (var target in targets)
            {
                value += calcDamageValue(game, player, opponent, target, damage);
            }
            value /= targets.Count();
            return value;
        }
        float calcDamageValue(THHGame game, THHPlayer player, THHPlayer opponent, Card target, int damage)
        {
            float value = 0;
            if (target == opponent.master)
                value = target.getCurrentLife(game) > damage ? damage : float.MaxValue;
            else if (opponent.field.Contains(target))
                value = target.getCurrentLife(game) > damage ? target.getAttack(game) * damage : target.getAttack(game) * target.getCurrentLife(game) + 1;
            return value;
        }
        float calcServantsValue(THHGame game, THHPlayer player, THHPlayer opponent, IEnumerable<Card> servants)
        {
            return servants.Sum(s => calcServantValue(game, player, opponent, s));
        }
        float calcServantValue(THHGame game, THHPlayer player, THHPlayer opponent, Card servant)
        {
            return calcServantValue(game, player, opponent, servant.getAttack(game), servant.getCurrentLife(game));
        }
        float calcServantValue(THHGame game, THHPlayer player, THHPlayer opponent, int attack, int life)
        {
            return attack * life + 1;
        }
        float calcFreezeValue(THHGame game, THHPlayer player, THHPlayer opponent, Card target)
        {
            if (target == opponent.master || opponent.field.Contains(target))
                return target.getAttack(game);
            return 0;
        }
    }
}