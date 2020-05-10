using NUnit.Framework;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TouhouHeartstone;
using TouhouHeartstone.Builtin;
using UnityEngine;
using UnityEngine.TestTools;

namespace Tests
{
    public class PatchouliCardTests
    {
        [Test]
        public void PatchouliSkillTest()
        {
            THHGame game = TestGameflow.initStandardGame(null, new int[] { 0, 1 },
                Enumerable.Repeat(new Patchouli(), 2).ToArray(),
                Enumerable.Repeat(Enumerable.Repeat(new RashFairy(), 30).ToArray(), 2).ToArray(),
                new GameOption() { });
            game.run();
            game.sortedPlayers[0].cmdInitReplace(game);
            game.sortedPlayers[1].cmdInitReplace(game);
            game.sortedPlayers[0].cmdTurnEnd(game);
            game.sortedPlayers[1].cmdTurnEnd(game);
            game.sortedPlayers[0].cmdTurnEnd(game);

            int preHand = game.sortedPlayers[1].hand.count;
            int preDeck = game.sortedPlayers[1].deck.count;
            int preLife = game.sortedPlayers[1].master.getCurrentLife();
            game.sortedPlayers[1].cmdUse(game, game.sortedPlayers[1].skill, 0, game.sortedPlayers[1].master);
            
            Assert.AreEqual(1, game.sortedPlayers[1].hand.count - preHand);
            Assert.AreEqual(-1, game.sortedPlayers[1].deck.count - preDeck);
            Assert.AreEqual(-2, game.sortedPlayers[1].master.getCurrentLife() - preLife);
            
        }

        [Test]
        public void PatchouliBestMagicTest()
        {
            THHGame game = TestGameflow.initStandardGame(null, new int[] { 0, 1 },
                Enumerable.Repeat(new Patchouli(), 2).ToArray(),
                Enumerable.Repeat(Enumerable.Repeat(new BestMagic(), 30).ToArray(), 2).ToArray(),
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

            game.sortedPlayers[1].cmdUse(game, game.sortedPlayers[1].hand[0], 0);
            int preGem = game.sortedPlayers[1].gem;
            game.sortedPlayers[1].cmdUse(game, game.sortedPlayers[1].hand[0], 0);
            Assert.AreEqual(1, preGem - game.sortedPlayers[1].gem);
        }

        [Test]
        public void PatchouliMegaReaverTest()
        {
            THHGame game = TestGameflow.initStandardGame(null, new int[] { 0, 1 },
                Enumerable.Repeat(new Patchouli(), 2).ToArray(),
                Enumerable.Repeat(Enumerable.Repeat(new MegaReaver(), 30).ToArray(), 2).ToArray(),
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

            int preHand = game.sortedPlayers[1].hand.count;
            game.sortedPlayers[1].cmdUse(game, game.sortedPlayers[1].hand[0], 0);
            if (preHand <= 3)
                Assert.AreEqual(0, game.sortedPlayers[1].hand.count);
            else
                Assert.AreEqual(3, preHand - game.sortedPlayers[1].hand.count);
        }

        [Test]
        public void PatchouliPhilosopherStoneTest()
        {

        }

        [Test]
        public void PatchouliAgniShineTest()
        {
            TouhouCardEngine.CardDefine[][] decks = new TouhouCardEngine.CardDefine[][]
            {
                Enumerable.Repeat(new RashFairy(), 30).ToArray(),
                Enumerable.Repeat(new AgniShine(), 30).ToArray()
            };
            THHGame game = TestGameflow.initStandardGame(null, new int[] { 0, 1 },
                Enumerable.Repeat(new Patchouli(), 2).ToArray(),
                decks,
                new GameOption() { });
            game.run();
            game.sortedPlayers[0].cmdInitReplace(game);
            game.sortedPlayers[1].cmdInitReplace(game);
            game.sortedPlayers[0].cmdUse(game, game.sortedPlayers[0].hand[0], 0);
            game.sortedPlayers[0].cmdTurnEnd(game);
            game.sortedPlayers[1].cmdTurnEnd(game);
            game.sortedPlayers[0].cmdUse(game, game.sortedPlayers[0].hand[0], 0);
            game.sortedPlayers[0].cmdTurnEnd(game);
            game.sortedPlayers[1].cmdTurnEnd(game);
            game.sortedPlayers[0].cmdTurnEnd(game);
            game.sortedPlayers[1].cmdTurnEnd(game);
            game.sortedPlayers[0].cmdTurnEnd(game);
            game.sortedPlayers[1].cmdTurnEnd(game);
            game.sortedPlayers[0].cmdTurnEnd(game);
            
            game.sortedPlayers[1].cmdUse(game, game.sortedPlayers[1].hand[0], 0);
            THHCard.DamageEventArg damage = game.triggers.getRecordedEvents().LastOrDefault(e => e is THHCard.DamageEventArg) as THHCard.DamageEventArg;
            Assert.AreEqual(0, game.sortedPlayers[0].field.count);
            Assert.AreEqual(4, damage.value);
        }

        [Test]
        public void PatchouliPrincessUndineTest()
        {
            TouhouCardEngine.CardDefine[][] decks = new TouhouCardEngine.CardDefine[][]
            {
                Enumerable.Repeat(new RoyalFlare(), 30).ToArray(),
                Enumerable.Repeat(new AgniShine(), 30).ToArray()
            };
            THHGame game = TestGameflow.initStandardGame(null, new int[] { 0, 1 },
                Enumerable.Repeat(new Patchouli(), 2).ToArray(),
                decks,
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
            game.sortedPlayers[0].cmdTurnEnd(game);
            game.sortedPlayers[1].cmdTurnEnd(game);
            game.sortedPlayers[0].cmdTurnEnd(game);

            game.sortedPlayers[1].cmdUse(game, game.sortedPlayers[1].hand[0], 0);
            Assert.AreEqual(1, game.sortedPlayers[1].field.count);
            Assert.AreEqual(0, game.sortedPlayers[1].field[0].getAttack());
            Assert.AreEqual(8, game.sortedPlayers[1].field[0].getLife());
            game.sortedPlayers[1].cmdTurnEnd(game);
            int preLife = game.sortedPlayers[1].master.getCurrentLife();
            game.sortedPlayers[0].cmdUse(game, game.sortedPlayers[0].hand[0], 0, game.sortedPlayers[1].master);
            Assert.AreEqual(1, preLife - game.sortedPlayers[1].master.getCurrentLife());
        }

        [Test]
        public void PatchouliSylphyHornTest()
        {
            TouhouCardEngine.CardDefine[][] decks = new TouhouCardEngine.CardDefine[][]
            {
                Enumerable.Repeat(new RashFairy(), 30).ToArray(),
                Enumerable.Repeat(new AgniShine(), 30).ToArray()
            };
            THHGame game = TestGameflow.initStandardGame(null, new int[] { 0, 1 },
                Enumerable.Repeat(new Patchouli(), 2).ToArray(),
                decks,
                new GameOption() { });
            game.run();
            game.sortedPlayers[0].cmdInitReplace(game);
            game.sortedPlayers[1].cmdInitReplace(game);
            game.sortedPlayers[0].cmdUse(game, game.sortedPlayers[0].hand[0], 0);
            game.sortedPlayers[0].cmdTurnEnd(game);
            game.sortedPlayers[1].cmdTurnEnd(game);
            game.sortedPlayers[0].cmdUse(game, game.sortedPlayers[0].hand[0], 0);
            game.sortedPlayers[0].cmdTurnEnd(game);
            game.sortedPlayers[1].cmdTurnEnd(game);
            game.sortedPlayers[0].cmdTurnEnd(game);
            game.sortedPlayers[1].cmdTurnEnd(game);
            game.sortedPlayers[0].cmdTurnEnd(game);
            game.sortedPlayers[1].cmdTurnEnd(game);
            game.sortedPlayers[0].cmdTurnEnd(game);

        }

        public void PatchouliTrilithonShakeTest()
        {

        }

        public void PatchouliMetalFatigueTest()
        {

        }

        public void PatchouliKoakumaTest()
        {

        }

        public void PatchouliRoyalFlareTest()
        {

        }

        public void PatchouliSilentSeleneTest()
        {

        }
    }
}

