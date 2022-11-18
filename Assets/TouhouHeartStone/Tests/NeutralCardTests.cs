using System;
using System.Collections;
using NUnit.Framework;
using UnityEngine.TestTools;
using System.Linq;
using TouhouHeartstone;
using TouhouCardEngine;
using TouhouHeartstone.Builtin;
using System.Collections.Generic;
using UnityEngine.XR;
using UnityEngine;

namespace Tests
{
    public class NeutralCardTests
    {
        [Test]
        public void rashFairyTest()
        {
            CardEngine game = TestGameflow.initStandardGame(null, new int[] {0, 1},
                Enumerable.Repeat(new Reimu(), 2).ToArray(),
                Enumerable.Repeat(Enumerable.Repeat(new RashFairy(), 30).ToArray(), 2).ToArray(),
                new HearthstoneOption() { });
            game.runGame();
            game.getSortedPlayers()[0].cmdInitReplace(game);
            game.getSortedPlayers()[1].cmdInitReplace(game);

            game.getSortedPlayers()[0].cmdUse(game, game.getSortedPlayers()[0].getHand()[0], 0);
            Assert.True(game.getSortedPlayers()[0].getField()[0].isCharge(game));
            Assert.True(game.getSortedPlayers()[0].getField()[0].canAttack(game));
        }

        [Test]
        public void humanVillageGuardTest()
        {
            CardEngine game = TestGameflow.initStandardGame(null, new int[] {0, 1},
                Enumerable.Repeat(new Reimu(), 2).ToArray(),
                Enumerable.Repeat(Enumerable.Repeat(new HumanVillageGuard(), 30).ToArray(), 2).ToArray(),
                new HearthstoneOption() { });
            game.runGame();
            game.getSortedPlayers()[0].cmdInitReplace(game);
            game.getSortedPlayers()[1].cmdInitReplace(game);

            game.getSortedPlayers()[0].cmdTurnEnd(game);
            game.getSortedPlayers()[1].cmdTurnEnd(game);
            game.getSortedPlayers()[0].cmdTurnEnd(game);
            game.getSortedPlayers()[1].cmdTurnEnd(game);

            game.getSortedPlayers()[0].cmdUse(game, game.getSortedPlayers()[0].getHand()[0], 0);
            Assert.True(game.getSortedPlayers()[0].getField()[0].isTaunt(game));
        }

        [Test]
        public void rifleHunterTest()
        {
            CardEngine game = TestGameflow.initStandardGame(null, new int[] {0, 1},
                Enumerable.Repeat(new Reimu(), 2).ToArray(),
                Enumerable.Repeat(Enumerable.Repeat(new RifleHunter(), 30).ToArray(), 2).ToArray(),
                new HearthstoneOption() { });
            game.runGame();
            game.getSortedPlayers()[0].cmdInitReplace(game);
            game.getSortedPlayers()[1].cmdInitReplace(game);

            game.getSortedPlayers()[0].cmdTurnEnd(game);
            game.getSortedPlayers()[1].cmdTurnEnd(game);
            game.getSortedPlayers()[0].cmdTurnEnd(game);
            game.getSortedPlayers()[1].cmdTurnEnd(game);

            game.getSortedPlayers()[0].cmdUse(game, game.getSortedPlayers()[0].getHand()[0], 0, game.getSortedPlayers()[1].getMaster());
            Assert.AreEqual(28, game.getSortedPlayers()[1].getMaster().getCurrentLife(game));
        }

        [Test]
        public void missingSpecterTest()
        {
            CardEngine game = TestGameflow.initStandardGame(null, new int[] {0, 1},
                Enumerable.Repeat(new Reimu(), 2).ToArray(),
                Enumerable.Repeat(Enumerable.Repeat(new MissingSpecter(), 30).ToArray(), 2).ToArray(),
                new HearthstoneOption() { });
            game.runGame();
            game.getSortedPlayers()[0].cmdInitReplace(game);
            game.getSortedPlayers()[1].cmdInitReplace(game);

            game.getSortedPlayers()[0].cmdTurnEnd(game);
            game.getSortedPlayers()[1].cmdTurnEnd(game);
            game.getSortedPlayers()[0].cmdTurnEnd(game);
            game.getSortedPlayers()[1].cmdTurnEnd(game);
            game.getSortedPlayers()[0].cmdTurnEnd(game);
            game.getSortedPlayers()[1].cmdTurnEnd(game);
            game.getSortedPlayers()[0].cmdTurnEnd(game);
            game.getSortedPlayers()[1].cmdTurnEnd(game);

            game.getSortedPlayers()[0].cmdUse(game, game.getSortedPlayers()[0].getHand()[0], 0);
            game.getSortedPlayers()[0].cmdTurnEnd(game);
            game.getSortedPlayers()[1].cmdUse(game, game.getSortedPlayers()[1].getHand()[0], 0);
            game.getSortedPlayers()[1].cmdTurnEnd(game);
            game.getSortedPlayers()[0].cmdAttack(game, game.getSortedPlayers()[0].getField()[0], game.getSortedPlayers()[1].getField()[0]);

            Assert.AreEqual(1, game.getSortedPlayers()[0].getField().count);
            Assert.AreEqual(1, game.getSortedPlayers()[1].getField().count);
        }

        [Test]
        public void missingSpecterTest_disable()
        {
            TestGameflow.createGame(out var game, out var you, out var oppo,
                new KeyValuePair<int, int>(MissingSpecter.ID, 30));
            game.skipTurnUntil(() => game.getCurrentPlayer() == you && you.getGem() >= game.getDefine<MissingSpecter>().cost);
            you.cmdUse(game, you.getHand()[0], 0);
            Assert.IsInstanceOf<MissingSpecter>(you.getField()[0].define);
            (you.getField()[0].define.getEffects()[0] as DeathRattle).onDisable(game, you.getField()[0], null); //类似沉默
            you.getField()[0].die(game); //干掉它
            Assert.AreEqual(0, you.getField().count);
        }

        [Test]
        public void drizzleFairyTest()
        {
            CardEngine game = TestGameflow.initGameWithoutPlayers(null, new HearthstoneOption()
            {
                shuffle = false
            });
            game.createPlayer(0, "玩家0", game.getDefine<Reimu>(), Enumerable
                .Repeat(game.getDefine<DrizzleFairy>() as CardDefine, 29)
                .Concat(Enumerable.Repeat(game.getDefine<FantasySeal>(), 1)));
            game.createPlayer(1, "玩家1", game.getDefine<Reimu>(), Enumerable
                .Repeat(game.getDefine<DrizzleFairy>() as CardDefine, 29)
                .Concat(Enumerable.Repeat(game.getDefine<FantasySeal>(), 1)));
            game.runGame();
            game.getSortedPlayers()[0].cmdInitReplace(game);
            game.getSortedPlayers()[1].cmdInitReplace(game);

            //第一回合
            game.getSortedPlayers()[0].cmdUse(game, game.getSortedPlayers()[0].getHand()[1], 0);

            game.getSortedPlayers()[0].cmdTurnEnd(game);

            game.getSortedPlayers()[1].cmdUse(game, game.getSortedPlayers()[1].getHand()[1], 0);

            game.getSortedPlayers()[1].cmdTurnEnd(game);
            //第二回合
            game.getSortedPlayers()[0].cmdUse(game, game.getSortedPlayers()[0].getHand()[1], 0);
            game.getSortedPlayers()[0].cmdUse(game, game.getSortedPlayers()[0].getHand()[1], 0);

            game.getSortedPlayers()[0].cmdTurnEnd(game);
            game.getSortedPlayers()[1].cmdTurnEnd(game);
            //第三回合，共计4个妖精
            game.getSortedPlayers()[0].cmdUse(game, game.getSortedPlayers()[0].getHand()[1], 0);

            game.getSortedPlayers()[0].cmdTurnEnd(game);
            game.getSortedPlayers()[1].cmdTurnEnd(game);
            //第四回合
            game.getSortedPlayers()[0].cmdTurnEnd(game);

            game.getSortedPlayers()[1].cmdUse(game, game.getSortedPlayers()[1].getHand()[0], 0);

            //对3个随从造成3点伤害
            THHCard.DamageEventArg damage =
                game.triggers.getRecordedEvents().LastOrDefault(e => e is THHCard.DamageEventArg) as
                    THHCard.DamageEventArg;
            Assert.AreEqual(3, damage.infoDic.Count);
            Assert.AreEqual(3, damage.infoDic.First().Value.damagedValue);
            Assert.AreEqual(1, game.getSortedPlayers()[0].getField().count);
        }

        [Test]
        public void flameFairyTest()
        {
            CardEngine game = TestGameflow.initGameWithoutPlayers(null, new HearthstoneOption()
            {
                shuffle = false
            }); //首先，上来先新建一个不带玩家的游戏
            game.createPlayer(0, "玩家0", game.getDefine<Reimu>(),
                Enumerable.Repeat(game.getDefine<FlameFairy>() as CardDefine, 30)); //创建两个玩家，给他们设置初始英雄，套牌
            game.createPlayer(1, "玩家1", game.getDefine<Reimu>(),
                Enumerable.Repeat(game.getDefine<FlameFairy>() as CardDefine, 30));
            game.runGame(); //运行游戏

            //炎之妖精 战吼：对一个随机敌方随从造成1点伤害
            //那么我们要测试这个战吼内容

            game.skipTurnWhen(() => game.getSortedPlayers()[0].getGem() < 1);

            game.getSortedPlayers()[0].cmdUse(game, game.getSortedPlayers()[0].getHand()[0], 0); //第一个行动的玩家使用手上的第一张牌，反正满手都是炎之妖精随便用
            Assert.IsInstanceOf<FlameFairy>(game.getSortedPlayers()[0].getField()[0].define); //预期场上拍了一个炎之妖精
            //不过因为对方场上没有随从，所以啥事也没发生。
            game.getSortedPlayers()[0].cmdTurnEnd(game); //第一个行动的玩家回合结束
            game.getSortedPlayers()[1].cmdUse(game, game.getSortedPlayers()[1].getHand()[0], 0);
            Assert.IsInstanceOf<FlameFairy>(game.getSortedPlayers()[1].getField()[0].define); //第二个行动的玩家拍一个炎之妖精，这个时候应该触发战吼的
            Assert.AreEqual(0, game.getSortedPlayers()[0].getField().count); //对面的妖精应该被打死了
            var damage =
                game.triggers.getRecordedEvents().LastOrDefault(e => e is THHCard.DamageEventArg) as
                    THHCard.DamageEventArg; //这会产生一个伤害事件
            Assert.AreEqual(1, damage.infoDic.First().Value.damagedValue); //伤害值为1

            //大概就是这样了，稍微测试一下保证每一张卡都在正常工作吧，能减少很多bug。
            //出了个随从重名的小问题，测试结果看看Log还是挺正常的，那就没有问题了。
            //做这个测试是为了防止以后对系统的改动导致已经做好的卡失效，这样能防止很多bug。
        }

        [Test]
        public void SunnyMilkTest()
        {
            CardEngine game = TestGameflow.initGameWithoutPlayers(null, new HearthstoneOption()
            {
                shuffle = false
            });
            game.createPlayer(0, "玩家0", game.getDefine<Reimu>(),
                Enumerable.Repeat(game.getDefine<SunnyMilk>() as CardDefine, 30));
            game.createPlayer(1, "玩家1", game.getDefine<Reimu>(),
                Enumerable.Repeat(game.getDefine<SunnyMilk>() as CardDefine, 30));

            game.skipTurnWhen(() => game.getSortedPlayers()[0].getGem() < 2);

            game.getSortedPlayers()[0].cmdUse(game, game.getSortedPlayers()[0].getHand()[0], 0);
            Assert.True(game.getSortedPlayers()[0].getField()[0].isStealth(game));
        }

        [Test]
        public void LunaChildTest()
        {
            CardEngine game = TestGameflow.initGameWithoutPlayers(null, new HearthstoneOption()
            {
                shuffle = false
            });
            game.createPlayer(0, "玩家0", game.getDefine<Reimu>(),
                Enumerable.Repeat(game.getDefine<LunaChild>() as CardDefine, 30));
            game.createPlayer(1, "玩家1", game.getDefine<Reimu>(), Enumerable
                .Repeat(game.getDefine<LunaChild>() as CardDefine, 29)
                .Concat(Enumerable.Repeat(game.getDefine<DefaultServant>(), 1)));
            game.skipTurnWhen(() => game.getSortedPlayers()[0].getGem() < 3);

            game.getSortedPlayers()[0].cmdUse(game, game.getSortedPlayers()[0].getHand()[0], 0);
            Assert.False(game.getSortedPlayers()[0].getField()[0].isStealth(game)); //default随从一开始没有潜行
            game.getSortedPlayers()[0].cmdUse(game, game.getSortedPlayers()[0].getHand()[1], 1, game.getSortedPlayers()[0].getField()[0]);
            Assert.True(game.getSortedPlayers()[0].getField()[0].isStealth(game)); //露娜出场后default随从变为潜行状态
        }

        /*[Test]
        public void StarSphereTest()
        {
            THHGame game = TestGameflow.initGameWithoutPlayers(null, new GameOption()
            {
                shuffle = false
            });
            game.createPlayer(0, "玩家0", game.getCardDefine<Reimu>(), Enumerable.Repeat(game.getCardDefine<LunaChild>() as CardDefine, 30));
            game.createPlayer(1, "玩家1", game.getCardDefine<Reimu>(), Enumerable.Repeat(game.getCardDefine<LunaChild>() as CardDefine, 28)
            .Concat(Enumerable.Repeat(game.getCardDefine<SunnyMilk>(), 1))
            .Concat(Enumerable.Repeat(game.getCardDefine<LunaChild>(), 1))
            .Concat(Enumerable.Repeat(game.getCardDefine<DefaultServant>(), 1)));
            game.skipTurnWhen(() => game.sortedPlayers[0].gem < 9);

            game.sortedPlayers[0].cmdUse(game, game.sortedPlayers[0].hand[0], 0);   //default
            game.sortedPlayers[0].cmdUse(game, game.sortedPlayers[0].hand[3], 1);   //斯塔
            Assert.False(game.sortedPlayers[0].field[0].isStealth(game));   //没有桑尼和露娜，光环没起效
            game.sortedPlayers[0].cmdUse(game, game.sortedPlayers[0].hand[1], 2);   //桑尼
            game.sortedPlayers[0].cmdUse(game, game.sortedPlayers[0].hand[0], 3);   //露娜
            game.sortedPlayers[0].cmdUse(game, game.sortedPlayers[0].hand[0], 4);
            Assert.True(game.sortedPlayers[0].field[2].isStealth(game));    //露娜和桑尼在场，光环起效
        }*/

        [Test]
        public void BeerFairyTest()
        {
            CardEngine game = TestGameflow.initGameWithoutPlayers(null, new HearthstoneOption()
            {
                shuffle = false
            });
            game.createPlayer(0, "玩家0", game.getDefine<Reimu>(),
                Enumerable.Repeat(game.getDefine<BeerFairy>() as CardDefine, 30));
            game.createPlayer(1, "玩家1", game.getDefine<Reimu>(),
                Enumerable.Repeat(game.getDefine<BeerFairy>() as CardDefine, 30));
            game.skipTurnWhen(() => game.getSortedPlayers()[0].getGem() < 4);

            var s = game.getSortedPlayers()[0].getHand()[0];
            game.getSortedPlayers()[0].cmdUse(game, s, 0);
            Assert.True(game.getSortedPlayers()[0].getField().Contains(s));
            game.getSortedPlayers()[0].cmdUse(game, game.getSortedPlayers()[0].getHand()[0], 0, game.getSortedPlayers()[0].getField()[0]);
            Assert.True(game.getSortedPlayers()[0].getHand().Contains(s));
            //TODO:落叶，这里就只有两个小熊猫相互测试，还不够。
            //还要测试手牌满了的情况下，小熊猫被挤死了
            /*game.skipTurnWhen(() => game.sortedPlayers[0].gem < 10);
            var ss = game.sortedPlayers[0].hand[0];
            game.sortedPlayers[0].cmdUse(game, ss, 0);
            Assert.True(game.sortedPlayers[0].field.Contains(ss));
            game.sortedPlayers[0].cmdUse(game, game.sortedPlayers[0].hand[0], 0, game.sortedPlayers[0].field[0]);
            Assert.True(ss.isDead(game));*/
        }

        [Test]
        public void Daiyousei2Test()
        {
            TestGameflow.createGame(out var game, out var you, out var oppo,
                new KeyValuePair<int, int>(Daiyousei_N.ID, 2)
            );
            game.skipTurnUntil(() => game.getCurrentPlayer() == oppo && oppo.getGem() >= 1);
            oppo.cmdUse(game, oppo.getHand().First());
            oppo.cmdTurnEnd(game);
            you.cmdUse(game, you.getHand().getCard<Daiyousei_N>());
            you.cmdAttack(game, you.getField()[0], oppo.getField()[0]);
            Assert.AreEqual(1, you.getGem()); //获得一个仅限本回合使用的法力水晶
            Assert.AreEqual(1, you.getMaxGem()); //最大法力值不变
            game.Dispose();
        }

        [Test]
        public void Cirno2Test()
        {
            TestGameflow.createGame(out var game, out var you, out var oppo,
                new KeyValuePair<int, int>(Cirno_N_YYM.ID, 2)
            );
            game.skipTurnUntil(() => game.getCurrentPlayer() == oppo && oppo.getGem() >= 1);
            oppo.cmdUse(game, oppo.getHand().First());
            oppo.cmdTurnEnd(game);
            you.cmdUse(game, you.getHand().getCard<Cirno_N_YYM>(), targets: oppo.getField()[0]);
            you.cmdTurnEnd(game);
            Assert.True(oppo.getField()[0].canAttack(game) == false);
            oppo.cmdAttack(game, oppo.getField()[0], you.getMaster()); //尝试攻击
            game.Dispose();
        }

        [Test]
        public void Cirno3Test()
        {
            TestGameflow.createGame(out var game, out var you, out var oppo,
                new KeyValuePair<int, int>(Cirno_N_HMX.ID, 2)
            );
            game.skipTurnUntil(() => game.getCurrentPlayer() == oppo && oppo.getGem() >= 3);
            for (int i = 0; i < 3; i++)
                oppo.cmdUse(game, oppo.getHand().First());
            oppo.cmdTurnEnd(game);
            you.cmdUse(game, you.getHand().getCard<Cirno_N_HMX>(), targets: oppo.getField()[1]);
            you.cmdTurnEnd(game);
            oppo.cmdAttack(game, oppo.getField()[0], you.getMaster()); //攻击失败
            oppo.cmdAttack(game, oppo.getField()[1], you.getMaster()); //可以攻击
            oppo.cmdAttack(game, oppo.getField()[2], you.getMaster()); //攻击失败
            game.Dispose();
        }

        [Test]
        public void LettyTest()
        {
            TestGameflow.createGame(out var game, out var you, out var oppo,
                new KeyValuePair<int, int>(LettyWhiterock_N.ID, 30));
            game.skipUntilCanUse<LettyWhiterock_N>(you);
            you.cmdUse(game, you.getHand().getCard<LettyWhiterock_N>());
            Assert.AreEqual(game.getDefine<LettyWhiterock_N>().cost,
                you.getHand().getCard<LettyWhiterock_N>().getCost(game));
            game.skipUntilHasGem(you, game.getDefine<LettyWhiterock_N>().cost * 2 + 1);
            Assert.AreEqual(game.getDefine<LettyWhiterock_N>().cost + 1,
                you.getHand().getCard<LettyWhiterock_N>().getCost(game));
            you.cmdUse(game, you.getHand().getCard<LettyWhiterock_N>());
            Assert.AreEqual(game.getDefine<LettyWhiterock_N>().cost,
                you.getHand().getCard<LettyWhiterock_N>().getCost(game));
            //game.skipUntilCanUse<LettyWhiterock_N>(you);
            //Assert.AreEqual(game.getCardDefine<LettyWhiterock_N>().cost + 1, you.hand.getCard<LettyWhiterock_N>().getCost(game));
            //THHGame game = TestGameflow.initGameWithoutPlayers(null, new GameOption() { shuffle = false });
            //THHPlayer you = game.createPlayer(0, "玩家0", game.getCardDefine<Reimu>(), Enumerable.Repeat(game.getCardDefine<LettyWhiterock_N>() as CardDefine, 4)
            //    .Concat(Enumerable.Repeat(game.getCardDefine<ArcaneKnowledge>(), 4)));
            //THHPlayer oppo = game.createPlayer(1, "玩家1", game.getCardDefine<Reimu>(), Enumerable.Repeat(game.getCardDefine<ArcaneKnowledge>() as CardDefine, 30));

            //game.skipTurnWhen(() => you.gem < 5);

            //you.cmdUse(game, you.hand.getCard<LettyWhiterock_N>());
            //you.cmdTurnEnd(game);
            //Assert.True(oppo.hand.Where(c => c.define is ArcaneKnowledge).All(c => c.getCost(game) == 5));
            //oppo.cmdUse(game, oppo.hand.First());
            //Assert.True(oppo.hand.Where(c => c.define is ArcaneKnowledge).All(c => c.getCost(game) == 4));
            //oppo.cmdTurnEnd(game);
            //you.cmdUse(game, you.hand.getCard<ArcaneKnowledge>());
        }

        [Test]
        public void Chen_NTest()
        {
            TestGameflow.createGame(out var game, out var you, out var oppo,
                new KeyValuePair<int, int>(Chen_N.ID, 2)
            );
            game.skipTurnUntil(() => game.getCurrentPlayer() == oppo && oppo.getGem() >= 2);
            oppo.cmdUse(game, oppo.getHand().First());
            oppo.cmdTurnEnd(game);
            you.cmdUse(game, you.getHand().getCard<Chen_N>());
            Assert.AreEqual(3, you.getField()[0].getAttack(game));
            Assert.True(you.getField()[0].isRush(game));
            game.Dispose();
        }

        [Test]
        public void Koakuma_NTest()
        {
            TestGameflow.createGame(out var game, out var you, out var oppo,
                new KeyValuePair<int, int>(Koakuma_N.ID, 2),
                new KeyValuePair<int, int>(DoyouSpear.ID, 2)
            );
            game.skipTurnUntil(() => game.getCurrentPlayer() == you && you.getGem() >= 6);
            you.cmdUse(game, you.getHand().getCard<DoyouSpear>());
            Pile.MoveCardEventArg movecardevent = game.triggers.getRecordedEvents().OfType<Pile.MoveCardEventArg>()
                .LastOrDefault(c => c.to == you.getGrave());
            you.cmdUse(game, you.getHand().getCard<Koakuma_N>());
            Assert.True(you.getHand().Contains(movecardevent.card));
        }

        [Test]
        public void AliceMargatroid_NTest()
        {
            TestGameflow.createGame(out var game, out var you, out var oppo,
                new KeyValuePair<int, int>(AliceMargatroid_N.ID, 1)
            );
            game.skipUntilCanUse<AliceMargatroid_N>(you);
            you.cmdUse(game, you.getHand().getCard<AliceMargatroid_N>());
            you.cmdTurnEnd(game);
            Assert.AreEqual(2, you.getField().count);
            Assert.IsInstanceOf<Doll_N>(you.getField()[1].define);
        }

        [Test]
        public void Lily_WhiteTest()
        {
            TestGameflow.createGame(out var game, out var you, out var oppo,
                new KeyValuePair<int, int>(LilyWhite_N.ID, 5),
                new KeyValuePair<int, int>(Doll_N.ID, 4)
            );
            game.skipTurnUntil(() => game.getCurrentPlayer() == you && you.getGem() >= 6);
            you.cmdUse(game, you.getHand().getCard<Doll_N>());
            you.cmdUse(game, you.getHand().getCard<Doll_N>());
            you.cmdUse(game, you.getHand().getCard<LilyWhite_N>());
            game.skipTurnWhen(() => you.getGem() < 7);
            var buff = you.getField().getCard<Doll_N>().getBuffs()[0];
            Assert.AreEqual(LilyWhite_N.ID, buff.id);
            Assert.True(you.getField()[1].getCurrentLife(game) == 2);
            Assert.True(you.getField()[1].getAttack(game) == 2);
        }

        [Test]
        public void KonpakuYoumu_NTest()
        {
            TestGameflow.createGame(out var game, out var you, out var oppo,
                new KeyValuePair<int, int>(KonpakuYoumu_N.ID, 6)
            );
            game.skipTurnUntil(() => game.getCurrentPlayer() == oppo && oppo.getGem() >= 5);
            oppo.cmdUse(game, oppo.getHand().First());
            oppo.cmdTurnEnd(game);
            you.cmdUse(game, you.getHand().getCard<KonpakuYoumu_N>());
            Assert.True(you.getField()[0].getMaxAttackTimes(game) == 2);
            you.cmdAttack(game, you.getField()[0], oppo.getField()[0]);
            you.cmdAttack(game, you.getField()[0], oppo.getField()[0]);
            Assert.False(you.getField()[0].canAttack(game));
        }

        [Test]
        public void RemiliaScarletTest()
        {
            TestGameflow.createGame(out var game, out var you, out var oppo,
                new KeyValuePair<int, int>(RemiliaScarlet_N.ID, 5),
                new KeyValuePair<int, int>(Doll_N.ID, 4)
            );
            game.skipTurnUntil(() => game.getCurrentPlayer() == you && you.getGem() >= 6);
            var handBeforeUse = you.getHand().ToArray();
            you.cmdUse(game, you.getHand().getCard<RemiliaScarlet_N>());
            Pile.MoveCardEventArg movecardevent = game.triggers.getRecordedEvents().OfType<Pile.MoveCardEventArg>()
                .LastOrDefault(c => c.to == you.getGrave());
            Assert.AreEqual(4,
                game.triggers.getRecordedEvents().OfType<Pile.MoveCardEventArg>()
                    .Count(c => c.to == you.getGrave())); //前面爆了一张牌，所以是4
            Card throwcard = movecardevent.card;
            Assert.True(handBeforeUse.Contains(throwcard));
        }

        [Test]
        public void FlandreScarletTest()
        {
            TestGameflow.createGame(out var game, out var you, out var oppo,
                new KeyValuePair<int, int>(FlandreScarlet_N.ID, 8)
            );
            game.skipTurnUntil(() => game.getCurrentPlayer() == oppo && oppo.getGem() >= 6);
            oppo.cmdUse(game, oppo.getHand().First());
            Card card = oppo.getField()[0];
            oppo.cmdTurnEnd(game);
            var handBeforeUse = you.getHand().ToArray();
            you.cmdUse(game, you.getHand().getCard<FlandreScarlet_N>());
            Pile.MoveCardEventArg movecardevent = game.triggers.getRecordedEvents().OfType<Pile.MoveCardEventArg>()
                .LastOrDefault(c => c.to == you.getGrave());
            Card throwcard = movecardevent.card;
            Assert.True(handBeforeUse.Contains(throwcard));
            Assert.True(oppo.getGrave().Contains(card));
        }

        [Test]
        public void IzayoiSakuyaTest()
        {
            TestGameflow.createGame(out var game, out var you, out var oppo,
                new KeyValuePair<int, int>(IzayoiSakuya_N.ID, 5),
                new KeyValuePair<int, int>(Doll_N.ID, 4)
            );
            game.skipTurnUntil(() => game.getCurrentPlayer() == you && you.getGem() >= 6);
            you.cmdUse(game, you.getHand().getCard<IzayoiSakuya_N>());
            Card card = game.triggers.getRecordedEvents().OfType<HearthStoneRule.DrawEventArg>().Last().card;
            Assert.True(you.getHand().Contains(card));
            Assert.AreEqual(0, card.getCost(game));
        }

        [Test]
        public void YakumoRanTest()
        {
            TestGameflow.createGame(out var game, out var you, out var oppo,
                new KeyValuePair<int, int>(YakumoRan_N.ID, 8)
            );
            game.skipTurnUntil(() => game.getCurrentPlayer() == oppo && oppo.getGem() >= 7);
            oppo.cmdUse(game, oppo.getHand().First());
            oppo.cmdTurnEnd(game);
            you.cmdUse(game, you.getHand().getCard<YakumoRan_N>());
            Card card = game.triggers.getRecordedEvents().OfType<HearthStoneRule.CreateTokenEventArg>().Last().card;
            Assert.IsInstanceOf<Chen_N>(card.define);
            Assert.True(you.getField()[1].hasKeyword(game, KEYWORD.RUSH));
            var buff = you.getField()[1].getBuffs()[0];
            Assert.AreEqual(YakumoRan_N.ID, buff.id);
        }

        [Test]
        public void SaigyoujiYuyukoTest()
        {
            TestGameflow.createGame(out var game, out var you, out var oppo,
                new KeyValuePair<int, int>(SaigyoujiYuyuko_N.ID, 9)
            );
            game.skipTurnUntil(() => game.getCurrentPlayer() == oppo && oppo.getGem() >= 8);
            oppo.cmdUse(game, oppo.getHand().First());
            Card card = oppo.getField()[0];
            oppo.cmdTurnEnd(game);
            you.cmdUse(game, you.getHand().getCard<SaigyoujiYuyuko_N>(), targets: card);
            Assert.True(oppo.getGrave().Contains(card));
        }

        [Test]
        public void YukariYakumoTest()
        {
            TestGameflow.createGame(out var game, out var you, out var oppo,
                new KeyValuePair<int, int>(YakumoYukari_N.ID, 1)
            );
            game.skipUntilCanUse<YakumoYukari_N>(you);
            you.cmdUse(game, you.getHand().getCard<DefaultServant>());
            you.cmdTurnEnd(game);
            for (int i = 0; i < 7; i++)
            {
                oppo.cmdUse(game, oppo.getHand()[0]);
            }

            oppo.cmdTurnEnd(game);
            you.cmdUse(game, you.getHand().getCard<YakumoYukari_N>());
            Assert.AreEqual(1, you.getField().count);
            Assert.IsInstanceOf<YakumoYukari_N>(you.getField()[0].define);
            Assert.AreEqual(0, oppo.getField().count);
            Assert.AreEqual(10, oppo.getHand().count);
        }

        [Test]
        public void caseYukariOverflowTest()
        {
            TestGameflow.createGame(out var game, out var you, out _,
                new KeyValuePair<int, int>(YakumoYukari_N.ID, 1));
            game.skipUntilCanUse<YakumoYukari_N>(you);
            _ = you.createToken(game, 6, game.getDefine<DefaultServant>());
            Assert.AreEqual(6, you.getField().count);
            int graveCount = you.getGrave().count;
            you.cmdUse(game, you.getHand().getCard<YakumoYukari_N>());
            Assert.AreEqual(10, you.getHand().count);
            Assert.AreEqual(graveCount + 5, you.getGrave().count);
        }

        [Test]
        public void Rumia_NTest()
        {
            TestGameflow.createGame(out var game, out var you, out var oppo,
                new KeyValuePair<int, int>(Rumia_N.ID, 6)
            );
            game.skipTurnUntil(() => game.getCurrentPlayer() == oppo && oppo.getGem() >= 3);
            oppo.cmdUse(game, oppo.getHand().First(), 0); //敌方杂鱼A
            oppo.cmdUse(game, oppo.getHand().First(), 1); //敌方杂鱼B
            oppo.cmdTurnEnd(game);
            you.cmdUse(game, you.getHand().getCard<Rumia_N>());
            game.skipTurnUntil(() => game.getCurrentPlayer() == you && you.getGem() >= 4);
            Assert.True(you.getField().getCard<Rumia_N>().canAttack(game));
            you.cmdAttack(game, you.getField().getCard<Rumia_N>(), target: oppo.getMaster()); //尝试指脸
            Card target = game.triggers.getRecordedEvents().OfType<THHCard.AttackEventArg>().Last().target;
            game.logger.log("目标是：" + target); //真正的攻击目标
            you.cmdTurnEnd(game);
        }

        [Test]
        public void PrismriverTest()
        {
            TestGameflow.createGame(out var game, out var you, out var oppo,
                new KeyValuePair<int, int>(LunasaPrismriver_N.ID, 10),
                new KeyValuePair<int, int>(MerlinPrismriver_N.ID, 10),
                new KeyValuePair<int, int>(LyricaPrismriver_N.ID, 10)
            );
            game.skipTurnUntil(() => game.getCurrentPlayer() == you && you.getGem() >= 6);
            you.cmdUse(game, you.getHand().First()); //从牌库或手牌召唤骚灵乐团...（一起召唤吗？）
            Assert.True(you.getField().Any(c => c.define is LunasaPrismriver_N));
            Assert.True(you.getField().Any(c => c.define is MerlinPrismriver_N));
            Assert.True(you.getField().Any(c => c.define is LyricaPrismriver_N));
        }

        [Test]
        public void yakumoRanTest()
        {
            TestGameflow.createGame(out var game, out var you, out var oppo,
                new KeyValuePair<int, int>(YakumoRan_N.ID, 2),
                new KeyValuePair<int, int>(YakumoYukari_N.ID, 2)
            );
            game.skipTurnUntil(() => game.getCurrentPlayer() == you && you.getGem() >= 1);
            you.setGem(game, 10);
            you.cmdUse(game, you.getHand().getCard<YakumoRan_N>());
            Card chen = you.getField()[1];
            you.setGem(game, 10);
            you.cmdUse(game, you.getHand().getCard<YakumoYukari_N>());
            you.setGem(game, 10);
            you.cmdUse(game, chen);
            Assert.True(chen.hasKeyword(game, KEYWORD.RUSH));
        }

        [Test]
        public void caseChenBackToHandTest()
        {
            TestGameflow.createGame(out var game, out var you, out var oppo,
                new KeyValuePair<int, int>(Chen_N.ID, 1));
            game.skipUntilCanUse<DefaultServant>(oppo);
            oppo.cmdUse(game, oppo.getHand().getCard<DefaultServant>());
            game.skipUntilCanUse(you, game.getDefine<Chen_N>(), game.getDefine<Chen_N>());
            you.cmdUse(game, you.getHand().getCard<Chen_N>());
            Assert.True(you.getField().getCard<Chen_N>().isRush(game));
            Assert.True(you.getField().getCard<Chen_N>().canAttack(game));
            you.cmdAttack(game, you.getField().getCard<Chen_N>(), oppo.getField()[0]);
            you.getField().getCard<Chen_N>().fieldToHand(game);
            Assert.False(you.getHand().getCard<Chen_N>().isRush(game));
            you.cmdUse(game, you.getHand().getCard<Chen_N>());
            Assert.True(you.getField().getCard<Chen_N>().isRush(game));
            Assert.True(you.getField().getCard<Chen_N>().canAttack(game));
            you.cmdAttack(game, you.getField().getCard<Chen_N>(), oppo.getField()[0]);
        }

        [Test]
        public void MoonedInsect_Wriggle_NTest()
        {
            CardEngine game = TestGameflow.initGameWithoutPlayers(null, new HearthstoneOption()
            {
                shuffle = false
            });
            Player oppo = game.createPlayer(0, "玩家0", game.getDefine<TestMaster2>(),
                Enumerable.Repeat(game.getDefine<DefaultServant>() as CardDefine, 30));
            Player you = game.createPlayer(1, "玩家1", game.getDefine<TestMaster2>(),
                Enumerable.Repeat(game.getDefine<DefaultServant>() as CardDefine, 27)
                    .Concat(Enumerable.Repeat(game.getDefine<WriggleNightbug_N>(), 3)));

            game.skipUntilCanUse<WriggleNightbug_N>(you);
            you.cmdUse(game, you.getHand().getCard<WriggleNightbug_N>());
            you.cmdTurnEnd(game);
            oppo.cmdTurnEnd(game);
            you.getField()[0].setDead(game, true);
            game.updateDeath();

            game.Dispose();
        }

        [Test]
        public void FlowerMix_Mystia_NTest()
        {
            CardEngine game = TestGameflow.initGameWithoutPlayers(null, new HearthstoneOption()
            {
                shuffle = false
            });
            Player oppo = game.createPlayer(0, "玩家0", game.getDefine<TestMaster2>(),
                Enumerable.Repeat(game.getDefine<DefaultServant>() as CardDefine, 30));
            Player you = game.createPlayer(1, "玩家1", game.getDefine<TestMaster2>(),
                Enumerable.Repeat(game.getDefine<DefaultServant>() as CardDefine, 27)
                    .Concat(Enumerable.Repeat(game.getDefine<MystiaLorelei_N>(), 3)));

            game.skipUntilCanUse<DefaultServant>(oppo);
            oppo.cmdUse(game, oppo.getHand().getCard<DefaultServant>());

            game.skipUntilCanUse<MystiaLorelei_N>(you);
            you.cmdUse(game, you.getHand().getCard<MystiaLorelei_N>(), targets: oppo.getField()[0]);

            you.cmdTurnEnd(game);

            oppo.cmdTurnEnd(game);


            game.Dispose();
        }

        [Test]
        public void InvisibleFullMoon_Reisen_NTest()
        {
            TestGameflow.createGame(out var game, out var you, out var oppo,
                new KeyValuePair<int, int>(ReisenUdongeinInaba_N.ID, 2)
            );
            game.skipUntilCanUse<ReisenUdongeinInaba_N>(you);
            you.cmdUse(game, you.getHand().getCard<ReisenUdongeinInaba_N>());

            game.Dispose();
        }

        [Test]
        public void TewiInaba_NTest()
        {
            TestGameflow.createGame(out var game, out var you, out var oppo,
                new KeyValuePair<int, int>(TewiInaba_N.ID, 2)
            );
            game.skipUntilCanUse<TewiInaba_N>(you);
            you.cmdUse(game, you.getHand().getCard<TewiInaba_N>());

            DiscoverRequest discoverRequest = game.answers.getRequest<DiscoverRequest>(you.id);
            int cardId = discoverRequest.cardIdArray[0];
            Card card = you.getDeck().getCard(cardId);
            you.cmdDiscover(game, cardId);
            you.draw(game);
            Assert.True(you.getHand().Contains(card));
            game.Dispose();
        }

        [Test]
        public void FullMoonNight_Keine_N()
        {
            CardEngine game = TestGameflow.initGameWithoutPlayers(null, new HearthstoneOption()
            {
                shuffle = false
            });
            Player oppo = game.createPlayer(0, "玩家0", game.getDefine<TestMaster2>(),
                Enumerable.Repeat(game.getDefine<DefaultServant>() as CardDefine, 30));
            Player you = game.createPlayer(1, "玩家1", game.getDefine<TestMaster2>(),
                Enumerable.Repeat(game.getDefine<LuckyCoin>() as CardDefine, 15)
                    .Concat(Enumerable.Repeat(game.getDefine<KamishirasawaKeine_Human_N>(), 1)));

            game.skipUntilHasGem(you, 1);
            game.skipUntilHasGem(you, 2);
            game.skipUntilHasGem(you, 3);
            game.skipUntilHasGem(you, 4);

            game.Dispose();
        }

        [Test]
        public void Mokou_NTest()
        {
            CardEngine game = TestGameflow.initGameWithoutPlayers(null, new HearthstoneOption()
            {
                shuffle = false
            });
            Player oppo = game.createPlayer(0, "玩家0", game.getDefine<TestMaster2>(),
                Enumerable.Repeat(game.getDefine<DefaultServant>() as CardDefine, 30));
            Player you = game.createPlayer(1, "玩家1", game.getDefine<TestMaster2>(),
                Enumerable.Repeat(game.getDefine<DefaultServant>() as CardDefine, 29)
                    .Concat(Enumerable.Repeat(game.getDefine<HuziwaraNoMokou_N>(), 1)));

            for (int i = 0; i < 3; i++)
            {
                game.skipUntilCanUse<DefaultServant>(oppo);
                oppo.cmdUse(game, oppo.getHand().getCard<DefaultServant>());
            }

            for (int i = 0; i < 3; i++)
            {
                game.skipUntilCanUse<DefaultServant>(you);
                you.cmdUse(game, you.getHand().getCard<DefaultServant>());
            }

            game.skipUntilCanUse<HuziwaraNoMokou_N>(you);
            you.cmdUse(game, you.getHand().getCard<HuziwaraNoMokou_N>());
            Assert.True(you.getField()[0].canAttack(game));

            you.getField()[0].setDead(game, true);
            game.updateDeath();
            you.cmdTurnEnd(game);
            oppo.cmdTurnEnd(game);

            you.cmdTurnEnd(game);
            oppo.cmdTurnEnd(game);

            game.Dispose();
        }

        [Test]
        public void Eirin_NTest()
        {
            TestGameflow.createGame(out var game, out var you, out var oppo,
                new KeyValuePair<int, int>(YagokoroEirin_N.ID, 2)
            );
            game.skipUntilCanUse<YagokoroEirin_N>(you);
            you.cmdUse(game, you.getHand().getCard<YagokoroEirin_N>(), targets: oppo.getMaster());
            DiscoverRequest discoverRequest = game.answers.getRequest<DiscoverRequest>(you.id);
            int cardId = discoverRequest.cardIdArray[1];
            // Card card = you.deck.getCard(cardId);
            you.cmdDiscover(game, cardId);

            game.skipUntilCanUse<YagokoroEirin_N>(you);
            you.cmdUse(game, you.getHand().getCard<YagokoroEirin_N>(), targets: oppo.getMaster());
            DiscoverRequest discoverRequest2 = game.answers.getRequest<DiscoverRequest>(you.id);
            int cardId2 = discoverRequest2.cardIdArray[0];
            you.cmdDiscover(game, cardId2);

            game.Dispose();
        }

        [Test]
        public void Kaguya_NTest()
        {
            TestGameflow.createGame(out var game, out var you, out var oppo,
                new KeyValuePair<int, int>(HouraisanKaguya_N.ID, 2)
            );
            

            game.skipUntilCanUse<HouraisanKaguya_N>(you);


            you.cmdUse(game, you.getHand().getCard<DefaultServant>());
            you.cmdUse(game, you.getHand().getCard<DefaultServant>());
            you.cmdUse(game, you.getHand().getCard<DefaultServant>());
            you.cmdUse(game, you.getHand().getCard<DefaultServant>());
            

            you.cmdTurnEnd(game);
            oppo.cmdTurnEnd(game);

            you.cmdUse(game, you.getHand().getCard<HouraisanKaguya_N>());
            DiscoverRequest discoverRequest1 = game.answers.getRequest<DiscoverRequest>(you.id);
            int cardId1 = discoverRequest1.cardIdArray.Where(c=>c == ReturnOfImperishableNight_Eternity_N.ID).First();
            you.cmdDiscover(game, cardId1);

            you.draw(game, out HearthStoneRule.DrawEventArg arg01);
            if (!arg01.isCanceled)
                Debug.Log("1！");

            you.cmdTurnEnd(game);
            oppo.cmdTurnEnd(game);

            you.draw(game, out HearthStoneRule.DrawEventArg arg02);
            if (!arg02.isCanceled)
                Debug.Log("2！");

            you.cmdTurnEnd(game);
            oppo.cmdTurnEnd(game);

            you.draw(game, out HearthStoneRule.DrawEventArg arg03);
            if (!arg03.isCanceled)
                Debug.Log("永夜完结！");

            you.cmdUse(game, you.getHand().getCard<HouraisanKaguya_N>());
            DiscoverRequest discoverRequest2 = game.answers.getRequest<DiscoverRequest>(you.id);
            int cardId2 = discoverRequest2.cardIdArray.Where(c => c == ReturnOfImperishableNight_Eternity_N.ID).First();
            you.cmdDiscover(game, cardId2);

            you.draw(game, out HearthStoneRule.DrawEventArg arg11);
            if (!arg11.isCanceled)
                Debug.Log("1！");

            you.cmdTurnEnd(game);
            oppo.cmdTurnEnd(game);

            you.draw(game, out HearthStoneRule.DrawEventArg arg12);
            if (!arg12.isCanceled)
                Debug.Log("2！");

            you.cmdTurnEnd(game);
            oppo.cmdTurnEnd(game);

            you.draw(game, out HearthStoneRule.DrawEventArg arg13);
            if (!arg13.isCanceled)
                Debug.Log("永夜真的完结！");

            game.Dispose();
        }

        [Test]
        public void HoanMeirinTest()
        {
            TestGameflow.createGame(out var game, out var you, out var oppo,
                new KeyValuePair<int, int>(HoanMeirin_N.ID, 1));
            game.skipUntilCanUse<DefaultServant>(oppo);
            oppo.cmdUse<DefaultServant>(game);
            game.skipUntilCanUse<HoanMeirin_N>(you);
            you.cmdUse<HoanMeirin_N>(game);
            game.skipUntilYourTurn(oppo);
            int originAttack = game.getDefine<HoanMeirin_N>().attack;
            Assert.AreEqual(originAttack, you.getField().getCard<HoanMeirin_N>().getAttack(game));
            oppo.cmdAttack<DefaultServant>(game, you.getField().getCard<HoanMeirin_N>());
            Assert.AreEqual(game.getDefine<DefaultServant>().life - originAttack,
                oppo.getField().getCard<DefaultServant>().getCurrentLife(game));
            Assert.AreEqual(originAttack + 3, you.getField().getCard<HoanMeirin_N>().getAttack(game));
        }

        [Test]
        public void AkiSizuha_NTest()
        {
            TestGameflow.createGame(out var game, out var you, out var oppo,
                new KeyValuePair<int, int>(AkiSizuha_N.ID, 1));
            game.skipUntilCanUse<AkiSizuha_N>(you);
            you.cmdUse(game, you.getHand().getCard<AkiSizuha_N>());
            Assert.AreEqual(0, oppo.getHand().Count(c => c.define is Neutral_73));
            you.cmdTurnEnd(game);
            Assert.AreEqual(1, oppo.getHand().Count(c => c.define is Neutral_73));
            game.Dispose();
        }

        [Test]
        public void AkiMinoriko_NTest()
        {
            TestGameflow.createGame(out var game, out var you, out var oppo,
                new KeyValuePair<int, int>(AkiMinoriko_N.ID, 1));
            game.skipUntilCanUse<AkiMinoriko_N>(you);
            you.cmdUse(game, you.getHand().getCard<AkiMinoriko_N>());
            Assert.AreEqual(0, you.getHand().Count(c => c.define is Neutral_74));
            you.cmdTurnEnd(game);
            Assert.AreEqual(1, you.getHand().Count(c => c.define is Neutral_74));
            game.skipUntilCanUse<DefaultServant>(you);
            you.cmdUse<DefaultServant>(game, out var s);
            game.skipUntilCanUse<Neutral_74>(you);
            int life = s.getCurrentLife(game);
            you.cmdUse<Neutral_74>(game, 0, s);
            Assert.AreEqual(life + 2, s.getCurrentLife(game));
            game.Dispose();
        }

        [Test]
        public void KagiyamaHina_NTest()
        {
            TestGameflow.createGame(out var game, out var you, out var oppo,
                new KeyValuePair<int, int>(KagiyamaHina_N.ID, 1));
            for (int i = 0; i < 2; i++)
            {
                game.skipUntilCanUse<DefaultServant>(you);
                you.cmdUse<DefaultServant>(game);
            }

            game.skipUntilCanUse<KagiyamaHina_N>(you);
            you.cmdUse(game, you.getHand().getCard<KagiyamaHina_N>(), 1);
            Assert.True(you.getField()[1].define is KagiyamaHina_N);
            Assert.True(you.getField()[0].isSilenced(game));
            Assert.True(you.getField()[2].isSilenced(game));
            game.Dispose();
        }

        [Test]
        public void KawasiroNitori_NTest()
        {
            TestGameflow.createGame(out var game, out var you, out var oppo,
                new KeyValuePair<int, int>(KawasiroNitori_N.ID, 1));
            game.skipUntilCanUse<KawasiroNitori_N>(you);
            you.cmdUse<KawasiroNitori_N>(game, out var s);
            Assert.True(s.hasKeyword(game, KEYWORD.HAS_SHIELD) && s.isStealth(game));
            game.Dispose();
        }

        [Test]
        public void InubashiriMomizi_NTest()
        {
            TestGameflow.createGame(out var game, out var you, out var oppo,
                new KeyValuePair<int, int>(InubashiriMomizi_N.ID, 2));
            game.skipUntilCanUse<DefaultServant>(oppo);
            oppo.cmdUse<DefaultServant>(game);
            game.skipUntilCanUse<InubashiriMomizi_N>(you);
            Assert.Less(oppo.getField().count, 2);
            var handCount = you.getHand().count;
            you.cmdUse(game, you.getHand().getCard<InubashiriMomizi_N>());
            Assert.AreEqual(handCount - 1, you.getHand().count);
            game.skipUntilCanUse<DefaultServant>(oppo);
            oppo.cmdUse<DefaultServant>(game);
            game.skipUntilCanUse<InubashiriMomizi_N>(you);
            Assert.AreEqual(2, oppo.getField().count);
            handCount = you.getHand().count;
            you.cmdUse(game, you.getHand().getCard<InubashiriMomizi_N>());
            Assert.AreEqual(handCount, you.getHand().count);
            game.Dispose();
        }

        [Test]
        public void ShameimaruAya_NTest()
        {
            TestGameflow.createGame(out var game, out var you, out var oppo,
                new KeyValuePair<int, int>(ShameimaruAya_N.ID, 1));
            game.skipUntilCanUse<ShameimaruAya_N>(you);
            int c1 = oppo.getHand().count;
            int c2 = you.getHand().count;
            you.cmdUse<ShameimaruAya_N>(game, out var aya);
            Assert.AreEqual(c1 + 1, oppo.getHand().count);
            Assert.AreEqual(c2, you.getHand().count);
            Assert.True(aya.canAttack(game));
            you.cmdAttack(game, aya, oppo.getMaster());
            game.Dispose();
        }

        [Test]
        public void MoriyaSuwako_NTest()
        {
            TestGameflow.createGame(out var game, out var you, out var oppo,
                new KeyValuePair<int, int>(MoriyaSuwako_N.ID, 1));
            for (int i = 0; i < 2; i++)
            {
                game.skipUntilCanUse<DefaultServant>(oppo);
                oppo.cmdUse<DefaultServant>(game);
            }

            game.skipUntilCanUse<MoriyaSuwako_N>(you);
            you.cmdUse<MoriyaSuwako_N>(game, out var s);
            Assert.True(s.canAttack(game));
            s.setDead(game, true);
            game.updateDeath();
            Assert.AreEqual(you.getField().count, oppo.getField().count);
            Assert.True(you.getField()[0].define is Frog_N && you.getField()[0].canAttack(game));
            game.Dispose();
        }

        [Test]
        public void Sanae_NTest()
        {
            TestGameflow.createGame(out var game, out var you, out var oppo,
                new KeyValuePair<int, int>(Sanae_N.ID, 1));

            game.skipUntilCanUse<Sanae_N>(you);
            you.cmdUse(game, you.getHand().getCard<Sanae_N>());
            //TODO: 
            game.Dispose();
        }

        [Test]
        public void YasakaKanako_NTest()
        {
            TestGameflow.createGame(out var game, out var you, out var oppo,
                new KeyValuePair<int, int>(YasakaKanako_N.ID, 7));

            game.skipUntilCanUse<YasakaKanako_N>(you);
            var totalCount = you.getHand().count;
            var count = you.getHand().Count(c => c.getCost(game) >= 5);
            you.cmdUse<YasakaKanako_N>(game);
            Assert.AreEqual(totalCount - count, you.getHand().count);
            Assert.AreEqual(count, you.getField().count);
            Assert.AreEqual(you.getField().count - 1, you.getField().Count(c => c.define is Onbashira));
            game.Dispose();
        }

        [Test]
        public void KisumeTest()
        {
            TestGameflow.createGame<DefaultServant, Onbashira>(out var game, out var you, out var oppo,
                new KeyValuePair<int, int>(Kisume.ID, 2));
            game.skipTurnUntil((() => you.getHand().Any(card => card.define is Kisume)));
            game.skipUntilCanUse<Onbashira>(oppo);
            int count = you.getHand().Count(card => card.define is Kisume);
            oppo.cmdUse<Onbashira>(game);
            Assert.AreEqual(count, you.getField().count);
            game.Dispose();
        }

        [Test]
        public void KurodaniYamameTest()
        {
            TestGameflow.createGame(out var game, out var you, out var oppo,
                new KeyValuePair<int, int>(KurodaniYamame.ID, 2));
            game.skipUntilCanUse<DefaultServant>(oppo);
            oppo.cmdUse<DefaultServant>(game, out var ds);
            game.skipUntilCanUse<KurodaniYamame>(you);
            you.cmdUse<KurodaniYamame>(game, out var s);
            you.cmdTurnEnd(game);
            game.skipUntilYourTurn(you);
            you.cmdAttack(game, s, ds);
            Assert.False(ds.isDead(game));
            Assert.AreEqual(1, ds.getCurrentLife(game));
            game.Dispose();
        }

        [Test]
        public void MizuhashiParseeTest()
        {
            TestGameflow.createGame(out var game, out var you, out var oppo,
                new KeyValuePair<int, int>(MizuhashiParsee.ID, 2));
            game.skipUntilCanUse<DefaultServant>(oppo);
            oppo.cmdUse<DefaultServant>(game, out var def);
            game.skipUntilCanUse<MizuhashiParsee>(you);
            you.cmdUse<MizuhashiParsee>(game, out var s);
            Assert.AreEqual(2, s.getAttack(game));
            game.skipUntilCanUse<MizuhashiParsee>(you);
            def.setAttack(game, 3);
            you.cmdUse<MizuhashiParsee>(game, out s);
            Assert.AreEqual(4, s.getAttack(game));
            game.Dispose();
        }

        [Test]
        public void HoshigumaYugiTest()
        {
            TestGameflow.createGame(out var game, out var you, out var oppo,
                new KeyValuePair<int, int>(HoshigumaYugi.ID, 2));
            game.skipUntilCanUse<DefaultServant>(oppo);
            var yugi = you.getHand().getCard<HoshigumaYugi>();
            Assert.NotNull(yugi);
            Assert.AreEqual(9, yugi.getCost(game));
            oppo.cmdUse<DefaultServant>(game, out var def);
            Assert.AreEqual(9 - def.getAttack(game), yugi.getCost(game));
            def.addBuff(game, new CodeBuff(1, new AttackModifier(2)));
            Assert.AreEqual(9 - def.getAttack(game), yugi.getCost(game));
            def.setDead(game, true);
            game.updateDeath();
            Assert.AreEqual(9,yugi.getCost(game));
            game.Dispose();
        }

        [Test]
        public void KomeijiSatoriTest()
        {
            TestGameflow.createGame<KomeijiSatori, DefaultServant>(out var game, out var you, out var oppo);
            game.skipUntilHasGem(oppo, 10);
            oppo.cmdUse<DefaultServant>(game, out var def);
            game.skipUntilYourTurn(you);
            you.cmdUse<KomeijiSatori>(game);
            DiscoverRequest discoverRequest = game.answers.getRequest<DiscoverRequest>(you.id);
            Assert.NotNull(discoverRequest);
            int id = discoverRequest.cardIdArray[0];
            you.cmdDiscover(game, id);
            Assert.AreEqual(1, you.getHand().Count(c => c.define is DefaultServant));
        }

        [Test]
        public void KaenbyouRinTest()
        {
            TestGameflow.createGame<KaenbyouRin, DefaultServant>(out var game, out var you, out var oppo);
            game.skipUntilHasGem(oppo, 10);
            for (int i = 0; i < 3; i++)
            {
                oppo.cmdUse<DefaultServant>(game);
            }
            game.skipUntilYourTurn(you);
            you.cmdUse<KaenbyouRin>(game, out var rin);
            var atk = rin.getAttack(game);
            Assert.AreEqual(1,atk);
            oppo.getField().setDead(game, true);
            rin.setDead(game,true);
            game.updateDeath();
            you.cmdUse<KaenbyouRin>(game, out rin);
            Assert.AreEqual(atk+4, rin.getAttack(game));
        }

        [Test]
        public void ReiujiUtsuhoTest()
        {
            TestGameflow.createGame<ReiujiUtsuho, DefaultServant>(out var game, out var you, out var oppo);
            game.skipUntilHasGem(oppo, 10);
            for (int i = 0; i < 3; i++)
            {
                oppo.cmdUse<DefaultServant>(game);
            }

            game.skipUntilYourTurn(you);
            you.cmdUse<ReiujiUtsuho>(game);
            you.cmdTurnEnd(game);
            Assert.AreEqual(oppo.getField()[0].getLife(game) - 3, oppo.getField()[0].getCurrentLife(game));
            Assert.AreEqual(you.getMaster().getLife(game) - 3, you.getMaster().getCurrentLife(game));
        }
        
        [Test]
        public void KomeijiKoishiTest()
        {
            throw new NotImplementedException();
        }
    }
}