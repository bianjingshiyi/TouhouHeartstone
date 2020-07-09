using NUnit.Framework;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TouhouCardEngine;
using TouhouHeartstone;
using TouhouHeartstone.Builtin;
using UnityEngine;
using UnityEngine.TestTools;
using System;
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

        [Test]
        public void PatchouliTrilithonShakeTest()
        {
            THHGame game = TestGameflow.initGameWithoutPlayers(null, new GameOption()
            {
                shuffle = false
            });
            THHPlayer defaultPlayer = game.createPlayer(0, "玩家0", game.getCardDefine<TestMaster2>(), Enumerable.Repeat(game.getCardDefine<DefaultServant>() as CardDefine, 30));
            THHPlayer elusivePlayer = game.createPlayer(1, "玩家1", game.getCardDefine<TestMaster2>(), Enumerable.Repeat(game.getCardDefine<DefaultServant>() as CardDefine, 28)
            .Concat(Enumerable.Repeat(game.getCardDefine<TrilithonShake>(), 2)));

            defaultPlayer.cmdTurnEnd(game);

            game.skipTurnUntil(() => elusivePlayer.gem >= game.getCardDefine<TrilithonShake>().cost && game.currentPlayer == elusivePlayer);
            elusivePlayer.cmdUse(game, elusivePlayer.hand.First(c => c.define.id == TrilithonShake.ID), 0);

            game.Dispose();
        }

        [Test]
        public void PatchouliMetalFatigueTest()
        {
            THHGame game = TestGameflow.initGameWithoutPlayers(null, new GameOption()
            {
                shuffle = false
            });
            THHPlayer defaultPlayer = game.createPlayer(0, "玩家0", game.getCardDefine<TestMaster2>(), Enumerable.Repeat(game.getCardDefine<DefaultServant>() as CardDefine, 30));
            THHPlayer elusivePlayer = game.createPlayer(1, "玩家1", game.getCardDefine<TestMaster2>(), Enumerable.Repeat(game.getCardDefine<DefaultServant>() as CardDefine, 28)
            .Concat(Enumerable.Repeat(game.getCardDefine<MetalFatigue>(), 2)));

            game.skipTurnUntil(() => defaultPlayer.gem >= game.getCardDefine<DefaultServant>().cost && game.currentPlayer == defaultPlayer);
            defaultPlayer.cmdUse(game, defaultPlayer.hand.First(c => c.define.id == DefaultServant.ID), 0);

            elusivePlayer.cmdTurnEnd(game);

            game.skipTurnUntil(() => defaultPlayer.gem >= game.getCardDefine<DefaultServant>().cost && game.currentPlayer == defaultPlayer);
            defaultPlayer.cmdUse(game, defaultPlayer.hand.First(c => c.define.id == DefaultServant.ID), 0);

            game.skipTurnUntil(() => elusivePlayer.gem >= game.getCardDefine<MetalFatigue>().cost && game.currentPlayer == elusivePlayer);
            elusivePlayer.cmdUse(game, elusivePlayer.hand.First(c => c.define.id == MetalFatigue.ID), 0);

            game.Dispose();
        }

        [Test]
        public void PatchouliKoakumaTest()
        {
            TestGameflow.createGame(out var game, out var you, out var oppo,
                new KeyValuePair<int, int>(Koakuma.ID, 1),
                new KeyValuePair<int, int>(FireBall.ID, 3)
            );//游戏初始化写的这么复杂干嘛
            for (int i = 0; i < 3; i++)
            {
                game.skipTurnUntil(() => game.currentPlayer == you && you.gem >= game.getCardDefine<FireBall>().cost);
                you.cmdUse(game, you.hand.getCard<FireBall>(), targets: oppo.master);
                you.cmdTurnEnd(game);
            }//先射三发火球
            game.skipTurnUntil(() => game.currentPlayer == you && you.gem >= game.getCardDefine<Koakuma>().cost);
            you.cmdUse(game, you.hand.getCard<Koakuma>(), targets: oppo.master);//使用小恶魔
            DiscoverRequest discoverRequest = game.answers.getRequest<DiscoverRequest>(you.id);
            Assert.NotNull(discoverRequest);//确实发现
            Assert.True(discoverRequest.cardIdArray.All(id => you.grave.Any(c => c.id == id)));//发现的全是墓地里的卡
            int cardId = discoverRequest.cardIdArray[0];
            Card card = you.grave.getCard(cardId);//随便选一张
            you.cmdDiscover(game, cardId);//选
            Assert.True(you.hand.Contains(card));//确认它在你的手牌里了
            //TODO:落叶，补充各种特殊情况，比如墓地里没有卡，手牌满了，墓地里还有随从

            game.Dispose();
        }
        public void PatchouliRoyalFlareTest()
        {

        }

        public void PatchouliSilentSeleneTest()
        {

        }
    }
}

