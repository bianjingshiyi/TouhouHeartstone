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
            TestGameflow.createGame(out var game, out var you, out var oppo,
                new KeyValuePair<int, int>(BestMagic.ID, 1),
                new KeyValuePair<int, int>(FireBall.ID, 3)
            );
            game.skipTurnUntil(() => game.currentPlayer == you && you.gem >= game.getCardDefine<BestMagic>().cost);
            you.cmdUse(game, you.hand.getCard<BestMagic>());//使用秘藏魔法
            Assert.True(game.players[0].hand.Where(c => c.define is FireBall).All(c => c.getCost() == -1));//火球术费用-5
            you.cmdUse(game, you.hand.getCard<FireBall>());
            Assert.True(game.players[0].hand.Where(c => c.define is FireBall).All(c => c.getCost() == 4));
            you.cmdTurnEnd(game);
            //Assert.True(game.players[0].hand.Where(c => c.define is FireBall).All(c => c.getCost() == 4));//回合结束，效果消失
            game.Dispose();
        }

        [Test]
        public void PatchouliMegaReaverTest()
        {
            TestGameflow.createGame(out var game, out var you, out var oppo,
                new KeyValuePair<int, int>(MegaReaver.ID, 1),
                new KeyValuePair<int, int>(FireBall.ID, 3)
            );
            game.skipTurnUntil(() => game.currentPlayer == you && you.gem >= game.getCardDefine<FireBall>().cost);
            Assert.True(game.players[0].hand.Where(c => c.define is MegaReaver).All(c => c.getCost() == 7));
            you.cmdUse(game, you.hand.getCard<FireBall>());//使用法术牌
            Assert.True(game.players[0].hand.Where(c => c.define is MegaReaver).All(c => c.getCost() == 5));//费用-2
            you.cmdTurnEnd(game);
            Assert.True(game.players[0].hand.Where(c => c.define is MegaReaver).All(c => c.getCost() == 7));//只能在当前回合生效
            game.Dispose();
        }

        [Test]
        public void PatchouliAgniShineTest()
        {
            TestGameflow.createGame(out var game, out var you, out var oppo,
                new KeyValuePair<int, int>(AgniShine.ID, 3)
            );
            game.skipTurnUntil(() => game.currentPlayer == you && you.gem >= game.getCardDefine<AgniShine>().cost);
            you.cmdUse(game, you.hand.getCard<AgniShine>(), targets: oppo.master);
            game.Dispose();
        }

        //[Test]
        public void PatchouliPrincessUndineTest()
        {

        }

        [Test]
        public void PatchouliSylphyHornTest()
        {
            TestGameflow.createGame(out var game, out var you, out var oppo,
                new KeyValuePair<int, int>(SylphyHorn.ID, 1),
                new KeyValuePair<int, int>(DefaultServant.ID, 3)
            );
            game.skipTurnUntil(() => game.currentPlayer == you && you.gem >= game.getCardDefine<SylphyHorn>().cost+1);
            you.cmdUse(game, you.hand.getCard<DefaultServant>());
            you.cmdUse(game, you.hand.getCard<SylphyHorn>(), targets: you.master);
            game.Dispose();
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
            .Concat(Enumerable.Repeat(game.getCardDefine<Boulder>(), 2)));

            defaultPlayer.cmdTurnEnd(game);

            game.skipTurnUntil(() => elusivePlayer.gem >= game.getCardDefine<Boulder>().cost && game.currentPlayer == elusivePlayer);
            elusivePlayer.cmdUse(game, elusivePlayer.hand.First(c => c.define.id == Boulder.ID), 0);

            game.skipTurnUntil(() => defaultPlayer.gem >= game.getCardDefine<DefaultServant>().cost && game.currentPlayer == defaultPlayer);
            defaultPlayer.cmdUse(game, defaultPlayer.hand.First(c => c.define.id == DefaultServant.ID), 0);

            game.skipTurnUntil(() => elusivePlayer.gem >= game.getCardDefine<TrilithonShake>().cost && game.currentPlayer == elusivePlayer);
            elusivePlayer.cmdAttack(game, elusivePlayer.field[0], defaultPlayer.field[0]);
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
                new KeyValuePair<int, int>(FireBall.ID, 3),
                new KeyValuePair<int, int>(DefaultServant.ID, 3)
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
            //game.skipTurnUntil(() => game.currentPlayer == you && you.gem >= game.getCardDefine<Koakuma>().cost);
            //you.cmdUse(game, you.hand.getCard<Koakuma>(), targets: oppo.master);//不射了，直接使用小恶魔
            //you.cmdUse(game, you.hand.getCard<DefaultServant>());
            //you.field[0].die(game);
            //you.cmdUse(game, you.hand.getCard<Koakuma>(), targets: oppo.master);
            game.Dispose();
        }

        [Test]
        public void PatchouliRoyalFlareTest()
        {
            TestGameflow.createGame(out var game, out var you, out var oppo,
            new KeyValuePair<int, int>(RoyalFlare.ID, 3)
            );
            game.skipTurnUntil(() => game.currentPlayer == you && you.gem >= game.getCardDefine<AgniShine>().cost);
            you.cmdUse(game, you.hand.getCard<RoyalFlare>());
            game.Dispose();
        }

        [Test]
        public void PatchouliElementSpriteTest()
        {
            TestGameflow.createGame(out var game, out var you, out var oppo,
            new KeyValuePair<int, int>(ElementSprite.ID, 3)
            );
            game.skipTurnUntil(() => game.currentPlayer == you && you.gem >= game.getCardDefine<ElementSprite>().cost);
            you.cmdUse(game, you.hand.getCard<ElementSprite>());
            
            game.Dispose();
        }

        [Test]
        public void PatchouliTheGreatLibraryTest()
        {
            TestGameflow.createGame(out var game, out var you, out var oppo,
                new KeyValuePair<int, int>(TheGreatLibrary.ID, 1),
                new KeyValuePair<int, int>(FireBall.ID, 20)
            );
            game.skipTurnUntil(() => game.currentPlayer == you && you.gem >= game.getCardDefine<TheGreatLibrary>().cost);
            you.cmdUse(game, you.hand.getCard<TheGreatLibrary>());
            DiscoverRequest discoverRequest = game.answers.getRequest<DiscoverRequest>(you.id);
            Assert.NotNull(discoverRequest);
            Assert.True(discoverRequest.cardIdArray.All(id => you.deck.Any(c => c.id == id)));
            int cardId = discoverRequest.cardIdArray[0];
            Card card = you.deck.getCard(cardId);
            you.cmdDiscover(game, cardId);
            Assert.True(you.hand.Contains(card));
            game.Dispose();
        }

        [Test]
        public void PatchouliLibraryProtectorTest()
        {
            TestGameflow.createGame(out var game, out var you, out var oppo,
                new KeyValuePair<int, int>(LibraryProtector.ID, 1),
                new KeyValuePair<int, int>(FireBall.ID, 20)
            );
            game.skipTurnUntil(() => game.currentPlayer == you && you.gem >= game.getCardDefine<LibraryProtector>().cost);
            you.cmdUse(game, you.hand.getCard<LibraryProtector>());
            game.Dispose();
        }

        [Test]
        public void PatchouliArcaneKnowledgeTest()
        {
            TestGameflow.createGame(out var game, out var you, out var oppo,
                new KeyValuePair<int, int>(ArcaneKnowledge.ID, 1),
                new KeyValuePair<int, int>(FireBall.ID, 20)
            );
            game.skipTurnUntil(() => game.currentPlayer == you && you.gem >= game.getCardDefine<ArcaneKnowledge>().cost);
            you.cmdUse(game, you.hand.getCard<ArcaneKnowledge>());
            game.Dispose();
        }

        [Test]
        public void PatchouliSilentSeleneTest()
        {
            THHGame game = TestGameflow.initGameWithoutPlayers(null, new GameOption(){shuffle = false});
            THHPlayer defaultPlayer = game.createPlayer(0, "玩家0", game.getCardDefine<TestMaster2>(), Enumerable.Repeat(game.getCardDefine<MissingSpecter>() as CardDefine, 30));
            THHPlayer elusivePlayer = game.createPlayer(1, "玩家1", game.getCardDefine<TestMaster2>(), Enumerable.Repeat(game.getCardDefine<SilentSelene>() as CardDefine, 28)
            .Concat(Enumerable.Repeat(game.getCardDefine<BestMagic>(), 2)));

            game.skipTurnUntil(() => game.currentPlayer == defaultPlayer && defaultPlayer.gem >= game.getCardDefine<MissingSpecter>().cost);
            defaultPlayer.cmdUse(game, defaultPlayer.hand.getCard<MissingSpecter>());

            game.skipTurnUntil(() => game.currentPlayer == elusivePlayer && elusivePlayer.gem >= 10);
            elusivePlayer.cmdUse(game, elusivePlayer.hand.getCard<BestMagic>());//使用秘藏魔法减费
            Assert.True(game.players[0].hand.Where(c => c.define is SilentSelene).All(c => c.getCost() == 7));
            elusivePlayer.cmdUse(game, elusivePlayer.hand.getCard<SilentSelene>());

            game.Dispose();
        }

        [Test]
        public void SummerRedTest()
        {
            TestGameflow.createGame(out var game, out var you, out var oppo,
                new KeyValuePair<int, int>(SummerRed.ID, 1),
                new KeyValuePair<int, int>(DefaultServant.ID, 20)
            );
            game.skipTurnUntil(() => game.currentPlayer == you && you.gem >= game.getCardDefine<DefaultServant>().cost);
            you.cmdUse(game, you.hand.getCard<DefaultServant>());
            you.cmdUse(game, you.hand.getCard<SummerRed>(), targets:you.field[0]);
            you.cmdTurnEnd(game);
            game.Dispose();
        }

        [Test]
        public void WinterSoberTest()
        {
            TestGameflow.createGame(out var game, out var you, out var oppo,
                new KeyValuePair<int, int>(WinterSober.ID, 1),
                new KeyValuePair<int, int>(DefaultServant.ID, 20)
            );
            game.skipTurnUntil(() => game.currentPlayer == you && you.gem >= game.getCardDefine<DefaultServant>().cost + 1);
            you.cmdUse(game, you.hand.getCard<DefaultServant>());
            you.cmdTurnEnd(game);
            oppo.cmdTurnEnd(game);
            you.cmdUse(game, you.hand.getCard<WinterSober>(), targets: you.field[0]);
            you.cmdAttack(game, you.field[0], oppo.master);
            game.Dispose();
        }

        [Test]
        public void SpringWindTest()
        {
            TestGameflow.createGame(out var game, out var you, out var oppo,
                new KeyValuePair<int, int>(SpringWind.ID, 1),
                new KeyValuePair<int, int>(DefaultServant.ID, 20)
            );
            game.skipTurnUntil(() => game.currentPlayer == you && you.gem >= game.getCardDefine<SummerRed>().cost + 1);
            you.cmdUse(game, you.hand.getCard<DefaultServant>());
            you.cmdUse(game, you.hand.getCard<SpringWind>(), targets: you.field[0]);
            you.cmdTurnEnd(game);
            game.Dispose();
        }

        [Test]
        public void AutumnBladeTest()
        {
            TestGameflow.createGame(out var game, out var you, out var oppo,
                new KeyValuePair<int, int>(AutumnBlade.ID, 1),
                new KeyValuePair<int, int>(DefaultServant.ID, 20)
            );
            game.skipTurnUntil(() => game.currentPlayer == oppo && oppo.gem >= 3);
            for(int i=0;i<3;i++)
            {
                oppo.cmdUse(game, oppo.hand.getCard<DefaultServant>(),i);
            }
            oppo.cmdTurnEnd(game);
            you.cmdUse(game, you.hand.getCard<AutumnBlade>());
            game.Dispose();
        }

        [Test]
        public void EarthGunTest()
        {
            TestGameflow.createGame(out var game, out var you, out var oppo,
                new KeyValuePair<int, int>(EarthGun.ID, 1),
                new KeyValuePair<int, int>(DefaultServant.ID, 20)
            );
            game.skipTurnUntil(() => game.currentPlayer == you && you.gem >= game.getCardDefine<SummerRed>().cost + 1);
            you.cmdUse(game, you.hand.getCard<EarthGun>());
            game.Dispose();
        }

        [Test]
        public void ElementHarvesterTest()
        {
            TestGameflow.createGame(out var game, out var you, out var oppo,
                new KeyValuePair<int, int>(ElementHarvester.ID, 1),
                new KeyValuePair<int, int>(EarthGun.ID, 20)
            );
            game.skipTurnUntil(() => game.currentPlayer == you && you.gem >= 3);
            for (int i = 0; i < 3; i++)
            {
                you.cmdUse(game, you.hand.getCard<EarthGun>());
            }
            you.cmdTurnEnd(game);
            for (int i = 0; i < 3; i++)
            {
                oppo.cmdUse(game, oppo.hand.getCard<DefaultServant>(),i);
                oppo.field[i].setCurrentLife(2);
            }
            oppo.cmdTurnEnd(game);
            you.cmdUse(game, you.hand.getCard<ElementHarvester>());
            game.Dispose();
        }

        [Test]
        public void MercuryPoisonTest()
        {
            TestGameflow.createGame(out var game, out var you, out var oppo,
                new KeyValuePair<int, int>(MercuryPoison.ID, 1),
                new KeyValuePair<int, int>(DefaultServant.ID, 20)
            );
            game.skipTurnUntil(() => game.currentPlayer == oppo && oppo.gem >= 3);
            for (int i = 0; i < 3; i++)
            {
                oppo.cmdUse(game, oppo.hand.getCard<DefaultServant>(),i);
            }
            oppo.field[0].setAttack(5);
            oppo.cmdTurnEnd(game);
            you.cmdUse(game, you.hand.getCard<MercuryPoison>());
            you.cmdTurnEnd(game);
            oppo.cmdTurnEnd(game);
            game.Dispose();
        }

        [Test]
        public void ForestFireTest()
        {
            TestGameflow.createGame(out var game, out var you, out var oppo,
                new KeyValuePair<int, int>(ForestFire.ID, 1),
                new KeyValuePair<int, int>(DefaultServant.ID, 20)
            );
            /*game.skipTurnUntil(() => game.currentPlayer == oppo && oppo.gem >= 3);
            for (int i = 0; i < 3; i++)
            {
                oppo.cmdUse(game, oppo.hand.getCard<DefaultServant>(), i);
            }
            oppo.cmdTurnEnd(game);*/
            game.skipTurnUntil(() => game.currentPlayer == you && you.gem >= 9);
            for (int i = 0; i < 3; i++)
            {
                you.cmdUse(game, you.hand.getCard<DefaultServant>(), i);
            }
            you.cmdUse(game, you.hand.getCard<ForestFire>(),targets:oppo.master);
            game.Dispose();
        }

        [Test]
        public void SproutingWalkingDeadTest()
        {
            TestGameflow.createGame(out var game, out var you, out var oppo,
                new KeyValuePair<int, int>(SproutingWalkingDead.ID, 1),
                new KeyValuePair<int, int>(DefaultServant.ID, 20)
            );
            game.skipTurnUntil(() => game.currentPlayer == you && you.gem >= 10);
            you.cmdUse(game, you.hand.getCard<DefaultServant>());
            you.cmdUse(game, you.hand.getCard<SproutingWalkingDead>(), targets: you.field[0]);
            you.field[0].die(game);
            you.cmdTurnEnd(game);
            game.Dispose();
        }

        [Test]
        public void WaterSpiritTest()
        {
            TestGameflow.createGame(out var game, out var you, out var oppo,
                new KeyValuePair<int, int>(WaterSpirit.ID, 1),
                new KeyValuePair<int, int>(DefaultServant.ID, 20)
            );
            game.skipTurnUntil(() => game.currentPlayer == you && you.gem >= 7);
            you.cmdUse(game, you.hand.getCard<DefaultServant>());
            you.field[0].damage(game, you.master, 4);
            you.cmdUse(game, you.hand.getCard<WaterSpirit>(), targets: you.field[0]);
            Assert.True(you.field[0].getCurrentLife() == 13);
            game.Dispose();
        }

        [Test]
        public void BurningRainTest()
        {
            TestGameflow.createGame(out var game, out var you, out var oppo,
                new KeyValuePair<int, int>(BurningRain.ID, 1),
                new KeyValuePair<int, int>(DefaultServant.ID, 20)
            );
            game.skipTurnUntil(() => game.currentPlayer == oppo && oppo.gem >= 3);
            for (int i = 0; i < 3; i++)
            {
                oppo.cmdUse(game, oppo.hand.getCard<DefaultServant>(), i);
            }
            oppo.cmdTurnEnd(game);

            you.cmdUse(game, you.hand.getCard<BurningRain>());
            game.Dispose();
        }

        [Test]
        public void SanElmoFirePillarTest()
        {
            TestGameflow.createGame(out var game, out var you, out var oppo,
                new KeyValuePair<int, int>(SanElmoFirePillar.ID, 1),
                new KeyValuePair<int, int>(DefaultServant.ID, 20)
            );
            game.skipTurnUntil(() => game.currentPlayer == oppo && oppo.gem >= 3);
            for (int i = 0; i < 3; i++)
            {
                oppo.cmdUse(game, oppo.hand.getCard<DefaultServant>(), i);
            }
            oppo.cmdTurnEnd(game);

            you.cmdUse(game, you.hand.getCard<SanElmoFirePillar>(),targets:oppo.field[1]);
            game.Dispose();
        }

        [Test]
        public void RingLavaTest()
        {
            TestGameflow.createGame(out var game, out var you, out var oppo,
                new KeyValuePair<int, int>(RingLava.ID, 1),
                new KeyValuePair<int, int>(DefaultServant.ID, 20)
            );
            game.skipTurnUntil(() => game.currentPlayer == oppo && oppo.gem >= 3);
            for (int i = 0; i < 3; i++)
            {
                oppo.cmdUse(game, oppo.hand.getCard<DefaultServant>(), i);
            }
            oppo.cmdTurnEnd(game);

            you.cmdUse(game, you.hand.getCard<RingLava>());
            you.cmdTurnEnd(game);
            oppo.cmdTurnEnd(game);
            you.cmdTurnEnd(game);
            game.Dispose();
        }

        [Test]
        public void LightyellowgustTest()
        {
            TestGameflow.createGame(out var game, out var you, out var oppo,
                new KeyValuePair<int, int>(Lightyellowgust.ID, 1),
                new KeyValuePair<int, int>(DefaultServant.ID, 20)
            );
            game.skipTurnUntil(() => game.currentPlayer == oppo && oppo.gem >= 3);
            for (int i = 0; i < 3; i++)
            {
                oppo.cmdUse(game, oppo.hand.getCard<DefaultServant>(), i);
            }
            oppo.cmdTurnEnd(game);

            you.cmdUse(game, you.hand.getCard<Lightyellowgust>());
            you.cmdAttack(game, you.field[0], oppo.field[1]);
            you.cmdTurnEnd(game);
            game.Dispose();
        }

        [Test]
        public void NoahFloodTest()
        {
            TestGameflow.createGame(out var game, out var you, out var oppo,
                new KeyValuePair<int, int>(NoahFlood.ID, 1),
                new KeyValuePair<int, int>(MissingSpecter.ID, 20)
            );
            game.skipTurnUntil(() => game.currentPlayer == you && you.gem >= 9);
            you.cmdUse(game, you.hand.getCard<MissingSpecter>());
            you.cmdUse(game, you.hand.getCard<NoahFlood>());
            game.Dispose();
        }
    }
}

