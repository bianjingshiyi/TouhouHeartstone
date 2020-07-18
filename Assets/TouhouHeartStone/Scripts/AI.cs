using System.Collections.Generic;
using System.Threading.Tasks;
using TouhouHeartstone;
using TouhouCardEngine.Interfaces;
using System.Linq;
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
            var actions = calcActions(game, player);
            if (actions.Count > 0)
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
                                    card.getAttack() + card.getLife());
                            }
                        }
                        else
                        {
                            responseDic.Add(new UseResponse() { cardId = card.id, position = i },
                                    card.getAttack() + card.getLife());
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
                                card.getCost());
                        }
                    }
                    else
                    {
                        responseDic.Add(new UseResponse() { cardId = card.id },
                            card.getCost());
                    }
                }
            }
            if (player.master.canAttack(game))
            {
                THHPlayer opponent = game.getOpponent(player);
                if (player.master.isAttackable(game, player, opponent.master, out _))
                {
                    int value = player.master.getAttack() > opponent.master.getCurrentLife() ?
                        opponent.master.getLife() : player.master.getAttack();
                    responseDic.Add(new AttackResponse() { cardId = player.master.id, targetId = opponent.master.id },
                        value);
                }
                foreach (var target in opponent.field)
                {
                    if (!player.master.isAttackable(game, player, target, out _))
                        continue;
                    int value = player.master.getAttack() > target.getCurrentLife() ?
                        target.getAttack() + target.getCurrentLife() : player.master.getAttack();
                    value += player.master.getCurrentLife() > target.getAttack() ?
                        -target.getAttack() : -player.master.getLife();
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
                    int value = servant.getAttack() > opponent.master.getCurrentLife() ?
                        opponent.master.getLife() : servant.getAttack();
                    responseDic.Add(new AttackResponse() { cardId = servant.id, targetId = opponent.master.id }, value);
                }
                foreach (var target in opponent.field)
                {
                    if (!servant.isAttackable(game, player, target, out _))
                        continue;
                    int value = servant.getAttack() > target.getCurrentLife() ?
                        target.getAttack() + target.getCurrentLife() : servant.getAttack();
                    value += servant.getCurrentLife() > target.getAttack() ?
                        -target.getAttack() : -servant.getLife();
                    responseDic.Add(new AttackResponse() { cardId = servant.id, targetId = target.id }, value);
                }
            }
            return responseDic;
        }
    }
}