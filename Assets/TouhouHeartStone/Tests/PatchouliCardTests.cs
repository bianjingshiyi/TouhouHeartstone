using NUnit.Framework;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TouhouCardEngine;
using TouhouHeartstone;
using TouhouHeartstone.Builtin;
using UnityEngine;
using System;
using BJSYGameCore;

namespace Tests
{
    public class PatchouliCardTests
    {
        //过期测试
        //[Test]
        //public void PatchouliSkillTest()
        //{
        //    THHGame game = TestGameflow.initStandardGame(null, new int[] { 0, 1 },
        //        Enumerable.Repeat(new Patchouli(), 2).ToArray(),
        //        Enumerable.Repeat(Enumerable.Repeat(new RashFairy(), 30).ToArray(), 2).ToArray(),
        //        new GameOption() { });
        //    game.run();
        //    game.sortedPlayers[0].cmdInitReplace(game);
        //    game.sortedPlayers[1].cmdInitReplace(game);
        //    game.sortedPlayers[0].cmdTurnEnd(game);
        //    game.sortedPlayers[1].cmdTurnEnd(game);
        //    game.sortedPlayers[0].cmdTurnEnd(game);

        //    int preHand = game.sortedPlayers[1].hand.count;
        //    int preDeck = game.sortedPlayers[1].deck.count;
        //    int preLife = game.sortedPlayers[1].master.getCurrentLife(game);
        //    game.sortedPlayers[1].cmdUse(game, game.sortedPlayers[1].skill, 0, game.sortedPlayers[1].master);

        //    Assert.AreEqual(1, game.sortedPlayers[1].hand.count - preHand);
        //    Assert.AreEqual(-1, game.sortedPlayers[1].deck.count - preDeck);
        //    Assert.AreEqual(-2, game.sortedPlayers[1].master.getCurrentLife(game) - preLife);

        //}
        [Test]
        public void SummerFireTest()
        {
            TestGameflow.createGame(out var game, out var you, out var oppo, Patchouli.ID);
            game.skipUntilCanUse<SummerFire>(you);
            you.cmdUse(game, you.getSkill(), 0, oppo.getMaster());
            Assert.AreEqual(oppo.getMaster().getLife(game) - 1, oppo.getMaster().getCurrentLife(game));
            game.Dispose();
        }

        [Test]
        public void BestMagicTest()
        {
            TestGameflow.createGame(out var game, out var you, out var oppo,
                new KeyValuePair<int, int>(BestMagic.ID, 1),
                new KeyValuePair<int, int>(FireBall.ID, 3)
            );
            game.skipTurnUntil(() => game.getCurrentPlayer() == you && you.getGem() >= game.getDefine<BestMagic>().cost);
            you.cmdUse(game, you.getHand().getCard<BestMagic>());//使用秘藏魔法
            Assert.True(you.getHand().Where(c => c.define is FireBall).All(c => c.getCost(game) == 0));//火球术费用-5
            you.cmdUse(game, you.getHand().getCard<FireBall>(), 0, you.getMaster());
            Assert.True(you.getHand().Where(c => c.define is FireBall).All(c => c.getCost(game) == 4));
            you.cmdTurnEnd(game);
            //Assert.True(game.players[0].hand.Where(c => c.define is FireBall).All(c => c.getCost(game) == 4));//回合结束，效果消失
            game.Dispose();
        }

        [Test]
        public void MegaReaverTest()
        {
            TestGameflow.createGame(out var game, out var you, out var oppo,
                new KeyValuePair<int, int>(MegaReaver.ID, 1),
                new KeyValuePair<int, int>(FireBall.ID, 3)
            );
            game.skipTurnUntil(() => game.getCurrentPlayer() == you && you.getGem() >= game.getDefine<FireBall>().cost);
            Assert.True(game.players[0].getHand().Where(c => c.define is MegaReaver).All(c => c.getCost(game) == 7));
            you.cmdUse(game, you.getHand().getCard<FireBall>(), 0, you.getMaster());//使用法术牌
            Assert.True(game.players[0].getHand().Where(c => c.define is MegaReaver).All(c => c.getCost(game) == 5));//费用-2
            you.cmdTurnEnd(game);
            Assert.True(game.players[0].getHand().Where(c => c.define is MegaReaver).All(c => c.getCost(game) == 7));//只能在当前回合生效
            game.Dispose();
        }

        [Test]
        public void AgniShineTest()
        {
            TestGameflow.createGame(out var game, out var you, out var oppo,
                new KeyValuePair<int, int>(AgniShine.ID, 3)
            );
            game.skipTurnUntil(() => game.getCurrentPlayer() == you && you.getGem() >= game.getDefine<AgniShine>().cost);
            you.cmdUse(game, you.getHand().getCard<AgniShine>(), targets: oppo.getMaster());
            game.Dispose();
        }

        [Test]
        public void PrincessUndineTest()
        {
            TestGameflow.createGame(out var game, out var you, out var oppo,
                new KeyValuePair<int, int>(PrincessUndine.ID, 1),
                new KeyValuePair<int, int>(DefaultServant.ID, 4),
                new KeyValuePair<int, int>(SummerFire.ID, 3)
            );
            game.skipTurnUntil(() => game.getCurrentPlayer() == you && you.getGem() >= 8);
            you.cmdUse(game, you.getHand().getCard<DefaultServant>());
            you.cmdUse(game, you.getHand().getCard<PrincessUndine>());
            var buff = you.getMaster().getBuffs()[0];
            Assert.AreEqual(PrincessUndine.ID, buff.id);
            you.cmdUse(game, you.getHand().getCard<SummerFire>(), targets: you.getField()[0]);
            Assert.True(you.getField()[0].getCurrentLife(game) == 7);
            you.getField()[0].damage(game, you.getMaster(), 4);
            Assert.True(you.getField()[0].getCurrentLife(game) == 6);
            you.cmdTurnEnd(game);
            Assert.AreEqual(1, you.getMaster().getBuffs().Length);
            you.getMaster().damage(game, null, 4);
            Assert.AreEqual(1, game.triggers.getRecordedEvents().OfType<THHCard.DamageEventArg>().Last().infoDic[you.getMaster()].damagedValue);
            oppo.cmdTurnEnd(game);
            Assert.AreEqual(0, you.getMaster().getBuffs().Length);

            game.Dispose();
        }

        [Test]
        public void SylphyHornTest()
        {
            TestGameflow.createGame(out var game, out var you, out var oppo,
                new KeyValuePair<int, int>(SylphyHorn.ID, 1),
                new KeyValuePair<int, int>(DefaultServant.ID, 3)
            );
            game.skipTurnUntil(() => game.getCurrentPlayer() == you && you.getGem() >= game.getDefine<SylphyHorn>().cost + 1);
            you.cmdUse(game, you.getHand().getCard<DefaultServant>());
            you.cmdUse(game, you.getHand().getCard<SylphyHorn>(), targets: you.getMaster());
            //Card.SetPropEventArg card = game.triggers.getRecordedEvents().OfType<Card.SetPropEventArg>().LastOrDefault(c =>c.card == you.master&&c.propName == PROPERTY.CURRENT_LIFE);
            //game.logger.log("a:" + card.value);
            game.Dispose();
        }

        [Test]
        public void TrilithonShakeTest()
        {
            TestGameflow.createGame(out var game, out var you, out _, new KeyValuePair<int, int>(TrilithonShake.ID, 1));
            game.skipUntilCanUse<TrilithonShake>(you);
            you.cmdUse(game, you.getHand().getCard<TrilithonShake>());
            you.cmdTurnEnd(game);
            game.skipTurnUntil(() => game.getCurrentPlayer() == you);
            Assert.False(you.getField()[0].canAttack(game));
            Assert.True(you.getField()[0].getKeywords(game).Contains(KEYWORD.CANTATTACK));
            you.getField()[0].silence(game);
            Assert.True(you.getField()[0].canAttack(game));

            game.Dispose();
        }

        [Test]
        public void MetalFatigueTest()
        {
            CardEngine game = TestGameflow.initGameWithoutPlayers(null, new HearthstoneOption()
            {
                shuffle = false
            });
            Player defaultPlayer = game.createPlayer(0, "玩家0", game.getDefine<TestMaster2>(), Enumerable.Repeat(game.getDefine<DefaultServant>() as CardDefine, 30));
            Player elusivePlayer = game.createPlayer(1, "玩家1", game.getDefine<TestMaster2>(), Enumerable.Repeat(game.getDefine<DefaultServant>() as CardDefine, 28)
            .Concat(Enumerable.Repeat(game.getDefine<MetalFatigue>(), 2)));

            game.skipTurnUntil(() => defaultPlayer.getGem() >= game.getDefine<DefaultServant>().cost && game.getCurrentPlayer() == defaultPlayer);
            defaultPlayer.cmdUse(game, defaultPlayer.getHand().First(c => c.define.id == DefaultServant.ID), 0);

            elusivePlayer.cmdTurnEnd(game);

            game.skipTurnUntil(() => defaultPlayer.getGem() >= game.getDefine<DefaultServant>().cost && game.getCurrentPlayer() == defaultPlayer);
            defaultPlayer.cmdUse(game, defaultPlayer.getHand().First(c => c.define.id == DefaultServant.ID), 0);

            game.skipTurnUntil(() => elusivePlayer.getGem() >= game.getDefine<MetalFatigue>().cost && game.getCurrentPlayer() == elusivePlayer);
            elusivePlayer.cmdUse(game, elusivePlayer.getHand().First(c => c.define.id == MetalFatigue.ID), 0);

            game.Dispose();
        }

        [Test]
        public void KoakumaTest()
        {
            TestGameflow.createGame(out var game, out var you, out var oppo,
                new KeyValuePair<int, int>(Koakuma.ID, 1),
                new KeyValuePair<int, int>(FireBall.ID, 3),
                new KeyValuePair<int, int>(DefaultServant.ID, 3)
            );//游戏初始化写的这么复杂干嘛
            for (int i = 0; i < 3; i++)
            {
                game.skipTurnUntil(() => game.getCurrentPlayer() == you && you.getGem() >= game.getDefine<FireBall>().cost);
                you.cmdUse(game, you.getHand().getCard<FireBall>(), targets: oppo.getMaster());
                you.cmdTurnEnd(game);
            }//先射三发火球
            game.skipTurnUntil(() => game.getCurrentPlayer() == you && you.getGem() >= game.getDefine<Koakuma>().cost);
            you.cmdUse(game, you.getHand().getCard<Koakuma>(), targets: oppo.getMaster());//使用小恶魔
            DiscoverRequest discoverRequest = game.answers.getRequest<DiscoverRequest>(you.id);
            Assert.NotNull(discoverRequest);//确实发现
            Assert.True(discoverRequest.cardIdArray.All(id => you.getGrave().Any(c => c.id == id)));//发现的全是墓地里的卡
            int cardId = discoverRequest.cardIdArray[0];
            Card card = you.getGrave().getCard(cardId);//随便选一张
            you.cmdDiscover(game, cardId);//选
            Assert.True(you.getHand().Contains(card));//确认它在你的手牌里了
            //TODO:落叶，补充各种特殊情况，比如墓地里没有卡，手牌满了，墓地里还有随从
            //game.skipTurnUntil(() => game.currentPlayer == you && you.gem >= game.getCardDefine<Koakuma>().cost);
            //you.cmdUse(game, you.hand.getCard<Koakuma>(), targets: oppo.master);//不射了，直接使用小恶魔
            //you.cmdUse(game, you.hand.getCard<DefaultServant>());
            //you.field[0].die(game);
            //you.cmdUse(game, you.hand.getCard<Koakuma>(), targets: oppo.master);
            game.Dispose();
        }

        [Test]
        public void RoyalFlareTest()
        {
            TestGameflow.createGame(out var game, out var you, out var oppo,
            new KeyValuePair<int, int>(RoyalFlare.ID, 3)
            );
            game.skipTurnUntil(() => game.getCurrentPlayer() == you && you.getGem() >= game.getDefine<AgniShine>().cost);
            you.cmdUse(game, you.getHand().getCard<RoyalFlare>());
            game.Dispose();
        }

        [Test]
        public void ElementSpriteTest()
        {
            TestGameflow.createGame(out var game, out var you, out var oppo,
            new KeyValuePair<int, int>(ElementSprite.ID, 3)
            );
            game.skipTurnUntil(() => game.getCurrentPlayer() == you && you.getGem() >= game.getDefine<ElementSprite>().cost);
            you.cmdUse(game, you.getHand().getCard<ElementSprite>());

            game.Dispose();
        }

        [Test]
        public void TheGreatLibraryTest()
        {
            TestGameflow.createGame(out var game, out var you, out var oppo,
                new KeyValuePair<int, int>(TheGreatLibrary.ID, 1),
                new KeyValuePair<int, int>(FireBall.ID, 20)
            );
            game.skipTurnUntil(() => game.getCurrentPlayer() == you && you.getGem() >= game.getDefine<TheGreatLibrary>().cost);
            you.cmdUse(game, you.getHand().getCard<TheGreatLibrary>());
            DiscoverRequest discoverRequest = game.answers.getRequest<DiscoverRequest>(you.id);
            Assert.NotNull(discoverRequest);
            Assert.True(discoverRequest.cardIdArray.All(id => you.getDeck().Any(c => c.id == id)));
            int cardId = discoverRequest.cardIdArray[0];
            Card card = you.getDeck().getCard(cardId);
            you.cmdDiscover(game, cardId);
            Assert.True(you.getHand().Contains(card));
            game.Dispose();
        }

        [Test]
        public void LibraryProtectorTest()
        {
            TestGameflow.createGame(out var game, out var you, out var oppo,
                new KeyValuePair<int, int>(LibraryProtector.ID, 1),
                new KeyValuePair<int, int>(FireBall.ID, 20)
            );
            game.skipTurnUntil(() => game.getCurrentPlayer() == you && you.getGem() >= game.getDefine<LibraryProtector>().cost);
            you.cmdUse(game, you.getHand().getCard<LibraryProtector>());
            game.Dispose();
        }

        [Test]
        public void ArcaneKnowledgeTest()
        {
            TestGameflow.createGame(out var game, out var you, out var oppo,
                new KeyValuePair<int, int>(ArcaneKnowledge.ID, 1),
                new KeyValuePair<int, int>(FireBall.ID, 20)
            );
            game.skipTurnUntil(() => game.getCurrentPlayer() == you && you.getGem() >= game.getDefine<ArcaneKnowledge>().cost);
            you.cmdUse(game, you.getHand().getCard<ArcaneKnowledge>());
            game.Dispose();
        }

        [Test]
        public void SilentSeleneTest()
        {
            CardEngine game = TestGameflow.initGameWithoutPlayers(null, new HearthstoneOption() { shuffle = false });
            Player defaultPlayer = game.createPlayer(0, "玩家0", game.getDefine<TestMaster2>(), Enumerable.Repeat(game.getDefine<MissingSpecter>() as CardDefine, 30));
            Player elusivePlayer = game.createPlayer(1, "玩家1", game.getDefine<TestMaster2>(), Enumerable.Repeat(game.getDefine<SilentSelene>() as CardDefine, 28)
            .Concat(Enumerable.Repeat(game.getDefine<BestMagic>(), 2)));

            game.skipTurnUntil(() => game.getCurrentPlayer() == defaultPlayer && defaultPlayer.getGem() >= game.getDefine<MissingSpecter>().cost);
            defaultPlayer.cmdUse(game, defaultPlayer.getHand().getCard<MissingSpecter>());

            game.skipTurnUntil(() => game.getCurrentPlayer() == elusivePlayer && elusivePlayer.getGem() >= 10);
            elusivePlayer.cmdUse(game, elusivePlayer.getHand().getCard<BestMagic>());//使用秘藏魔法减费
            Assert.True(game.players[0].getHand().Where(c => c.define is SilentSelene).All(c => c.getCost(game) == 7));
            elusivePlayer.cmdUse(game, elusivePlayer.getHand().getCard<SilentSelene>());

            game.Dispose();
        }

        [Test]
        public void SummerRedTest()
        {
            TestGameflow.createGame(out var game, out var you, out var oppo,
                new KeyValuePair<int, int>(SummerRed.ID, 1),
                new KeyValuePair<int, int>(DefaultServant.ID, 20)
            );
            game.skipTurnUntil(() => game.getCurrentPlayer() == you && you.getGem() >= game.getDefine<DefaultServant>().cost);
            you.cmdUse(game, you.getHand().getCard<DefaultServant>());
            you.cmdUse(game, you.getHand().getCard<SummerRed>(), targets: you.getField()[0]);
            you.cmdTurnEnd(game);
            game.Dispose();
        }

        [Test]
        public void WinterSoberTest()
        {
            TestGameflow.createGame(out var game, out var you, out var oppo,
                new KeyValuePair<int, int>(WinterElement.ID, 1),
                new KeyValuePair<int, int>(DefaultServant.ID, 20)
            );
            game.skipTurnUntil(() => game.getCurrentPlayer() == you && you.getGem() >= game.getDefine<DefaultServant>().cost + 1);
            you.cmdUse(game, you.getHand().getCard<DefaultServant>());
            you.cmdTurnEnd(game);
            oppo.cmdTurnEnd(game);
            you.cmdUse(game, you.getHand().getCard<WinterElement>(), targets: you.getField()[0]);
            you.cmdAttack(game, you.getField()[0], oppo.getMaster());
            game.Dispose();
        }

        [Test]
        public void SpringWindTest()
        {
            TestGameflow.createGame(out var game, out var you, out var oppo,
                new KeyValuePair<int, int>(SpringWind.ID, 1),
                new KeyValuePair<int, int>(DefaultServant.ID, 20)
            );
            game.skipTurnUntil(() => game.getCurrentPlayer() == you && you.getGem() >= game.getDefine<SummerRed>().cost + 1);
            you.cmdUse(game, you.getHand().getCard<DefaultServant>());
            you.cmdUse(game, you.getHand().getCard<SpringWind>(), targets: you.getField()[0]);
            you.cmdTurnEnd(game);
            game.Dispose();
        }

        [Test]
        public void AutumnBladeTest()
        {
            TestGameflow.createGame(out var game, out var you, out var oppo,
                new KeyValuePair<int, int>(AutumnEdge.ID, 1),
                new KeyValuePair<int, int>(DefaultServant.ID, 20)
            );
            game.skipTurnUntil(() => game.getCurrentPlayer() == oppo && oppo.getGem() >= 3);
            for (int i = 0; i < 3; i++)
            {
                oppo.cmdUse(game, oppo.getHand().getCard<DefaultServant>(), i);
            }
            oppo.cmdTurnEnd(game);
            you.cmdUse(game, you.getHand().getCard<AutumnEdge>());
            game.Dispose();
        }

        [Test]
        public void EarthGunTest()
        {
            TestGameflow.createGame(out var game, out var you, out var oppo,
                new KeyValuePair<int, int>(DoyouSpear.ID, 1),
                new KeyValuePair<int, int>(DefaultServant.ID, 20)
            );
            game.skipTurnUntil(() => game.getCurrentPlayer() == you && you.getGem() >= game.getDefine<SummerRed>().cost + 1);
            you.cmdUse(game, you.getHand().getCard<DoyouSpear>());
            game.Dispose();
        }

        [Test]
        public void ElementHarvesterTest()
        {
            TestGameflow.createGame(out var game, out var you, out var oppo,
                new KeyValuePair<int, int>(ElementalHarvester.ID, 1)
            );
            game.skipTurnUntil(() => game.getCurrentPlayer() == you && you.getGem() >= game.getDefine<DefaultServant>().cost * 3);
            for (int i = 0; i < 3; i++)
            {
                you.cmdUse(game, you.getHand().getCard<DefaultServant>());
            }
            you.cmdTurnEnd(game);
            for (int i = 0; i < 3; i++)
            {
                oppo.cmdUse(game, oppo.getHand().getCard<DefaultServant>());
                oppo.getField()[i].setCurrentLife(game, 2);
            }
            oppo.cmdTurnEnd(game);
            game.skipTurnUntil(() => game.getCurrentPlayer() == you && you.getGem() >= game.getDefine<ElementalHarvester>().cost);
            you.cmdUse(game, you.getHand().getCard<ElementalHarvester>());
            game.Dispose();
        }

        [Test]
        public void MercuryPoisonTest()
        {
            TestGameflow.createGame(out var game, out var you, out var oppo,
                new KeyValuePair<int, int>(MercuryPoison.ID, 1),
                new KeyValuePair<int, int>(DefaultServant.ID, 20)
            );
            game.skipTurnUntil(() => game.getCurrentPlayer() == oppo && oppo.getGem() >= 3);
            for (int i = 0; i < 3; i++)
            {
                oppo.cmdUse(game, oppo.getHand().getCard<DefaultServant>(), i);
            }
            oppo.getField()[0].setAttack(game, 5);
            oppo.cmdTurnEnd(game);
            game.skipTurnUntil(() => game.getCurrentPlayer() == you && you.getGem() >= 6);
            you.cmdUse(game, you.getHand().getCard<MercuryPoison>());
            you.cmdTurnEnd(game);
            oppo.cmdTurnEnd(game);
            game.Dispose();
        }

        [Test]
        public void ForestFireTest()
        {
            TestGameflow.createGame(out var game, out var you, out var oppo,
                new KeyValuePair<int, int>(ForestBlaze.ID, 1),
                new KeyValuePair<int, int>(DefaultServant.ID, 20)
            );
            /*game.skipTurnUntil(() => game.currentPlayer == oppo && oppo.gem >= 3);
            for (int i = 0; i < 3; i++)
            {
                oppo.cmdUse(game, oppo.hand.getCard<DefaultServant>(), i);
            }
            oppo.cmdTurnEnd(game);*/
            game.skipTurnUntil(() => game.getCurrentPlayer() == you && you.getGem() >= 9);
            for (int i = 0; i < 3; i++)
            {
                you.cmdUse(game, you.getHand().getCard<DefaultServant>(), i);
            }
            you.cmdUse(game, you.getHand().getCard<ForestBlaze>(), targets: oppo.getMaster());
            game.Dispose();
        }

        [Test]
        public void BurgeoningRiseTest()
        {
            TestGameflow.createGame(out var game, out var you, out var oppo,
                new KeyValuePair<int, int>(BurgeoningRise.ID, 1)
            );
            game.skipUntilCanUse<DefaultServant>(you);
            you.cmdUse(game, you.getHand().getCard<DefaultServant>());
            game.skipUntilCanUse<BurgeoningRise>(you);
            you.cmdUse(game, you.getHand().getCard<BurgeoningRise>(), targets: you.getField()[0]);
            you.getField()[0].die(game);
            Assert.AreEqual(1, you.getField().count);
            var dead = you.getField()[0];
            Assert.IsInstanceOf<DefaultServant>(dead.define);
            Buff buff = dead.getBuffs()[0];
            Assert.NotNull(buff);
            Assert.AreEqual(game.getDefine<DefaultServant>().attack + 3, dead.getAttack(game));
            Assert.AreEqual(game.getDefine<DefaultServant>().life + 3, dead.getLife(game));

            game.Dispose();
        }

        [Test]
        public void WaterElfTest()
        {
            TestGameflow.createGame(out var game, out var you, out var oppo,
                new KeyValuePair<int, int>(WaterElf.ID, 1),
                new KeyValuePair<int, int>(DefaultServant.ID, 20)
            );
            game.skipTurnUntil(() => game.getCurrentPlayer() == you && you.getGem() >= 7);
            you.cmdUse(game, you.getHand().getCard<DefaultServant>());
            you.getField()[0].damage(game, you.getMaster(), 4);
            you.cmdUse(game, you.getHand().getCard<WaterElf>(), targets: you.getField()[0]);
            Assert.True(you.getField()[0].getCurrentLife(game) == 13);
            Assert.True(you.getField()[0].hasKeyword(game, KEYWORD.TAUNT));
            var keywords = you.getField()[0].getProp(game, DEFINE.KEYWORDS);
            Debug.Log(keywords.GetType());
            Assert.True(keywords is Array);
            Assert.True(keywords is IEnumerable);
            game.Dispose();
        }

        [Test]
        public void PhlogisticRainTest()
        {
            TestGameflow.createGame(out var game, out var you, out var oppo,
                new KeyValuePair<int, int>(PhlogisticRain.ID, 1)
            );
            game.skipTurnUntil(() => game.getCurrentPlayer() == oppo && oppo.getGem() >= 3);
            for (int i = 0; i < 3; i++)
            {
                oppo.cmdUse(game, oppo.getHand().getCard<DefaultServant>(), i);
            }
            oppo.cmdTurnEnd(game);
            game.skipUntilCanUse<PhlogisticRain>(you);
            int totalLife = oppo.getMaster().getCurrentLife(game) + oppo.getField().Sum(s => s.getCurrentLife(game));
            you.cmdUse(game, you.getHand().getCard<PhlogisticRain>());
            Assert.AreEqual(totalLife - 6, oppo.getMaster().getCurrentLife(game) + oppo.getField().Sum(s => s.getCurrentLife(game)));
            game.Dispose();
        }

        [Test]
        public void SanElmoFirePillarTest()
        {
            TestGameflow.createGame(out var game, out var you, out var oppo,
                new KeyValuePair<int, int>(StElmoPillar.ID, 1),
                new KeyValuePair<int, int>(DefaultServant.ID, 20)
            );
            game.skipTurnUntil(() => game.getCurrentPlayer() == oppo && oppo.getGem() >= 3);
            for (int i = 0; i < 3; i++)
            {
                oppo.cmdUse(game, oppo.getHand().getCard<DefaultServant>(), i);
            }
            oppo.cmdTurnEnd(game);

            you.cmdUse(game, you.getHand().getCard<StElmoPillar>(), targets: oppo.getField()[1]);
            game.Dispose();
        }

        [Test]
        public void RingLavaTest()
        {
            TestGameflow.createGame(out var game, out var you, out var oppo,
                new KeyValuePair<int, int>(LavaCromlech.ID, 1),
                new KeyValuePair<int, int>(DefaultServant.ID, 20)
            );
            game.skipTurnUntil(() => game.getCurrentPlayer() == oppo && oppo.getGem() >= 3);
            for (int i = 0; i < 3; i++)
            {
                oppo.cmdUse(game, oppo.getHand().getCard<DefaultServant>(), i);
            }
            oppo.cmdTurnEnd(game);

            you.cmdUse(game, you.getHand().getCard<LavaCromlech>());
            you.cmdTurnEnd(game);
            oppo.cmdTurnEnd(game);
            you.cmdTurnEnd(game);
            game.Dispose();
        }

        [Test]
        public void LightyellowgustTest()
        {
            TestGameflow.createGame(out var game, out var you, out var oppo,
                new KeyValuePair<int, int>(GingerGust.ID, 1),
                new KeyValuePair<int, int>(DefaultServant.ID, 20)
            );
            game.skipTurnUntil(() => game.getCurrentPlayer() == oppo && oppo.getGem() >= 3);
            for (int i = 0; i < 3; i++)
            {
                oppo.cmdUse(game, oppo.getHand().getCard<DefaultServant>());
            }
            oppo.cmdTurnEnd(game);
            game.skipUntilCanUse<GingerGust>(you);
            you.cmdUse(game, you.getHand().getCard<GingerGust>());
            you.cmdAttack(game, you.getField()[0], oppo.getField()[1]);
            you.cmdTurnEnd(game);
            game.Dispose();
        }

        [Test]
        public void NoahFloodTest()
        {
            TestGameflow.createGame(out var game, out var you, out var oppo,
                new KeyValuePair<int, int>(NoachianDeluge.ID, 1),
                new KeyValuePair<int, int>(MissingSpecter.ID, 20)
            );
            game.skipTurnUntil(() => game.getCurrentPlayer() == you && you.getGem() >= 9);
            you.cmdUse(game, you.getHand().getCard<MissingSpecter>());
            you.cmdUse(game, you.getHand().getCard<NoachianDeluge>());
            game.Dispose();
        }

        [Test]
        public void PhilosopherStoneTest()
        {
            TestGameflow.createGame(out var game, out var you, out var oppo,
                new KeyValuePair<int, int>(PhilosopherStone.ID, 5)
            );

            // game.skipTurnUntil(() => game.currentPlayer == you && you.gem >= game.getCardDefine<PhilosopherStone>().cost);
            // var handBeforeUse = you.hand.ToArray();
            // you.cmdUse(game, you.hand.getCard<PhilosopherStone>());//挂刀
            // Assert.IsInstanceOf<PhilosopherStone>(you.item.define);//挂上了
            // var getCard = you.hand.First(c => !handBeforeUse.Contains(c));//给的牌
            // game.skipUntilCanUse(you, getCard);
            // handBeforeUse = you.hand.ToArray();
            // you.cmdUse(game, getCard);//用牌
            // getCard = you.hand.First(c => !handBeforeUse.Contains(c));//给的牌
            //TODO:现在是之前的版本，用一张牌给一张牌，但是是回合结束给牌，所以需要修改测试，让它通过。
            game.skipUntilCanUse<PhilosopherStone>(you);
            var handBeforeUse = you.getHand().ToArray().Length;

            you.cmdUse(game, you.getHand().getCard<PhilosopherStone>());
            Assert.IsInstanceOf<PhilosopherStone>(you.getItem().define);

            Debug.Log("手牌数量:" + handBeforeUse);
            you.cmdTurnEnd(game);
            handBeforeUse = you.getHand().ToArray().Length;
            Debug.Log("手牌数量:" + handBeforeUse);

            game.Dispose();
        }

        [Test]
        public void BiteTentacleTest()
        {
            TestGameflow.createGame(out var game, out var you, out var oppo,
                new KeyValuePair<int, int>(BiteTentacle.ID, 2),
                new KeyValuePair<int, int>(GingerGust.ID, 6)
            );
            game.skipTurnUntil(() => game.getCurrentPlayer() == you && you.getGem() >= game.getDefine<BiteTentacle>().cost);
            you.cmdUse(game, you.getHand().getCard<BiteTentacle>());
            var handBeforeUse = you.getHand().ToArray();
            game.skipTurnUntil(() => game.getCurrentPlayer() == you && you.getGem() >= 5);
            Pile.MoveCardEventArg movecardevent = game.triggers.getRecordedEvents().OfType<Pile.MoveCardEventArg>().LastOrDefault(c => c.to == you.getGrave());
            Card throwcard = movecardevent.card;
            Assert.True(handBeforeUse.Contains(throwcard));

            game.Dispose();
        }

        [Test]
        public void MultiCastTest()
        {
            TestGameflow.createGame(out var game, out var you, out var oppo,
                new KeyValuePair<int, int>(MultiCast.ID, 2),
                new KeyValuePair<int, int>(GingerGust.ID, 6)
            );
            game.skipTurnUntil(() => game.getCurrentPlayer() == you && you.getGem() >= game.getDefine<MultiCast>().cost);
            you.cmdUse(game, you.getHand().getCard<MultiCast>());
            game.Dispose();
        }

        [Test]
        public void AsthmaTest()
        {
            TestGameflow.createGame(out var game, out var you, out var oppo,
                new KeyValuePair<int, int>(MultiCast.ID, 1)
            );
            game.skipTurnUntil(() => game.getCurrentPlayer() == you && you.getGem() >= 2);
            for (int i = 0; i < 2; i++)
            {
                you.cmdUse(game, you.getHand().getCard<MultiCast>());
                you.draw(game, you.getDeck().Where(c => c.define is Asthma).ToArray()[0]);
            }
            game.Dispose();
        }
        [Test]
        public void getMixCardsTest()
        {
            TestGameflow.createGame(out var game, out _, out _);
            CardDefine[] mixedCards = Patchouli.getMixCards(game, game.getDefine<PhlogisticRain>(), game.getDefine<BurgeoningRise>());
            Debug.Log(string.Join<CardDefine>("，", mixedCards));
            Assert.AreEqual(4, mixedCards.Length);
        }
        [Test]
        public void caseLibraryProtectorBuffedSilenceHPWontResumeTest()
        {
            TestGameflow.createGame(out var game, out var you, out var _,
                new KeyValuePair<int, int>(LibraryProtector.ID, 1),
                new KeyValuePair<int, int>(SylphyHorn.ID, 1),
                new KeyValuePair<int, int>(DoyouSpear.ID, 1),
                new KeyValuePair<int, int>(NoachianDeluge.ID, 1));
            game.skipUntilCanUse<LibraryProtector>(you);
            you.cmdUse(game, you.getHand().getCard<LibraryProtector>());
            int defineLife = game.getDefine<LibraryProtector>().life;
            int lifeWhenUsed = you.getField()[0].getCurrentLife(game);
            Assert.Greater(lifeWhenUsed, defineLife);
            game.skipUntilCanUse<SylphyHorn>(you);
            you.cmdUse(game, you.getHand().getCard<SylphyHorn>(), 0, you.getField()[0]);
            Assert.Greater(you.getField()[0].getCurrentLife(game), lifeWhenUsed);
            you.cmdDiscover(game, you.getHand().getCard<DoyouSpear>().id);
            you.cmdDiscover(game, game.answers.getRequest<DiscoverRequest>(you.id).cardIdArray[0]);
            game.skipUntilCanUse<NoachianDeluge>(you);
            you.cmdUse(game, you.getHand().getCard<NoachianDeluge>());
            Assert.False(you.getField()[0].hasTag(game, KEYWORD.TAUNT));
            Assert.AreEqual(defineLife, you.getField()[0].getCurrentLife(game));
            Assert.AreEqual(defineLife, you.getField()[0].getLife(game));

            game.Dispose();
        }

        [Test]
        public void Double_BurgeoningRiseTest()
        {
            TestGameflow.createGame(out var game, out var you, out var oppo,
                new KeyValuePair<int, int>(BurgeoningRise.ID, 2)
            );
            game.skipUntilCanUse<DefaultServant>(you);
            you.cmdUse(game, you.getHand().getCard<DefaultServant>());
            game.skipUntilCanUse<BurgeoningRise>(you);
            you.cmdUse(game, you.getHand().getCard<BurgeoningRise>(), targets: you.getField()[0]);
            game.skipUntilCanUse<BurgeoningRise>(you);
            you.cmdUse(game, you.getHand().getCard<BurgeoningRise>(), targets: you.getField()[0]);
            you.getField()[0].die(game);
            Assert.AreEqual(2, you.getField().count);
            var dead = you.getField()[0];
            Assert.IsInstanceOf<DefaultServant>(dead.define);
            Buff buff = dead.getBuffs()[0];
            Assert.NotNull(buff);
            Assert.AreEqual(game.getDefine<DefaultServant>().attack + 3, dead.getAttack(game));
            Assert.AreEqual(game.getDefine<DefaultServant>().life + 3, dead.getLife(game));

            game.Dispose();
        }
    }
}

