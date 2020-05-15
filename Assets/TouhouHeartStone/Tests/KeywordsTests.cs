using NUnit.Framework;
using UnityEngine;
using System.Linq;
using TouhouHeartstone;
using TouhouCardEngine;
using TouhouHeartstone.Builtin;

namespace Tests
{
    public class KeywordsTests
    {
        [Test]
        public void RushTest()
        {
            THHGame game = TestGameflow.initGameWithoutPlayers(null, new GameOption()
            {
                shuffle = false
            });
            game.createPlayer(0, "玩家0", game.getCardDefine<Reimu>(), Enumerable.Repeat(game.getCardDefine<DefaultServant>() as CardDefine, 29)
            .Concat(Enumerable.Repeat(game.getCardDefine<RushServant>(), 1)));
            game.createPlayer(1, "玩家1", game.getCardDefine<Reimu>(), Enumerable.Repeat(game.getCardDefine<DefaultServant>() as CardDefine, 29)
            .Concat(Enumerable.Repeat(game.getCardDefine<RushServant>(), 1)));
            game.run();
            game.sortedPlayers[0].cmdInitReplace(game);
            game.sortedPlayers[1].cmdInitReplace(game);


            game.sortedPlayers[0].cmdTurnEnd(game);
            game.sortedPlayers[1].cmdUse(game, game.sortedPlayers[1].hand[1], 0);
            game.sortedPlayers[1].cmdTurnEnd(game);
            game.sortedPlayers[0].cmdUse(game, game.sortedPlayers[0].hand[0], 0);
            game.sortedPlayers[0].cmdUse(game, game.sortedPlayers[0].hand[1], 1);

            Assert.True(game.sortedPlayers[0].field[0].isRush());
            game.sortedPlayers[0].cmdAttack(game, game.sortedPlayers[0].field[1], game.sortedPlayers[1].master);
            Assert.AreEqual(30, game.sortedPlayers[1].master.getCurrentLife());
            game.sortedPlayers[0].cmdAttack(game, game.sortedPlayers[0].field[0], game.sortedPlayers[1].master);
            Assert.AreEqual(30, game.sortedPlayers[1].master.getCurrentLife());
            game.sortedPlayers[0].cmdAttack(game, game.sortedPlayers[0].field[0], game.sortedPlayers[1].field[0]);
            Debug.Log(game.sortedPlayers[1].field[0].getCurrentLife());
            Assert.AreEqual(6, game.sortedPlayers[1].field[0].getCurrentLife());
        }

        [Test]
        public void ShieldTest()
        {
            THHGame game = TestGameflow.initGameWithoutPlayers(null, new GameOption()
            {
                shuffle = false
            });
            game.createPlayer(0, "玩家0", game.getCardDefine<Reimu>(), Enumerable.Repeat(game.getCardDefine<DefaultServant>() as CardDefine, 29)
            .Concat(Enumerable.Repeat(game.getCardDefine<ShieldServant>(), 1)));
            game.createPlayer(1, "玩家1", game.getCardDefine<Reimu>(), Enumerable.Repeat(game.getCardDefine<DefaultServant>() as CardDefine, 29)
            .Concat(Enumerable.Repeat(game.getCardDefine<ShieldServant>(), 1)));
            game.run();
            game.sortedPlayers[0].cmdInitReplace(game);
            game.sortedPlayers[1].cmdInitReplace(game);

            game.sortedPlayers[0].cmdTurnEnd(game);
            game.sortedPlayers[1].cmdUse(game, game.sortedPlayers[1].hand[1], 0);
            game.sortedPlayers[1].cmdTurnEnd(game);
            game.sortedPlayers[0].cmdUse(game, game.sortedPlayers[0].hand[1], 0);
            game.sortedPlayers[0].cmdUse(game, game.sortedPlayers[0].hand[0], 1);
            game.sortedPlayers[0].cmdTurnEnd(game);
            game.sortedPlayers[1].cmdTurnEnd(game);

            game.sortedPlayers[0].cmdAttack(game, game.sortedPlayers[0].field[0], game.sortedPlayers[1].field[0]);
            Assert.AreEqual(6, game.sortedPlayers[0].field[0].getCurrentLife());
            game.sortedPlayers[0].cmdAttack(game, game.sortedPlayers[0].field[1], game.sortedPlayers[1].field[0]);
            Assert.AreEqual(1, game.sortedPlayers[0].field[1].getCurrentLife());
        }

        [Test]
        public void StealthTest()
        {
            THHGame game = TestGameflow.initGameWithoutPlayers(null, new GameOption()
            {
                shuffle = false
            });
            game.createPlayer(0, "玩家0", game.getCardDefine<Reimu>(), Enumerable.Repeat(game.getCardDefine<DefaultServant>() as CardDefine, 29)
            .Concat(Enumerable.Repeat(game.getCardDefine<StealthServant>(), 1)));
            game.createPlayer(1, "玩家1", game.getCardDefine<Reimu>(), Enumerable.Repeat(game.getCardDefine<DefaultServant>() as CardDefine, 29)
            .Concat(Enumerable.Repeat(game.getCardDefine<StealthServant>(), 1)));
            game.run();
            game.sortedPlayers[0].cmdInitReplace(game);
            game.sortedPlayers[1].cmdInitReplace(game);

            game.sortedPlayers[0].cmdTurnEnd(game);
            game.sortedPlayers[1].cmdUse(game, game.sortedPlayers[1].hand[1], 0);
            game.sortedPlayers[1].cmdTurnEnd(game);
            game.sortedPlayers[0].cmdUse(game, game.sortedPlayers[0].hand[1], 0);
            game.sortedPlayers[0].cmdUse(game, game.sortedPlayers[0].hand[0], 1);
            Assert.True(game.sortedPlayers[0].field[1].isStealth());
            game.sortedPlayers[0].cmdTurnEnd(game);


            game.sortedPlayers[1].cmdAttack(game, game.sortedPlayers[1].field[0], game.sortedPlayers[0].field[1]);
            //game.sortedPlayers[1].cmdAttack(game, game.sortedPlayers[1].field[0], game.sortedPlayers[1].master);

            Assert.AreEqual(7, game.sortedPlayers[1].field[0].getCurrentLife());
            game.sortedPlayers[1].cmdAttack(game, game.sortedPlayers[1].field[0], game.sortedPlayers[0].field[0]);
            Assert.AreEqual(6, game.sortedPlayers[1].field[0].getCurrentLife());
            game.sortedPlayers[1].cmdTurnEnd(game);
            game.sortedPlayers[0].cmdAttack(game, game.sortedPlayers[0].field[1], game.sortedPlayers[1].field[0]);
            Assert.False(game.sortedPlayers[0].field[1].isStealth());
        }
    }
}
