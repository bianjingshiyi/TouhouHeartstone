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
            if (player.skill.isUsable(game, player, out _))
            {
                if (player.skill.isNeedTarget(game, out var targets))
                {
                    foreach (var target in targets)
                    {
                        responseDic.Add(new UseResponse() { cardId = player.skill.id, targetsId = new int[] { target.id } }, player.skill.define.getCost());
                    }
                }
                else
                {
                    responseDic.Add(new UseResponse() { cardId = player.skill.id }, player.skill.define.getCost());
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
                    TouhouCardEngine.Card[] targets = card.getAvaliableTargets(game);
                    if (targets != null && targets.Length > 0)
                    {
                        foreach (var target in targets)
                        {
                            responseDic.Add(new UseResponse() { cardId = card.id, targetsId = new int[] { target.id } },
                                card.getCost(game));
                        }
                    }
                    else
                    {
                        responseDic.Add(new UseResponse() { cardId = card.id },
                            card.getCost(game));
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
        public float calcActionValue(Response response, THHGame game, THHPlayer player, THHPlayer opponent)
        {
            float value = 0;
            if (response is UseResponse use)
            {
                Card card = game.getCard(use.cardId);
                Card target = use.targetsId.Length > 0 ? game.getCard(use.targetsId[0]) : null;
                value = card.getCost(game);
                if (card.define is SummerFire)
                {
                    value = calcDamageValue(game, player, opponent, target, 1);
                }
            }
            return value;
        }
        float calcBuffValue(THHGame game, THHPlayer player, THHPlayer opponent, Card target, int attack, int life)
        {
            float value = 0;
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
    }
}