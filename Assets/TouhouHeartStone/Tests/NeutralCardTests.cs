using System.Collections;
using NUnit.Framework;
using UnityEngine.TestTools;
using System.Linq;
using TouhouHeartstone;
using TouhouCardEngine;
using TouhouHeartstone.Builtin;
using System.Collections.Generic;
using UnityEngine.XR;

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
            Assert.True(game.sortedPlayers[0].field[0].isCharge(game));
            Assert.True(game.sortedPlayers[0].field[0].canAttack(game));
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
            Assert.True(game.sortedPlayers[0].field[0].isTaunt(game));
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
            Assert.AreEqual(28, game.sortedPlayers[1].master.getCurrentLife(game));
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
        public void missingSpecterTest_disable()
        {
            TestGameflow.createGame(out var game, out var you, out var oppo,
                new KeyValuePair<int, int>(MissingSpecter.ID, 30));
            game.skipTurnUntil(() => game.currentPlayer == you && you.gem >= game.getDefine<MissingSpecter>().cost);
            you.cmdUse(game, you.hand[0], 0);
            Assert.IsInstanceOf<MissingSpecter>(you.field[0].define);
            (you.field[0].define.effects[0] as DeathRattle).onDisable(game, you.field[0], null);//类似沉默
            you.field[0].die(game);//干掉它
            Assert.AreEqual(0, you.field.count);
        }
        [Test]
        public void drizzleFairyTest()
        {
            THHGame game = TestGameflow.initGameWithoutPlayers(null, new GameOption()
            {
                shuffle = false
            });
            game.createPlayer(0, "玩家0", game.getDefine<Reimu>(), Enumerable.Repeat(game.getDefine<DrizzleFairy>() as CardDefine, 29)
            .Concat(Enumerable.Repeat(game.getDefine<FantasySeal>(), 1)));
            game.createPlayer(1, "玩家1", game.getDefine<Reimu>(), Enumerable.Repeat(game.getDefine<DrizzleFairy>() as CardDefine, 29)
            .Concat(Enumerable.Repeat(game.getDefine<FantasySeal>(), 1)));
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
        [Test]
        public void flameFairyTest()
        {
            THHGame game = TestGameflow.initGameWithoutPlayers(null, new GameOption()
            {
                shuffle = false
            });//首先，上来先新建一个不带玩家的游戏
            game.createPlayer(0, "玩家0", game.getDefine<Reimu>(), Enumerable.Repeat(game.getDefine<FlameFairy>() as CardDefine, 30));//创建两个玩家，给他们设置初始英雄，套牌
            game.createPlayer(1, "玩家1", game.getDefine<Reimu>(), Enumerable.Repeat(game.getDefine<FlameFairy>() as CardDefine, 30));
            game.run();//运行游戏

            //炎之妖精 战吼：对一个随机敌方随从造成1点伤害
            //那么我们要测试这个战吼内容

            game.skipTurnWhen(() => game.sortedPlayers[0].gem < 1);

            game.sortedPlayers[0].cmdUse(game, game.sortedPlayers[0].hand[0], 0);//第一个行动的玩家使用手上的第一张牌，反正满手都是炎之妖精随便用
            Assert.IsInstanceOf<FlameFairy>(game.sortedPlayers[0].field[0].define);//预期场上拍了一个炎之妖精
            //不过因为对方场上没有随从，所以啥事也没发生。
            game.sortedPlayers[0].cmdTurnEnd(game);//第一个行动的玩家回合结束
            game.sortedPlayers[1].cmdUse(game, game.sortedPlayers[1].hand[0], 0);
            Assert.IsInstanceOf<FlameFairy>(game.sortedPlayers[1].field[0].define);//第二个行动的玩家拍一个炎之妖精，这个时候应该触发战吼的
            Assert.AreEqual(0, game.sortedPlayers[0].field.count);//对面的妖精应该被打死了
            var damage = game.triggers.getRecordedEvents().LastOrDefault(e => e is THHCard.DamageEventArg) as THHCard.DamageEventArg;//这会产生一个伤害事件
            Assert.AreEqual(1, damage.value);//伤害值为1

            //大概就是这样了，稍微测试一下保证每一张卡都在正常工作吧，能减少很多bug。
            //出了个随从重名的小问题，测试结果看看Log还是挺正常的，那就没有问题了。
            //做这个测试是为了防止以后对系统的改动导致已经做好的卡失效，这样能防止很多bug。
        }

        [Test]
        public void SunnyMilkTest()
        {
            THHGame game = TestGameflow.initGameWithoutPlayers(null, new GameOption()
            {
                shuffle = false
            });
            game.createPlayer(0, "玩家0", game.getDefine<Reimu>(), Enumerable.Repeat(game.getDefine<SunnyMilk>() as CardDefine, 30));
            game.createPlayer(1, "玩家1", game.getDefine<Reimu>(), Enumerable.Repeat(game.getDefine<SunnyMilk>() as CardDefine, 30));

            game.skipTurnWhen(() => game.sortedPlayers[0].gem < 2);

            game.sortedPlayers[0].cmdUse(game, game.sortedPlayers[0].hand[0], 0);
            Assert.True(game.sortedPlayers[0].field[0].isStealth(game));
        }

        [Test]
        public void LunaChildTest()
        {
            THHGame game = TestGameflow.initGameWithoutPlayers(null, new GameOption()
            {
                shuffle = false
            });
            game.createPlayer(0, "玩家0", game.getDefine<Reimu>(), Enumerable.Repeat(game.getDefine<LunaChild>() as CardDefine, 30));
            game.createPlayer(1, "玩家1", game.getDefine<Reimu>(), Enumerable.Repeat(game.getDefine<LunaChild>() as CardDefine, 29)
            .Concat(Enumerable.Repeat(game.getDefine<DefaultServant>(), 1)));
            game.skipTurnWhen(() => game.sortedPlayers[0].gem < 3);

            game.sortedPlayers[0].cmdUse(game, game.sortedPlayers[0].hand[0], 0);
            Assert.False(game.sortedPlayers[0].field[0].isStealth(game));   //default随从一开始没有潜行
            game.sortedPlayers[0].cmdUse(game, game.sortedPlayers[0].hand[1], 1, game.sortedPlayers[0].field[0]);
            Assert.True(game.sortedPlayers[0].field[0].isStealth(game));    //露娜出场后default随从变为潜行状态
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
            THHGame game = TestGameflow.initGameWithoutPlayers(null, new GameOption()
            {
                shuffle = false
            });
            game.createPlayer(0, "玩家0", game.getDefine<Reimu>(), Enumerable.Repeat(game.getDefine<BeerFairy>() as CardDefine, 30));
            game.createPlayer(1, "玩家1", game.getDefine<Reimu>(), Enumerable.Repeat(game.getDefine<BeerFairy>() as CardDefine, 30));
            game.skipTurnWhen(() => game.sortedPlayers[0].gem < 4);

            var s = game.sortedPlayers[0].hand[0];
            game.sortedPlayers[0].cmdUse(game, s, 0);
            Assert.True(game.sortedPlayers[0].field.Contains(s));
            game.sortedPlayers[0].cmdUse(game, game.sortedPlayers[0].hand[0], 0, game.sortedPlayers[0].field[0]);
            Assert.True(game.sortedPlayers[0].hand.Contains(s));
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
            game.skipTurnUntil(() => game.currentPlayer == oppo && oppo.gem >= 1);
            oppo.cmdUse(game, oppo.hand.First());
            oppo.cmdTurnEnd(game);
            you.cmdUse(game, you.hand.getCard<Daiyousei_N>());
            you.cmdAttack(game, you.field[0], oppo.field[0]);
            Assert.AreEqual(1, you.gem);//获得一个仅限本回合使用的法力水晶
            Assert.AreEqual(1, you.maxGem);//最大法力值不变
            game.Dispose();
        }

        [Test]
        public void Cirno2Test()
        {
            TestGameflow.createGame(out var game, out var you, out var oppo,
                new KeyValuePair<int, int>(Cirno_N_YYM.ID, 2)
            );
            game.skipTurnUntil(() => game.currentPlayer == oppo && oppo.gem >= 1);
            oppo.cmdUse(game, oppo.hand.First());
            oppo.cmdTurnEnd(game);
            you.cmdUse(game, you.hand.getCard<Cirno_N_YYM>(), targets: oppo.field[0]);
            you.cmdTurnEnd(game);
            Assert.True(oppo.field[0].canAttack(game) == false);
            oppo.cmdAttack(game, oppo.field[0], you.master);//尝试攻击
            game.Dispose();
        }

        [Test]
        public void Cirno3Test()
        {
            TestGameflow.createGame(out var game, out var you, out var oppo,
                new KeyValuePair<int, int>(Cirno_N_HMX.ID, 2)
            );
            game.skipTurnUntil(() => game.currentPlayer == oppo && oppo.gem >= 3);
            for (int i = 0; i < 3; i++)
                oppo.cmdUse(game, oppo.hand.First());
            oppo.cmdTurnEnd(game);
            you.cmdUse(game, you.hand.getCard<Cirno_N_HMX>(), targets: oppo.field[1]);
            you.cmdTurnEnd(game);
            oppo.cmdAttack(game, oppo.field[0], you.master);//攻击失败
            oppo.cmdAttack(game, oppo.field[1], you.master);//可以攻击
            oppo.cmdAttack(game, oppo.field[2], you.master);//攻击失败
            game.Dispose();
        }

        [Test]
        public void LettyTest()
        {
            TestGameflow.createGame(out var game, out var you, out var oppo,
                new KeyValuePair<int, int>(LettyWhiterock_N.ID, 30));
            game.skipUntilCanUse<LettyWhiterock_N>(you);
            you.cmdUse(game, you.hand.getCard<LettyWhiterock_N>());
            Assert.AreEqual(game.getDefine<LettyWhiterock_N>().cost, you.hand.getCard<LettyWhiterock_N>().getCost(game));
            game.skipUntilHasGem(you, game.getDefine<LettyWhiterock_N>().cost * 2 + 1);
            Assert.AreEqual(game.getDefine<LettyWhiterock_N>().cost + 1, you.hand.getCard<LettyWhiterock_N>().getCost(game));
            you.cmdUse(game, you.hand.getCard<LettyWhiterock_N>());
            Assert.AreEqual(game.getDefine<LettyWhiterock_N>().cost, you.hand.getCard<LettyWhiterock_N>().getCost(game));
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
            game.skipTurnUntil(() => game.currentPlayer == oppo && oppo.gem >= 2);
            oppo.cmdUse(game, oppo.hand.First());
            oppo.cmdTurnEnd(game);
            you.cmdUse(game, you.hand.getCard<Chen_N>());
            Assert.AreEqual(3, you.field[0].getAttack(game));
            Assert.True(you.field[0].isRush(game));
            game.Dispose();
        }

        [Test]
        public void Koakuma_NTest()
        {
            TestGameflow.createGame(out var game, out var you, out var oppo,
                new KeyValuePair<int, int>(Koakuma_N.ID, 2),
                new KeyValuePair<int, int>(DoyouSpear.ID, 2)
            );
            game.skipTurnUntil(() => game.currentPlayer == you && you.gem >= 6);
            you.cmdUse(game, you.hand.getCard<DoyouSpear>());
            Pile.MoveCardEventArg movecardevent = game.triggers.getRecordedEvents().OfType<Pile.MoveCardEventArg>().LastOrDefault(c => c.to == you.grave);
            you.cmdUse(game, you.hand.getCard<Koakuma_N>());
            Assert.True(you.hand.Contains(movecardevent.card));

        }

        [Test]
        public void AliceMargatroid_NTest()
        {
            TestGameflow.createGame(out var game, out var you, out var oppo,
                new KeyValuePair<int, int>(AliceMargatroid_N.ID, 1)
            );
            game.skipUntilCanUse<AliceMargatroid_N>(you);
            you.cmdUse(game, you.hand.getCard<AliceMargatroid_N>());
            you.cmdTurnEnd(game);
            Assert.AreEqual(2, you.field.count);
            Assert.IsInstanceOf<Doll_N>(you.field[1].define);
        }

        [Test]
        public void Lily_WhiteTest()
        {
            TestGameflow.createGame(out var game, out var you, out var oppo,
                new KeyValuePair<int, int>(LilyWhite_N.ID, 5),
                new KeyValuePair<int, int>(Doll_N.ID, 4)
            );
            game.skipTurnUntil(() => game.currentPlayer == you && you.gem >= 6);
            you.cmdUse(game, you.hand.getCard<Doll_N>());
            you.cmdUse(game, you.hand.getCard<Doll_N>());
            you.cmdUse(game, you.hand.getCard<LilyWhite_N>());
            game.skipTurnWhen(() => you.gem < 7);
            var buff = you.field.getCard<Doll_N>().getBuffs()[0];
            Assert.AreEqual(LilyWhite_N.ID, buff.id);
            Assert.True(you.field[1].getCurrentLife(game) == 2);
            Assert.True(you.field[1].getAttack(game) == 2);
        }

        [Test]
        public void KonpakuYoumu_NTest()
        {
            TestGameflow.createGame(out var game, out var you, out var oppo,
                new KeyValuePair<int, int>(KonpakuYoumu_N.ID, 6)
            );
            game.skipTurnUntil(() => game.currentPlayer == oppo && oppo.gem >= 5);
            oppo.cmdUse(game, oppo.hand.First());
            oppo.cmdTurnEnd(game);
            you.cmdUse(game, you.hand.getCard<KonpakuYoumu_N>());
            Assert.True(you.field[0].getMaxAttackTimes(game) == 2);
            you.cmdAttack(game, you.field[0], oppo.field[0]);
            you.cmdAttack(game, you.field[0], oppo.field[0]);
            Assert.False(you.field[0].canAttack(game));
        }

        [Test]
        public void RemiliaScarletTest()
        {
            TestGameflow.createGame(out var game, out var you, out var oppo,
                new KeyValuePair<int, int>(RemiliaScarlet_N.ID, 5),
                new KeyValuePair<int, int>(Doll_N.ID, 4)
            );
            game.skipTurnUntil(() => game.currentPlayer == you && you.gem >= 6);
            var handBeforeUse = you.hand.ToArray();
            you.cmdUse(game, you.hand.getCard<RemiliaScarlet_N>());
            Pile.MoveCardEventArg movecardevent = game.triggers.getRecordedEvents().OfType<Pile.MoveCardEventArg>().LastOrDefault(c => c.to == you.grave);
            Assert.AreEqual(4, game.triggers.getRecordedEvents().OfType<Pile.MoveCardEventArg>().Count(c => c.to == you.grave));//前面爆了一张牌，所以是4
            Card throwcard = movecardevent.card;
            Assert.True(handBeforeUse.Contains(throwcard));
        }

        [Test]
        public void FlandreScarletTest()
        {
            TestGameflow.createGame(out var game, out var you, out var oppo,
                new KeyValuePair<int, int>(FlandreScarlet_N.ID, 8)
            );
            game.skipTurnUntil(() => game.currentPlayer == oppo && oppo.gem >= 6);
            oppo.cmdUse(game, oppo.hand.First());
            Card card = oppo.field[0];
            oppo.cmdTurnEnd(game);
            var handBeforeUse = you.hand.ToArray();
            you.cmdUse(game, you.hand.getCard<FlandreScarlet_N>());
            Pile.MoveCardEventArg movecardevent = game.triggers.getRecordedEvents().OfType<Pile.MoveCardEventArg>().LastOrDefault(c => c.to == you.grave);
            Card throwcard = movecardevent.card;
            Assert.True(handBeforeUse.Contains(throwcard));
            Assert.True(oppo.grave.Contains(card));
        }

        [Test]
        public void IzayoiSakuyaTest()
        {
            TestGameflow.createGame(out var game, out var you, out var oppo,
                new KeyValuePair<int, int>(IzayoiSakuya_N.ID, 5),
                new KeyValuePair<int, int>(Doll_N.ID, 4)
            );
            game.skipTurnUntil(() => game.currentPlayer == you && you.gem >= 6);
            you.cmdUse(game, you.hand.getCard<IzayoiSakuya_N>());
            Card card = game.triggers.getRecordedEvents().OfType<THHPlayer.DrawEventArg>().Last().card;
            Assert.True(you.hand.Contains(card));
            Assert.AreEqual(0, card.getCost(game));
        }

        [Test]
        public void YakumoRanTest()
        {
            TestGameflow.createGame(out var game, out var you, out var oppo,
                new KeyValuePair<int, int>(YakumoRan_N.ID, 8)
            );
            game.skipTurnUntil(() => game.currentPlayer == oppo && oppo.gem >= 7);
            oppo.cmdUse(game, oppo.hand.First());
            oppo.cmdTurnEnd(game);
            you.cmdUse(game, you.hand.getCard<YakumoRan_N>());
            Card card = game.triggers.getRecordedEvents().OfType<THHPlayer.CreateTokenEventArg>().Last().card;
            Assert.IsInstanceOf<Chen_N>(card.define);
            Assert.True(you.field[1].hasKeyword(game, KEYWORD.RUSH));
            var buff = you.field[1].getBuffs()[0];
            Assert.AreEqual(YakumoRan_N.ID, buff.id);
        }

        [Test]
        public void SaigyoujiYuyukoTest()
        {
            TestGameflow.createGame(out var game, out var you, out var oppo,
                new KeyValuePair<int, int>(SaigyoujiYuyuko_N.ID, 9)
            );
            game.skipTurnUntil(() => game.currentPlayer == oppo && oppo.gem >= 8);
            oppo.cmdUse(game, oppo.hand.First());
            Card card = oppo.field[0];
            oppo.cmdTurnEnd(game);
            you.cmdUse(game, you.hand.getCard<SaigyoujiYuyuko_N>(), targets: card);
            Assert.True(oppo.grave.Contains(card));
        }

        [Test]
        public void YukariYakumoTest()
        {
            TestGameflow.createGame(out var game, out var you, out var oppo,
                new KeyValuePair<int, int>(YakumoYukari_N.ID, 1)
            );
            game.skipUntilCanUse<YakumoYukari_N>(you);
            you.cmdUse(game, you.hand.getCard<DefaultServant>());
            you.cmdTurnEnd(game);
            for (int i = 0; i < 7; i++)
            {
                oppo.cmdUse(game, oppo.hand[0]);
            }
            oppo.cmdTurnEnd(game);
            you.cmdUse(game, you.hand.getCard<YakumoYukari_N>());
            Assert.AreEqual(1, you.field.count);
            Assert.IsInstanceOf<YakumoYukari_N>(you.field[0].define);
            Assert.AreEqual(0, oppo.field.count);
            Assert.AreEqual(10, oppo.hand.count);
            //oppo.cmdUse(game, oppo.hand.First());
            //Card oppocard = oppo.field[0];
            //oppo.cmdTurnEnd(game);
            //you.cmdUse(game, you.hand.getCard<Doll_N>());
            //Card youcard = you.field[0];
            //you.cmdTurnEnd(game);
            //oppo.cmdTurnEnd(game);
            //you.cmdUse(game, you.hand.getCard<YakumoYukari_N>());
            //Assert.True(you.hand.Contains(youcard));
            //Assert.True(oppo.hand.Contains(oppocard));
        }
        [Test]
        public void Rumia_NTest()
        {
            TestGameflow.createGame(out var game, out var you, out var oppo,
                new KeyValuePair<int, int>(Rumia_N.ID, 6)
            );
            game.skipTurnUntil(() => game.currentPlayer == oppo && oppo.gem >= 3);
            oppo.cmdUse(game, oppo.hand.First(), 0);//敌方杂鱼A
            oppo.cmdUse(game, oppo.hand.First(), 1);//敌方杂鱼B
            oppo.cmdTurnEnd(game);
            you.cmdUse(game, you.hand.getCard<Rumia_N>());
            game.skipTurnUntil(() => game.currentPlayer == you && you.gem >= 4);
            Assert.True(you.field.getCard<Rumia_N>().canAttack(game));
            you.cmdAttack(game, you.field.getCard<Rumia_N>(), target: oppo.master);//尝试指脸
            Card target = game.triggers.getRecordedEvents().OfType<THHCard.AttackEventArg>().Last().target;
            game.logger.log("目标是：" + target);//真正的攻击目标
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
            game.skipTurnUntil(() => game.currentPlayer == you && you.gem >= 6);
            you.cmdUse(game, you.hand.First());//从牌库或手牌召唤骚灵乐团...（一起召唤吗？）
            Assert.True(you.field.Any(c => c.define is LunasaPrismriver_N));
            Assert.True(you.field.Any(c => c.define is MerlinPrismriver_N));
            Assert.True(you.field.Any(c => c.define is LyricaPrismriver_N));
        }

        [Test]
        public void xxxTest()
        {
            TestGameflow.createGame(out var game, out var you, out var oppo,
                new KeyValuePair<int, int>(YakumoRan_N.ID, 2),
                new KeyValuePair<int, int>(YakumoYukari_N.ID, 2)
            );
            game.skipTurnUntil(() => game.currentPlayer == you && you.gem >= 1);
            you.setGem(game, 10);
            you.cmdUse(game, you.hand.getCard<YakumoRan_N>());
            Card chen = you.field[1];
            you.setGem(game, 10);
            you.cmdUse(game, you.hand.getCard<YakumoYukari_N>());
            you.setGem(game, 10);
            you.cmdUse(game, chen);
            Assert.True(chen.hasKeyword(game, KEYWORD.RUSH));
        }      
        
        [Test]
        public void MoonedInsect_Wriggle_NTest()
        {
            THHGame game = TestGameflow.initGameWithoutPlayers(null, new GameOption()
            {
                shuffle = false
            });
            THHPlayer oppo = game.createPlayer(0, "玩家0", game.getDefine<TestMaster2>(),
                Enumerable.Repeat(game.getDefine<DefaultServant>() as CardDefine, 30));
            THHPlayer you = game.createPlayer(1, "玩家1", game.getDefine<TestMaster2>(),
                Enumerable.Repeat(game.getDefine<DefaultServant>() as CardDefine, 27).Concat(Enumerable.Repeat(game.getDefine<WriggleNightbug_N>(), 3)));

            game.skipUntilCanUse<WriggleNightbug_N>(you);
            you.cmdUse(game, you.hand.getCard<WriggleNightbug_N>());
            you.cmdTurnEnd(game);
            oppo.cmdTurnEnd(game);
            you.field[0].setDead(game,true);
            game.updateDeath(game);
            
            game.Dispose();
        }

        [Test]
        public void FlowerMix_Mystia_NTest()
        {
            THHGame game = TestGameflow.initGameWithoutPlayers(null, new GameOption()
            {
                shuffle = false
            });
            THHPlayer oppo = game.createPlayer(0, "玩家0", game.getDefine<TestMaster2>(),
                Enumerable.Repeat(game.getDefine<DefaultServant>() as CardDefine, 30));
            THHPlayer you = game.createPlayer(1, "玩家1", game.getDefine<TestMaster2>(),
                Enumerable.Repeat(game.getDefine<DefaultServant>() as CardDefine, 27).Concat(Enumerable.Repeat(game.getDefine<MystiaLorelei_N>(), 3)));

            game.skipUntilCanUse<DefaultServant>(oppo);
            oppo.cmdUse(game, oppo.hand.getCard<DefaultServant>());

            game.skipUntilCanUse<MystiaLorelei_N>(you);
            you.cmdUse(game, you.hand.getCard<MystiaLorelei_N>(), targets: oppo.field[0]);
           
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
            you.cmdUse(game, you.hand.getCard<ReisenUdongeinInaba_N>());

            game.Dispose();
        }

        [Test]
        public void TewiInaba_NTest()
        {
            TestGameflow.createGame(out var game, out var you, out var oppo,
                new KeyValuePair<int, int>(TewiInaba_N.ID, 2)

            );
            game.skipUntilCanUse<TewiInaba_N>(you);
            you.cmdUse(game, you.hand.getCard<TewiInaba_N>());

            DiscoverRequest discoverRequest = game.answers.getRequest<DiscoverRequest>(you.id);
            int cardId = discoverRequest.cardIdArray[0];
            Card card = you.deck.getCard(cardId);
            you.cmdDiscover(game, cardId);
            you.draw(game);
            Assert.True(you.hand.Contains(card));
            game.Dispose();
        }
        [Test]
        public void FullMoonNight_Keine_N()
        {
            THHGame game = TestGameflow.initGameWithoutPlayers(null, new GameOption()
            {
                shuffle = false
            });
            THHPlayer oppo = game.createPlayer(0, "玩家0", game.getDefine<TestMaster2>(),
                Enumerable.Repeat(game.getDefine<DefaultServant>() as CardDefine, 30));
            THHPlayer you = game.createPlayer(1, "玩家1", game.getDefine<TestMaster2>(),
                Enumerable.Repeat(game.getDefine<LuckyCoin>() as CardDefine, 15).Concat(Enumerable.Repeat(game.getDefine<KamishirasawaKeine_Human_N>(), 1)));

            game.skipUntilHasGem(you, 1);
            game.skipUntilHasGem(you, 2);
            game.skipUntilHasGem(you, 3);
            game.skipUntilHasGem(you, 4);

            game.Dispose();
        }

        [Test]
        public void Mokou_NTest()
        {
            THHGame game = TestGameflow.initGameWithoutPlayers(null, new GameOption()
            {
                shuffle = false
            });
            THHPlayer oppo = game.createPlayer(0, "玩家0", game.getDefine<TestMaster2>(),
                Enumerable.Repeat(game.getDefine<DefaultServant>() as CardDefine, 30));
            THHPlayer you = game.createPlayer(1, "玩家1", game.getDefine<TestMaster2>(),
                Enumerable.Repeat(game.getDefine<DefaultServant>() as CardDefine, 29).Concat(Enumerable.Repeat(game.getDefine<HuziwaraNoMokou_N>(), 1)));

            for(int i=0; i<3; i++)
            {
                game.skipUntilCanUse<DefaultServant>(oppo);
                oppo.cmdUse(game, oppo.hand.getCard<DefaultServant>());
            }

            for (int i = 0; i < 3; i++)
            {
                game.skipUntilCanUse<DefaultServant>(you);
                you.cmdUse(game, you.hand.getCard<DefaultServant>());
            }

            game.skipUntilCanUse<HuziwaraNoMokou_N>(you);
            you.cmdUse(game, you.hand.getCard<HuziwaraNoMokou_N>());
            Assert.True(you.field[0].canAttack(game));

            you.field[0].setDead(game, true);
            game.updateDeath(game);

            game.Dispose();
        }

        [Test]
        public void Eirin_NTest()
        {
            TestGameflow.createGame(out var game, out var you, out var oppo,
                new KeyValuePair<int, int>(YagokoroEirin_N.ID, 2)
            );
            game.skipUntilCanUse<YagokoroEirin_N>(you);
            you.cmdUse(game, you.hand.getCard<YagokoroEirin_N>(),targets: oppo.master);
            DiscoverRequest discoverRequest = game.answers.getRequest<DiscoverRequest>(you.id);
            int cardId = discoverRequest.cardIdArray[1];
           // Card card = you.deck.getCard(cardId);
            you.cmdDiscover(game, cardId);

            game.skipUntilCanUse<YagokoroEirin_N>(you);
            you.cmdUse(game, you.hand.getCard<YagokoroEirin_N>(),targets: oppo.master);
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
            //清空手牌
            game.skipUntilHasGem(you, 10);
            you.cmdUse(game, you.hand.getCard<DefaultServant>());
            you.cmdUse(game, you.hand.getCard<DefaultServant>());
            you.cmdUse(game, you.hand.getCard<DefaultServant>());
            you.cmdUse(game, you.hand.getCard<DefaultServant>());
            you.cmdUse(game, you.hand.getCard<DefaultServant>());
            you.cmdTurnEnd(game);

            oppo.cmdUse(game, oppo.hand.getCard<DefaultServant>());
            oppo.cmdUse(game, oppo.hand.getCard<DefaultServant>());
            oppo.cmdUse(game, oppo.hand.getCard<DefaultServant>());
            oppo.cmdUse(game, oppo.hand.getCard<DefaultServant>());
            oppo.cmdUse(game, oppo.hand.getCard<DefaultServant>());
            oppo.cmdTurnEnd(game);

            game.skipUntilCanUse<HouraisanKaguya_N>(you);
            you.cmdUse(game, you.hand.getCard<HouraisanKaguya_N>());
            DiscoverRequest discoverRequest = game.answers.getRequest<DiscoverRequest>(you.id);
            int cardId = discoverRequest.cardIdArray[1];
            you.cmdDiscover(game, cardId);



            game.skipUntilCanUse<HouraisanKaguya_N>(you);
            you.cmdUse(game, you.hand.getCard<HouraisanKaguya_N>());
            DiscoverRequest discoverRequest2 = game.answers.getRequest<DiscoverRequest>(you.id);
            int cardId2 = discoverRequest2.cardIdArray[0];
            you.cmdDiscover(game, cardId2);

            you.cmdTurnEnd(game);
            oppo.cmdTurnEnd(game);

            you.cmdTurnEnd(game);
            oppo.cmdTurnEnd(game);

            you.cmdTurnEnd(game);
            oppo.cmdTurnEnd(game);
            game.Dispose();
        }
    }
}
