using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using System.Linq;
using TouhouHeartstone;
using Game;
namespace Tests
{
    public class AITests
    {
        [UnityTest]
        public IEnumerator calcActionsTest_Turn1()
        {
            THHGame game = TestGameflow.initGameWithoutPlayers(null, GameOption.Default);
            THHPlayer player = game.createPlayer(1, "玩家1", game.getCardDefine<TestMaster>(), Enumerable.Repeat(game.getCardDefine<TestServant>(), 30));
            game.createPlayer(2, "玩家2", game.getCardDefine<TestMaster>(), Enumerable.Repeat(game.getCardDefine<TestServant>(), 30));
            game.logger.log("Force Init");
            AI ai = new AI(game, player, false);
            _ = game.run();
            yield return new WaitForSeconds(.1f);
            game.players[0].cmdInitReplace(game);
            game.players[1].cmdInitReplace(game);
            yield return new WaitForSeconds(.1f);
            if (game.sortedPlayers[0] != player)
            {
                game.sortedPlayers[0].cmdTurnEnd(game);
                yield return new WaitForSeconds(.1f);
            }
            var actions = ai.calcActions(game, player);
            Assert.AreEqual(player.hand.count, actions.Count);
            UseResponse use = actions.Keys.First() as UseResponse;
            Assert.NotNull(use);
        }
        [UnityTest]
        public IEnumerator run_InitReplaceTest()
        {
            THHGame game = TestGameflow.initGameWithoutPlayers(null, GameOption.Default);
            THHPlayer player = game.createPlayer(1, "玩家1", game.getCardDefine<TestMaster>(), Enumerable.Repeat(game.getCardDefine<TestServant>(), 30));
            game.createPlayer(2, "玩家2", game.getCardDefine<TestMaster>(), Enumerable.Repeat(game.getCardDefine<TestServant>(), 30));
            AI ai = new AI(game, player);
            _ = game.run();
            yield return new WaitForSeconds(.1f);
            game.players[1].cmdInitReplace(game);
            yield return new WaitForSeconds(1f);
            Assert.True(game.triggers.getRecordedEvents().Any(e => e is THHGame.StartEventArg));
        }
        //[UnityTest]
        //public IEnumerator run_Turn1()
        //{
        //    THHGame game = TestGameflow.initGameWithoutPlayers(null, GameOption.Default);
        //    THHPlayer player = game.createPlayer(1, "玩家1", game.getCardDefine<TestMaster>(), Enumerable.Repeat(game.getCardDefine<TestServant>(), 30));
        //    game.createPlayer(2, "玩家2", game.getCardDefine<TestMaster>(), Enumerable.Repeat(game.getCardDefine<TestServant>(), 30));
        //    AI ai = new AI(game, player);
        //    _ = game.run();
        //    yield return new WaitForSeconds(.1f);
        //    game.players[1].cmdInitReplace(game);
        //    yield return new WaitForSeconds(1f);
        //    if (game.sortedPlayers[0] != player)
        //    {
        //        game.sortedPlayers[0].cmdTurnEnd(game);
        //        yield return new WaitForSeconds(.1f);
        //    }
        //    yield return new WaitForSeconds(1f);
        //    Assert.True(game.triggers.getRecordedEvents().Any(e => e is THHPlayer.UseEventArg));
        //}
    }
}