using System.Collections;
using NUnit.Framework;
using UnityEngine.TestTools;
using System.Linq;
using TouhouHeartstone;
using TouhouCardEngine;
using TouhouHeartstone.Builtin;

namespace Tests
{
    public class NeutralCardTests
    {
        [Test]
        public void rashFairyTest()
        {
            THHGame game = TestGameflow.initStandardGame(null, new int[] { 0, 1 },
            Enumerable.Repeat(new Reimu(), 2).ToArray(),
            Enumerable.Repeat(Enumerable.Repeat(new RashFairy(), 30).ToArray(), 2).ToArray(),
            new GameOption() { });
            game.run();
            game.sortedPlayers[0].cmdInitReplace(game);
            game.sortedPlayers[1].cmdInitReplace(game);

            game.sortedPlayers[0].cmdUse(game, game.sortedPlayers[0].hand[0], 0);
            Assert.True(game.sortedPlayers[0].field[0].isCharge());
            Assert.True(game.sortedPlayers[0].field[0].isReady());
        }
        [Test]
        public void humanVillageGuardTest()
        {
            THHGame game = TestGameflow.initStandardGame(null, new int[] { 0, 1 },
            Enumerable.Repeat(new Reimu(), 2).ToArray(),
            Enumerable.Repeat(Enumerable.Repeat(new HumanVillageGuard(), 30).ToArray(), 2).ToArray(),
            new GameOption() { });
            game.run();
            game.sortedPlayers[0].cmdInitReplace(game);
            game.sortedPlayers[1].cmdInitReplace(game);

            game.sortedPlayers[0].cmdTurnEnd(game);
            game.sortedPlayers[1].cmdTurnEnd(game);
            game.sortedPlayers[0].cmdTurnEnd(game);
            game.sortedPlayers[1].cmdTurnEnd(game);

            game.sortedPlayers[0].cmdUse(game, game.sortedPlayers[0].hand[0], 0);
            Assert.True(game.sortedPlayers[0].field[0].isTaunt());
        }
        [Test]
        public void rifleHunterTest()
        {
            THHGame game = TestGameflow.initStandardGame(null, new int[] { 0, 1 },
            Enumerable.Repeat(new Reimu(), 2).ToArray(),
            Enumerable.Repeat(Enumerable.Repeat(new RifleHunter(), 30).ToArray(), 2).ToArray(),
            new GameOption() { });
            game.run();
            game.sortedPlayers[0].cmdInitReplace(game);
            game.sortedPlayers[1].cmdInitReplace(game);

            game.sortedPlayers[0].cmdTurnEnd(game);
            game.sortedPlayers[1].cmdTurnEnd(game);
            game.sortedPlayers[0].cmdTurnEnd(game);
            game.sortedPlayers[1].cmdTurnEnd(game);

            game.sortedPlayers[0].cmdUse(game, game.sortedPlayers[0].hand[0], 0, game.sortedPlayers[1].master);
            Assert.AreEqual(28, game.sortedPlayers[1].master.getCurrentLife());
        }
        [Test]
        public void missingSpecterTest()
        {
            THHGame game = TestGameflow.initStandardGame(null, new int[] { 0, 1 },
            Enumerable.Repeat(new Reimu(), 2).ToArray(),
            Enumerable.Repeat(Enumerable.Repeat(new MissingSpecter(), 30).ToArray(), 2).ToArray(),
            new GameOption() { });
            game.run();
            game.sortedPlayers[0].cmdInitReplace(game);
            game.sortedPlayers[1].cmdInitReplace(game);

            game.sortedPlayers[0].cmdTurnEnd(game);
            game.sortedPlayers[1].cmdTurnEnd(game);
            game.sortedPlayers[0].cmdTurnEnd(game);
            game.sortedPlayers[1].cmdTurnEnd(game);
            game.sortedPlayers[0].cmdTurnEnd(game);
            game.sortedPlayers[1].cmdTurnEnd(game);
            game.sortedPlayers[0].cmdTurnEnd(game);
            game.sortedPlayers[1].cmdTurnEnd(game);

            game.sortedPlayers[0].cmdUse(game, game.sortedPlayers[0].hand[0], 0);
            game.sortedPlayers[0].cmdTurnEnd(game);
            game.sortedPlayers[1].cmdUse(game, game.sortedPlayers[1].hand[0], 0);
            game.sortedPlayers[1].cmdTurnEnd(game);
            game.sortedPlayers[0].cmdAttack(game, game.sortedPlayers[0].field[0], game.sortedPlayers[1].field[0]);

            Assert.AreEqual(1, game.sortedPlayers[0].field.count);
            Assert.AreEqual(1, game.sortedPlayers[1].field.count);
        }
        [Test]
        public void drizzleFairyTest()
        {
            THHGame game = TestGameflow.initGameWithoutPlayers(null, new GameOption()
            {
                shuffle = false
            });
            game.createPlayer(0, "玩家0", game.getCardDefine<Reimu>(), Enumerable.Repeat(game.getCardDefine<DrizzleFairy>() as CardDefine, 29)
            .Concat(Enumerable.Repeat(game.getCardDefine<FantasySeal>(), 1)));
            game.createPlayer(1, "玩家1", game.getCardDefine<Reimu>(), Enumerable.Repeat(game.getCardDefine<DrizzleFairy>() as CardDefine, 29)
            .Concat(Enumerable.Repeat(game.getCardDefine<FantasySeal>(), 1)));
            game.run();
            game.sortedPlayers[0].cmdInitReplace(game);
            game.sortedPlayers[1].cmdInitReplace(game);

            //第一回合
            game.sortedPlayers[0].cmdUse(game, game.sortedPlayers[0].hand[1], 0);

            game.sortedPlayers[0].cmdTurnEnd(game);

            game.sortedPlayers[1].cmdUse(game, game.sortedPlayers[1].hand[1], 0);

            game.sortedPlayers[1].cmdTurnEnd(game);
            //第二回合
            game.sortedPlayers[0].cmdUse(game, game.sortedPlayers[0].hand[1], 0);
            game.sortedPlayers[0].cmdUse(game, game.sortedPlayers[0].hand[1], 0);

            game.sortedPlayers[0].cmdTurnEnd(game);
            game.sortedPlayers[1].cmdTurnEnd(game);
            //第三回合，共计4个妖精
            game.sortedPlayers[0].cmdUse(game, game.sortedPlayers[0].hand[1], 0);

            game.sortedPlayers[0].cmdTurnEnd(game);
            game.sortedPlayers[1].cmdTurnEnd(game);
            //第四回合
            game.sortedPlayers[0].cmdTurnEnd(game);

            game.sortedPlayers[1].cmdUse(game, game.sortedPlayers[1].hand[0], 0);

            //对3个随从造成3点伤害
            THHCard.DamageEventArg damage = game.triggers.getRecordedEvents().LastOrDefault(e => e is THHCard.DamageEventArg) as THHCard.DamageEventArg;
            Assert.AreEqual(3, damage.cards.Length);
            Assert.AreEqual(3, damage.value);
            Assert.AreEqual(1, game.sortedPlayers[0].field.count);
        }
    }
}
