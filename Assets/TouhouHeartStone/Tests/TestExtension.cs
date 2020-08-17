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
            if (!game.triggers.getRecordedEvents().Any(e => e is THHGame.StartEventArg))
            {
                game.sortedPlayers[0].cmdInitReplace(game);
                game.sortedPlayers[1].cmdInitReplace(game);
            }
            int count = 0;
            while (condition())
            {
                if (game.currentPlayer == game.sortedPlayers[0])
                    game.sortedPlayers[0].cmdTurnEnd(game);
                if (!condition())
                    return;
                if (game.currentPlayer == game.sortedPlayers[1])
                    game.sortedPlayers[1].cmdTurnEnd(game);
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
                game.sortedPlayers[0].cmdInitReplace(game);
                game.sortedPlayers[1].cmdInitReplace(game);
            }
            int count = 0;
            while (!condition())
            {
                if (game.currentPlayer == game.sortedPlayers[0])
                    game.sortedPlayers[0].cmdTurnEnd(game);
                if (condition())
                    return;
                if (game.currentPlayer == game.sortedPlayers[1])
                    game.sortedPlayers[1].cmdTurnEnd(game);
                count++;
                if (count > 1000)
                    throw new StackOverflowException();
            }
        }
        public static void skipUntilYourTurn(this THHGame game, THHPlayer player)
        {
            game.skipTurnUntil(() => game.currentPlayer == player);
        }
        public static void skipUntilCanUse<T>(this THHGame game, THHPlayer you) where T : CardDefine
        {
            game.skipTurnUntil(() => game.currentPlayer == you && you.gem >= game.getCardDefine<T>().getProp<int>(nameof(ServantCardDefine.cost)));
        }
        public static void skipUntilHasGem(this THHGame game, THHPlayer you, int gem)
        {
            game.skipTurnUntil(() => game.currentPlayer == you && you.gem >= gem);
        }
        public static void skipUntilCanUse(this THHGame game, THHPlayer you, params CardDefine[] card)
        {
            game.skipTurnUntil(() => game.currentPlayer == you && you.gem >= card.Sum(c => c.getCost()));
        }
    }
}