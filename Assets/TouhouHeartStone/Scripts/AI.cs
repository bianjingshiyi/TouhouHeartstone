using System.Collections.Generic;
using System.Threading.Tasks;
using TouhouHeartstone;
using TouhouCardEngine.Interfaces;
using System.Linq;
using Card = TouhouCardEngine.Card;
using TouhouHeartstone.Builtin;

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
                    _ = game.answers.answer(player.id, new InitReplaceResponse()
                    {
                        cardsId = new int[0]
                    });
                    break;
                case FreeActRequest _:
                    _ = game.answers.answer(player.id, calcNextAction(game, player));
                    break;
                case DiscoverRequest discover:
                    _ = game.answers.answer(player.id, discover.getDefaultResponse(game, player.id));
                    break;
                default:
                    UberDebug.LogChannel("AI", "AI未处理的询问：" + request);
                    break;
            }
        }
        public Response calcNextAction(THHGame game, THHPlayer player)
        {
            var actions = calcActions(game, player).Where(p => p.Value > 0);
            if (actions.Count() > 0)
            {
                return actions.OrderByDescending(p => p.Value).First().Key;
            }
            else
                return new TurnEndResponse();
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
                        TouhouCardEngine.Card[] targets = card.getAvaliableTargets(game);
                        if (targets != null)
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
                value = card.getCost(game);
                if (card.define is SummerFire)
                    value = calcDamageValue(game, player, opponent, target, 1);
                else if (card.define is AutumnEdge)
                    value = calcRandomDamageValue(game, player, opponent, opponent.field, 2);
                else if (card.define is SpringWind)
                    value = calcBuffValue(game, player, opponent, target, 0, 2);
                else if (card.define is WinterElement)
                    value = calcFreezeValue(game, player, opponent, target);
                else if (card.define is SummerFire)
                    value = calcBuffValue(game, player, opponent, target, 2, 0);
                else if (card.define is DoyouSpear)
                    value = calcServantValue(game, player, opponent, 1, 1);
                else if (card.define is MultiCast)
                    value = player.hand.Where(c => c.isSpell() && c.getCost(game) > 1).Count() > 0 ? 2 : 0;
                else if (card.define is TheGreatLibrary)
                    value = 1;
                else if (card.define is BestMagic)
                    value = 5;
                else if (card.define is ArcaneKnowledge)
                    value = 4;
                else if (card.define is PhilosopherStone)
                    value = 2;
                else if (card.define is MetalFatigue)
                    value = calcAOEDamageValue(game, player, opponent, opponent.field, 3);
                else if (card.define is SylphyHorn)
                    value = calcBuffValue(game, player, opponent, target, 3, 6);
                else if (card.define is PrincessUndine)
                    value = calcAOEDamageValue(game, player, opponent, player.field, 3);
                else if (card.define is AgniShine)
                    value = calcDamageValue(game, player, opponent, target, 7);
                else if (card.define is TrilithonShake)
                    value = calcServantValue(game, player, opponent, 3, 9);
                else if (card.define is RoyalFlare)
                    value = calcDamageValue(game, player, opponent, opponent.master, 15);
                else if (card.define is SilentSelene)
                    value = calcServantsValue(game, player, opponent, opponent.field);
                else if (card.define is ElementalHarvester)
                    value = calcAOEDamageValue(game, player, opponent, opponent.field, 2);
                else if (card.define is BurgeoningRise)
                    value = calcBuffValue(game, player, opponent, target, 3, 3) + (target.getOwner() == player ? calcServantValue(game, player, opponent, target) - 1 : 0);
                else if (card.define is PhlogisticRain)
                    value = calcRandomDamageValue(game, player, opponent, opponent.field.Append(opponent.master), 1) * 6;
                else if (card.define is StElmoPillar)
                    value = calcDamageValue(game, player, opponent, target, 6);
                else if (card.define is NoachianDeluge)
                    value = calcServantValue(game, player, opponent, 2, 2) + 1;
                else if (card.define is MercuryPoison)
                    value = calcAOEDamageValue(game, player, opponent, opponent.field, 3);
                else if (card.define is ForestBlaze)
                    value = calcBuffAllValue(game, player, opponent, player.field, 2, 2) + calcRandomDamageValue(game, player, opponent, opponent.field, 1) * player.field.count;
                else if (card.define is WaterElf)
                    value = calcBuffValue(game, player, opponent, target, 3, 6 + target.getLife(game) - target.getCurrentLife(game)) + 1;
                else if (card.define is LavaCromlech)
                    value = (calcServantValue(game, player, opponent, 2, 2) + calcRandomDamageValue(game, player, opponent, opponent.field, 2)) * 3;
                else if (card.define is GingerGust)
                    value = (calcServantValue(game, player, opponent, 3, 1) + 1) * opponent.field.count;
            }
            return value;
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