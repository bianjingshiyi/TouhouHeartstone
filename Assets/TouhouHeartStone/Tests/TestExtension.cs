using System.Linq;
using System;
using TouhouHeartstone;
using TouhouCardEngine;

namespace Tests
{
    static class TestExtension
    {
        public static void skipTurnWhen(this THHGame game, Func<bool> condition)
        {
            if (!game.isRunning)
            {
                game.run();
            }
            if (!game.triggers.getRecordedEvents().Any(e => e is HearthStoneRule.StartEventArg))
            {
                game.getSortedPlayers()[0].cmdInitReplace(game);
                game.getSortedPlayers()[1].cmdInitReplace(game);
            }
            int count = 0;
            while (condition())
            {
                if (game.getCurrentPlayer() == game.getSortedPlayers()[0])
                    game.getSortedPlayers()[0].cmdTurnEnd(game);
                if (!condition())
                    return;
                if (game.getCurrentPlayer() == game.getSortedPlayers()[1])
                    game.getSortedPlayers()[1].cmdTurnEnd(game);
                count++;
                if (count > 100)
                    throw new StackOverflowException();
            }
        }
        public static void skipTurnUntil(this THHGame game, Func<bool> condition)
        {
            if (!game.isRunning)
            {
                game.run();
                game.getSortedPlayers()[0].cmdInitReplace(game);
                game.getSortedPlayers()[1].cmdInitReplace(game);
            }
            int count = 0;
            while (!condition())
            {
                if (game.getCurrentPlayer() == game.getSortedPlayers()[0])
                    game.getSortedPlayers()[0].cmdTurnEnd(game);
                if (condition())
                    return;
                if (game.getCurrentPlayer() == game.getSortedPlayers()[1])
                    game.getSortedPlayers()[1].cmdTurnEnd(game);
                count++;
                if (count > 1000)
                    throw new StackOverflowException();
            }
        }
        public static void skipUntilYourTurn(this THHGame game, Player player)
        {
            game.skipTurnUntil(() => game.getCurrentPlayer() == player);
        }
        public static void skipUntilCanUse<T>(this THHGame game, Player you) where T : CardDefine
        {
            game.skipTurnUntil(() => game.getCurrentPlayer() == you && you.getGem() >= game.getDefine<T>().getProp<int>(nameof(ServantCardDefine.cost)));
        }
        public static void skipUntilHasGem(this THHGame game, Player you, int gem)
        {
            game.skipTurnUntil(() => game.getCurrentPlayer() == you && you.getGem() >= gem);
        }
        public static void skipUntilCanUse(this THHGame game, Player you, params CardDefine[] card)
        {
            game.skipTurnUntil(() => game.getCurrentPlayer() == you && you.getGem() >= card.Sum(c => c.getCost()));
        }
        public static void cmdUse<T>(this Player player, THHGame game, int position = 0, params Card[] targets) where T : CardDefine
        {
            player.cmdUse(game, player.getHand().getCard<T>(), position, targets);
        }
        public static void cmdUse<T>(this Player player, THHGame game, out Card servant,int position = 0, params Card[] targets) where T : ServantCardDefine
        {
            servant = player.getHand().getCard<T>();
            player.cmdUse(game, servant, position, targets);
        }
        public static void cmdAttack<T>(this Player player, THHGame game, Card target) where T : CardDefine
        {
            player.cmdAttack(game, player.getField().getCard<T>(), target);
        }
    }
}