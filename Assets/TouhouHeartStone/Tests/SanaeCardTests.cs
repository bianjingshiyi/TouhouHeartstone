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
using System.Threading.Tasks;
using UI;
using Card = TouhouCardEngine.Card;

namespace Tests
{
    public class SanaeCardTests
    {
        [Test]
        public void sanaeSkillTest()
        {
            THHGame game = TestGameflow.initStandardGame(null, new int[] { 0, 1 },
                Enumerable.Repeat(new Sanae(), 2).ToArray(),
                Enumerable.Repeat(Enumerable.Repeat(new RashFairy(), 30).ToArray(), 2).ToArray(),
                new GameOption() { });
            game.run();
            game.sortedPlayers[0].cmdInitReplace(game);
            game.sortedPlayers[1].cmdInitReplace(game);

            game.sortedPlayers[0].cmdUse(game, game.sortedPlayers[0].hand[0], 0);
            game.sortedPlayers[0].cmdAttack(game, game.sortedPlayers[0].field[0], game.sortedPlayers[1].master);
            game.sortedPlayers[0].cmdTurnEnd(game);
            game.sortedPlayers[1].cmdTurnEnd(game);
            game.sortedPlayers[0].cmdTurnEnd(game);
            game.sortedPlayers[1].cmdUse(game, game.sortedPlayers[1].skill, 0, game.sortedPlayers[1].master);

            THHCard.HealEventArg heal =
                game.triggers.getRecordedEvents().LastOrDefault(e => e is THHCard.HealEventArg) as THHCard.HealEventArg;
            Assert.AreEqual(1, heal.cards.Length);
            Assert.AreEqual(2, heal.value);
            Assert.AreEqual(1, heal.infoDic[game.sortedPlayers[1].master].healedValue);
        }

        //拼点测试
        [Test]
        public void RevealDuelTest()
        {
            THHGame game = TestGameflow.initGameWithoutPlayers(null, new GameOption()
            {
                shuffle = false
            });
            THHPlayer oppo = game.createPlayer(0, "玩家0", game.getDefine<Sanae>(),
                Enumerable.Repeat(game.getDefine<TestFreezeBrain>() as CardDefine, 30));
            THHPlayer you = game.createPlayer(1, "玩家1", game.getDefine<Sanae>(),
                Enumerable.Repeat(game.getDefine<DefaultServant>() as CardDefine, 30));
            var reveal = Sanae.revealDuel(you, game);
            Assert.True(reveal.IsCompleted);

            game.Dispose();
        }

        /// <summary>
        /// 星之仪式测试
        /// 信仰对决 将一张全新的“星之仪式”置入手牌；获胜时改为抽一张法力值消耗最高的牌
        /// </summary>
        [Test]
        public void StarRitualTest()
        {
            TestGameflow.createGame<FireBall, DefaultServant>(out var game, out var you, out var oppo,
                new GameOption() { shuffle = false },
                Sanae.ID, new[] { new KeyValuePair<int, int>(StarRitual.ID, 2) },
                Sanae.ID, new KeyValuePair<int, int>[0]);
            game.skipUntilCanUse<StarRitual>(you);
            int count = you.hand.count;
            you.cmdUse<StarRitual>(game);
            Assert.AreEqual(count, you.hand.count);
            Assert.AreEqual(4, you.hand[count - 1].getCost(game));
            game.skipTurnUntil(() => you.deck.count == 0);
            you.cmdUse<StarRitual>(game);
            count = you.hand.count;
            Assert.AreEqual(1, you.hand[count - 1].getCost(game));
            game.Dispose();
        }

        [Test]
        public void GrayThaumaturgyTest()
        {
            TestGameflow.createGame(out THHGame game, out THHPlayer you, out THHPlayer oppo,
                Sanae.ID,
                new KeyValuePair<int, int>(GrayThaumaturgy.ID, 2), new KeyValuePair<int, int>(StarRitual.ID, 4));
            game.skipUntilHasGem(you, 5);
            you.master.damage(game, null, 20);
            Assert.AreEqual(10, you.master.getCurrentLife(game));
            you.cmdUse(game, you.hand.getCard<GrayThaumaturgy>(), targets: you.master);
            Assert.AreEqual(18, you.master.getCurrentLife(game));
            game.turnEnd(you);
            game.skipUntilYourTurn(you);
            you.cmdUse<StarRitual>(game);
            Assert.AreEqual(18, you.master.getCurrentLife(game));
            you.cmdUse(game, you.hand.getCard<GrayThaumaturgy>(), targets: you.master);
            Assert.AreEqual(26, you.master.getCurrentLife(game));
            you.cmdUse<StarRitual>(game);
            Assert.AreEqual(30, you.master.getCurrentLife(game));
            you.master.damage(game, null, 20);
            you.cmdUse<StarRitual>(game);
            Assert.AreEqual(10, you.master.getCurrentLife(game));
            game.Dispose();
        }

        [Test]
        public void CharmOfGoodCommerceTest()
        {
            TestGameflow.createGame<FlyingSnake, DefaultServant>(out var game, out var you, out var oppo,
                new GameOption() { shuffle = false },
                Sanae.ID, new[] { new KeyValuePair<int, int>(CharmOfGoodCommerce.ID, 2) },
                Sanae.ID, new KeyValuePair<int, int>[0]);
            game.skipUntilCanUse<DefaultServant>(oppo);
            oppo.cmdUse(game, oppo.hand.getCard<DefaultServant>());

            game.skipUntilCanUse<CharmOfGoodCommerce>(you);
            you.master.damage(game, null, 5);
            int targetLife = oppo.field[0].getCurrentLife(game);
            int masterLife = you.master.getCurrentLife(game);
            you.cmdUse(game, you.hand.getCard<CharmOfGoodCommerce>(), targets: oppo.field[0]);
            Assert.AreEqual(targetLife - 3, oppo.field[0].getCurrentLife(game));
            Assert.AreEqual(masterLife + 4, you.master.getCurrentLife(game));
            game.Dispose();
        }

        [Test]
        public void BumperCropTest()
        {
            TestGameflow.createGame(out THHGame game, out THHPlayer you, out THHPlayer oppo,
                Sanae.ID,
                new KeyValuePair<int, int>(BumperCrop.ID, 1));
            for (int i = 0; i < 3; i++)
            {
                game.skipUntilCanUse<DefaultServant>(you);
                you.cmdUse(game, you.hand.getCard<DefaultServant>());
                you.field[0].damage(game, null, 1);
            }

            you.master.damage(game, null, 2);
            game.skipUntilCanUse<BumperCrop>(you);
            you.cmdUse(game, you.hand.getCard<BumperCrop>());
            for (int i = 0; i < 3; i++)
            {
                Assert.AreEqual(you.field[i].getLife(game), you.field[i].getCurrentLife(game));
            }

            Assert.AreEqual(30, you.master.getCurrentLife(game));
            game.Dispose();
        }

        [Test]
        public void GuestStarsMiracleTest()
        {
            TestGameflow.createGame<FlyingSnake, DefaultServant>(out var game, out var you, out var oppo,
                new GameOption() { shuffle = false },
                Sanae.ID, new[] { new KeyValuePair<int, int>(GuestStarsMiracle.ID, 1) },
                Sanae.ID, new KeyValuePair<int, int>[0]);
            for (int i = 0; i < 3; i++)
            {
                game.skipUntilCanUse<DefaultServant>(oppo);
                oppo.cmdUse(game, oppo.hand.getCard<DefaultServant>());
            }

            Pile oppoField = oppo.field;
            int defaultServantLife = oppoField[0].getCurrentLife(game);
            game.skipUntilCanUse<GuestStarsMiracle>(you);
            int count = you.hand.count;
            you.cmdUse<GuestStarsMiracle>(game, targets: oppoField[1]);
            Assert.AreEqual(defaultServantLife - 2, oppoField[0].getCurrentLife(game));
            Assert.AreEqual(defaultServantLife - 2, oppoField[1].getCurrentLife(game));
            Assert.AreEqual(defaultServantLife - 2, oppoField[2].getCurrentLife(game));
            Assert.AreEqual(count, you.hand.count);
            game.Dispose();
        }

        [Test]
        public void OpenSeaMiracleTest()
        {
            TestGameflow.createGame<FlyingSnake, DefaultServant>(out var game, out var you, out var oppo,
                new GameOption { shuffle = false },
                Sanae.ID, new[] { new KeyValuePair<int, int>(OpenSeaMiracle.ID, 2) },
                Sanae.ID, new KeyValuePair<int, int>[0]);
            for (int i = 0; i < 3; i++)
            {
                game.skipUntilCanUse<DefaultServant>(oppo);
                oppo.cmdUse(game, oppo.hand.getCard<DefaultServant>());
            }

            Pile oppoField = oppo.field;
            int defaultServantLife = oppoField[0].getCurrentLife(game);
            game.skipUntilCanUse<OpenSeaMiracle>(you);
            you.cmdUse<OpenSeaMiracle>(game, targets: oppoField[1]);
            Assert.AreEqual(defaultServantLife - 4, oppoField[0].getCurrentLife(game));
            Assert.AreEqual(defaultServantLife - 2, oppoField[1].getCurrentLife(game));
            Assert.AreEqual(defaultServantLife - 4, oppoField[2].getCurrentLife(game));
            game.skipTurnUntil(() => you.deck.count == 0);
            you.cmdUse<OpenSeaMiracle>(game, targets: oppo.field[1]);
            Assert.AreEqual(defaultServantLife - 2, oppoField[0].getCurrentLife(game));
            game.Dispose();
        }

        [Test]
        public void FlyingSnakeTest()
        {
            TestGameflow.createGame(out var game, out var you, out var oppo,
                Sanae.ID, new KeyValuePair<int, int>(FlyingSnake.ID, 1));
            game.skipUntilCanUse<DefaultServant>(oppo);
            oppo.cmdUse(game, oppo.hand.getCard<DefaultServant>());
            game.skipUntilCanUse<FlyingSnake>(you);
            Card snake = you.hand.getCard<FlyingSnake>();
            you.cmdUse(game, snake);
            Assert.True(snake.isAttackable(game, you, oppo.field[0], out _));
            you.cmdAttack(game, snake, oppo.field[0]);
            int attack = snake.getAttack(game);
            game.skipUntilHasGem(you, 2);
            you.cmdUse(game, you.skill, 0, snake);
            Assert.AreEqual(attack + 1, snake.getAttack(game));
            game.Dispose();
        }

        //[Test]
        //public void GodSummon_ThrustTest()
        //{
        //    TestGameflow.createGame(out var game, out var you, out var oppo,
        //        new KeyValuePair<int, int>(GodSummon_Thrust.ID, 2),
        //        new KeyValuePair<int, int>(YasakaKanako.ID, 2),
        //        new KeyValuePair<int, int>(DefaultServant.ID, 2)

        //    );

        //    game.skipUntilCanUse<GodSummon_Thrust>(you);
        //    you.cmdUse(game, you.hand.getCard<GodSummon_Thrust>());

        //    game.Dispose();

        //}

        [Test]
        public void GodSummonTest()
        {
            TestGameflow.createGame<YasakaKanako, DefaultServant>(out var game, out var you, out var oppo,
                new GameOption { shuffle = false },
                Sanae.ID, new[] { new KeyValuePair<int, int>(GodSummon.ID, 2) },
                Sanae.ID, new KeyValuePair<int, int>[0]);
            game.skipUntilCanUse<DefaultServant>(oppo);
            oppo.cmdUse<DefaultServant>(game, out Card enemy);
            game.skipUntilCanUse<GodSummon>(you);
            you.cmdUse<GodSummon>(game);
            Card kanako = you.field[0];
            Assert.IsInstanceOf<YasakaKanako>(kanako.define);
            Assert.True(kanako.canAttack(game));
            you.cmdTurnEnd(game);
            Assert.AreEqual(0, you.field.count);
            Assert.True(you.deck.Contains(kanako));
            game.skipUntilYourTurn(you);
            you.cmdUse<GodSummon>(game);
            kanako = you.field[0];
            Assert.True(kanako.canAttack(game));
            kanako.die(game);
            you.cmdTurnEnd(game);
            Assert.False(you.deck.Contains(kanako));
            Assert.AreEqual(0, you.field.count);
            game.Dispose();
        }

        [Test]
        public void ManipulatedGaintFrogTest()
        {
            THHGame game = TestGameflow.initGameWithoutPlayers(null, new GameOption()
            {
                shuffle = false
            });
            THHPlayer oppo = game.createPlayer(0, "玩家0", game.getDefine<Sanae>(),
                Enumerable.Repeat(game.getDefine<DefaultServant>() as CardDefine, 30));
            THHPlayer you = game.createPlayer(1, "玩家1", game.getDefine<Sanae>(),
                Enumerable.Repeat(game.getDefine<DefaultServant>() as CardDefine, 28)
                    .Concat(Enumerable.Repeat(game.getDefine<ManipulatedGaintFrog>(), 2)));

            for (int i = 0; i < 3; i = i + 1)
            {
                game.skipUntilCanUse<DefaultServant>(oppo);
                oppo.cmdUse<DefaultServant>(game, out Card servant);
                servant.setCurrentLife(game, 3);
            }

            for (int i = 0; i < 3; i = i + 1)
            {
                game.skipUntilCanUse<DefaultServant>(you);
                you.cmdUse<DefaultServant>(game, out Card servant);
                servant.setCurrentLife(game, 3);
            }

            game.skipUntilCanUse<ManipulatedGaintFrog>(you);
            Sanae.revealDuel(you, game);
            Sanae.revealDuel(you, game);
            you.cmdUse(game, you.hand.getCard<ManipulatedGaintFrog>());
            Sanae.revealDuel(you, game);

            you.field[0].setDead(game, true);
            game.updateDeath(game);
            Assert.AreEqual(0, you.field.count + oppo.field.count);
            game.Dispose();
        }

        [Test]
        public void OrochiOfAncientTimesTest()
        {
            TestGameflow.createGame<OrochiOfAncientTimes, DefaultServant>(out var game, out var you, out var oppo,
                new GameOption { shuffle = false },
                Sanae.ID, new[] { new KeyValuePair<int, int>(StarRitual.ID, 1) },
                Sanae.ID, new KeyValuePair<int, int>[0]);
            game.skipUntilCanUse<DefaultServant>(oppo);
            oppo.cmdUse<DefaultServant>(game, out Card enemServant);
            game.skipUntilCanUse<OrochiOfAncientTimes>(you);
            Assert.AreEqual(9, you.hand.getCard<OrochiOfAncientTimes>().getCost(game));
            you.cmdUse<StarRitual>(game);
            Assert.AreEqual(5, you.hand.getCard<OrochiOfAncientTimes>().getCost(game));
            Assert.True(you.hand.Last().define is OrochiOfAncientTimes);
            Assert.AreEqual(5, you.hand.Last().getCost(game));
            you.cmdTurnEnd(game);
            game.skipUntilYourTurn(you);
            Assert.AreEqual(9, you.hand.getCard<OrochiOfAncientTimes>().getCost(game));
            game.Dispose();
        }

        [Test]
        public void YasakaKanakoTest()
        {
            TestGameflow.createGame<StarRitual, DefaultServant>(out var game, out var you, out var oppo,
                new GameOption { shuffle = false },
                Sanae.ID, new[] { new KeyValuePair<int, int>(YasakaKanako.ID, 4) },
                Sanae.ID, new KeyValuePair<int, int>[0]);
            game.skipUntilHasGem(you, 10);
            for (int i = 0; i < 5; i++)
            {
                game.skipUntilCanUse<DefaultServant>(oppo);
                oppo.cmdUse<DefaultServant>(game);
            }
            game.skipUntilCanUse<StarRitual>(you);
            int revealCount = 3;
            for (int i = 0; i < revealCount; i++)
            {
                you.cmdUse<StarRitual>(game);
            }

            for (int i = 0; i < 4; i++)
            {
                game.skipUntilCanUse<YasakaKanako>(you);
                you.cmdUse<YasakaKanako>(game, targets: oppo.master);
                DiscoverRequest discoverRequest = game.answers.getRequest<DiscoverRequest>(you.id);
                Assert.NotNull(discoverRequest);
                int id = discoverRequest.cardIdArray[i];
                int handCount = you.hand.count;
                you.cmdDiscover(game, id);
                Card card = game.getCard(id);
                switch (card.define)
                {
                    case GodMiracleOfRain _:
                        foreach (var servant in oppo.field)
                        {
                            Assert.AreEqual(4 - revealCount, servant.getCurrentLife(game));
                        }

                        break;
                    case GodMiracleOfOnbashira _:
                        Assert.AreEqual((2 + revealCount <= 7) ? 2 + revealCount : 7, you.field.count);
                        break;
                    case GodMiracleOfWar _:
                        Assert.AreEqual(30 - 6 - revealCount, oppo.master.getCurrentLife(game));
                        break;
                    case GodMiracleOfRevolution _:
                        Assert.AreEqual(handCount + 2 + revealCount <= 10 ? 2 + handCount + 2 + revealCount : 10,
                            you.hand.count);
                        break;
                }

                you.field.setDead(game, true);
                game.updateDeath(game);
            }
        }

        [Test]
        public void MoriyaSuwakoTest()
        {
            TestGameflow.createGame<StarRitual, SorcererApprentice>(out var game, out var you, out var oppo,
                new GameOption { shuffle = false },
                Sanae.ID, new[] { new KeyValuePair<int, int>(MoriyaSuwako.ID, 4) },
                Sanae.ID, new KeyValuePair<int, int>[0]);
            game.skipUntilHasGem(you, 10);
            int cost = you.hand.getCard<MoriyaSuwako>().getCost(game);
            for (int i = 0; i < 5; i++)
            {
                you.cmdUse<StarRitual>(game);
            }

            int oppoFieldCount = 3;
            for (int i = 0; i < oppoFieldCount; i++)
            {
                game.skipUntilCanUse<SorcererApprentice>(oppo);
                oppo.cmdUse<SorcererApprentice>(game);
            }

            Assert.AreEqual(cost - 5, you.hand.getCard<MoriyaSuwako>().getCost(game));
            for (int i = 0; i < 4; i++)
            {
                game.skipUntilCanUse<MoriyaSuwako>(you);
                you.cmdUse<MoriyaSuwako>(game, out Card suwako);
                game.setNextRandomInt(i + 1);
                suwako.setDead(game, true);
                game.updateDeath(game);
                switch (i)
                {
                    case 0:
                        Assert.AreEqual(oppoFieldCount - 1, oppo.field.count);
                        break;
                    case 1:
                        Assert.True(you.field.All(c => c.define is Frog));
                        break;
                    case 2:
                        Assert.True(oppo.field.All(c => c.getAttack(game) == 1));
                        break;
                    default:
                        Assert.True(you.hand.Contains(suwako));
                        Assert.NotNull(suwako.getModifiers());
                        break;
                }
            }
        }
        [Test]
        public void suwakoReduceCostTest()
        {
            TestGameflow.createGame(out var game, out var you, out _,
                new KeyValuePair<int, int>(InvokeCard.ID, 1),
                new KeyValuePair<int, int>(DefaultServant.ID, 5),
                new KeyValuePair<int, int>(MoriyaSuwako.ID, 1));
            //打一张祈求牌
            game.skipUntilCanUse<InvokeCard>(you);
            you.cmdUse<InvokeCard>(game);
            //预期诹访子保持原费用
            Assert.AreEqual(you.deck.getCard<MoriyaSuwako>().define.getCost(), you.deck.getCard<MoriyaSuwako>().getCost(game));
            game.skipTurnUntil(() => you.hand.getCard<MoriyaSuwako>() != null);
            Card suwako = you.hand.getCard<MoriyaSuwako>();
            Assert.AreEqual(suwako.define.getCost() - 1, suwako.getCost(game));
        }
        [Test]
        public void kamikazeReduceCostTest()
        {
            TestGameflow.createGame(out var game, out var you, out _,
                new KeyValuePair<int, int>(InvokeCard.ID, 1),
                new KeyValuePair<int, int>(DefaultServant.ID, 5),
                new KeyValuePair<int, int>(GodWind.ID, 1));
            //打一张祈求牌
            game.skipUntilCanUse<InvokeCard>(you);
            //预期诹访子保持原费用
            Assert.AreEqual(you.deck.getCard<GodWind>().define.getCost(), you.deck.getCard<GodWind>().getCost(game));
            you.cmdUse<InvokeCard>(game);
            //预期诹访子保持原费用
            Assert.AreEqual(you.deck.getCard<GodWind>().define.getCost(), you.deck.getCard<GodWind>().getCost(game));
            game.skipTurnUntil(() => you.hand.getCard<GodWind>() != null);
            Card suwako = you.hand.getCard<GodWind>();
            Assert.AreEqual(suwako.define.getCost() - 1, suwako.getCost(game));
        }
        [Test]
        public void SuwakoBackToFullHandTest()
        {
            TestGameflow.createGame(out var game, out var you, out var oppo,
                new KeyValuePair<int, int>(MoriyaSuwako.ID, 1));
            game.skipUntilCanUse<MoriyaSuwako>(you);
            you.cmdUse<MoriyaSuwako>(game, out Card suwako);
            game.skipTurnUntil(() => you.hand.count == 10);
            game.setNextRandomInt(4);
            // 这里偷了个懒，得把suwako亡语设成回手,因为目前诹访子的写法不允许选亡语触发
            // ——不许偷懒
            suwako.setDead(game, true);
            game.updateDeath(game);
            //这检查结果也太几把偷懒了，起码要改成检查诹访子没有回手吧？
            //Assert.AreEqual(10,you.hand.count);
            Assert.False(you.hand.Any(c => c.define is MoriyaSuwako));
        }

        [Test]
        public void SuwakoEmptyOppoFieldTest()
        {
            TestGameflow.createGame(out var game, out var you, out var oppo,
                new KeyValuePair<int, int>(MoriyaSuwako.ID, 1));
            game.skipUntilCanUse<MoriyaSuwako>(you);
            you.cmdUse<MoriyaSuwako>(game, out Card suwako);
            game.setNextRandomInt(1);
            suwako.setDead(game, true);
            game.updateDeath(game);
        }

        [Test]
        public void GodWindTest()
        {
            TestGameflow.createGame(out THHGame game, out THHPlayer you, out THHPlayer oppo,
                new KeyValuePair<int, int>(InvokeCard.ID, 1),
                new KeyValuePair<int, int>(DefaultServant.ID, 5),
                new KeyValuePair<int, int>(GodWind.ID, 1));
            game.skipUntilCanUse<InvokeCard>(you);
            you.cmdUse<InvokeCard>(game);
            Assert.AreEqual(10, you.deck.getCard<GodWind>().getCost(game));
            for (int i = 0; i < 3; i++)
            {
                game.skipUntilCanUse<DefaultServant>(oppo);
                oppo.cmdUse<DefaultServant>(game);
            }

            for (int i = 0; i < 3; i++)
            {
                game.skipUntilCanUse<DefaultServant>(you);
                you.cmdUse<DefaultServant>(game);
            }

            List<Card> allServants = new List<Card>();
            allServants.AddRange(you.field.ToArray());
            allServants.AddRange(oppo.field.ToArray());
            game.skipUntilCanUse<GodWind>(you);
            Assert.AreEqual(9, you.hand.getCard<GodWind>().getCost(game));
            you.cmdUse<GodWind>(game);
            for (int i = 0; i < 6; i++)
            {
                Assert.True(oppo.deck.Contains(allServants[i]));
            }
        }

        [Test]
        public void LittleShrineTest()
        {
            TestGameflow.createGame<FireBall, DefaultServant>(out var game, out var you, out var oppo,
                new GameOption { shuffle = false },
                Sanae.ID, new[] { new KeyValuePair<int, int>(LittleShrine.ID, 1) },
                Sanae.ID, new KeyValuePair<int, int>[0]);
            game.skipUntilCanUse<LittleShrine>(you);
            int i = you.hand.count;
            you.cmdUse<LittleShrine>(game);
            you.cmdTurnEnd(game);
            Assert.AreEqual(i, you.hand.count);
        }

        [Test]
        public void ForgottenRecrptionTest()
        {
            TestGameflow.createGame(out THHGame game, out THHPlayer you, out THHPlayer oppo,
                new KeyValuePair<int, int>(ForgottenRecrption.ID, 1),
                new KeyValuePair<int, int>(MoriyaSuwako.ID, 1));
            game.skipUntilCanUse<MoriyaSuwako>(you);
            you.cmdUse<MoriyaSuwako>(game, out Card suwako);
            Assert.NotNull(suwako);
            game.setNextRandomInt(2);
            suwako.setDead(game, true);
            game.updateDeath(game);
            game.skipUntilCanUse<ForgottenRecrption>(you);
            you.cmdUse<ForgottenRecrption>(game);
            Assert.True(you.hand.Contains(suwako));
        }

        [Test]
        public void OmikujiBombTest()
        {
            throw new NotImplementedException();
        }
    }
}