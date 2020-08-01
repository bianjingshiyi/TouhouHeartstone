using System.Collections;
using NUnit.Framework;
using UnityEngine.TestTools;
using System.Linq;
using TouhouHeartstone;
using TouhouCardEngine;
using TouhouHeartstone.Builtin;
using System.Collections.Generic;
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
            game.skipTurnUntil(() => game.currentPlayer == you && you.gem >= game.getCardDefine<MissingSpecter>().cost);
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
        //TODO:炎之妖精之后的中立卡组测试
        [Test]
        public void flameFairyTest()
        {
            THHGame game = TestGameflow.initGameWithoutPlayers(null, new GameOption()
            {
                shuffle = false
            });//首先，上来先新建一个不带玩家的游戏
            game.createPlayer(0, "玩家0", game.getCardDefine<Reimu>(), Enumerable.Repeat(game.getCardDefine<FlameFairy>() as CardDefine, 30));//创建两个玩家，给他们设置初始英雄，套牌
            game.createPlayer(1, "玩家1", game.getCardDefine<Reimu>(), Enumerable.Repeat(game.getCardDefine<FlameFairy>() as CardDefine, 30));
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
            game.createPlayer(0, "玩家0", game.getCardDefine<Reimu>(), Enumerable.Repeat(game.getCardDefine<SunnyMilk>() as CardDefine, 30));
            game.createPlayer(1, "玩家1", game.getCardDefine<Reimu>(), Enumerable.Repeat(game.getCardDefine<SunnyMilk>() as CardDefine, 30));

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
            game.createPlayer(0, "玩家0", game.getCardDefine<Reimu>(), Enumerable.Repeat(game.getCardDefine<LunaChild>() as CardDefine, 30));
            game.createPlayer(1, "玩家1", game.getCardDefine<Reimu>(), Enumerable.Repeat(game.getCardDefine<LunaChild>() as CardDefine, 29)
            .Concat(Enumerable.Repeat(game.getCardDefine<DefaultServant>(), 1)));
            game.skipTurnWhen(() => game.sortedPlayers[0].gem < 3);

            game.sortedPlayers[0].cmdUse(game, game.sortedPlayers[0].hand[0], 0);
            Assert.False(game.sortedPlayers[0].field[0].isStealth(game));   //default随从一开始没有潜行
            game.sortedPlayers[0].cmdUse(game, game.sortedPlayers[0].hand[1], 1, game.sortedPlayers[0].field[0]);
            Assert.True(game.sortedPlayers[0].field[0].isStealth(game));    //露娜出场后default随从变为潜行状态
        }

        [Test]
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
        }

        [Test]
        public void BeerFairyTest()
        {
            THHGame game = TestGameflow.initGameWithoutPlayers(null, new GameOption()
            {
                shuffle = false
            });
            game.createPlayer(0, "玩家0", game.getCardDefine<Reimu>(), Enumerable.Repeat(game.getCardDefine<BeerFairy>() as CardDefine, 30));
            game.createPlayer(1, "玩家1", game.getCardDefine<Reimu>(), Enumerable.Repeat(game.getCardDefine<BeerFairy>() as CardDefine, 30));
            game.skipTurnWhen(() => game.sortedPlayers[0].gem < 4);

            var s = game.sortedPlayers[0].hand[0];
            game.sortedPlayers[0].cmdUse(game, s, 0);
            Assert.True(game.sortedPlayers[0].field.Contains(s));
            game.sortedPlayers[0].cmdUse(game, game.sortedPlayers[0].hand[0], 0, game.sortedPlayers[0].field[0]);
            Assert.True(game.sortedPlayers[0].hand.Contains(s));
            //TODO:落叶，这里就只有两个小熊猫相互测试，还不够。
            //还要测试手牌满了的情况下，小熊猫被挤死了
        }

        [Test]
        public void Daiyousei2Test()
        {
            TestGameflow.createGame(out var game, out var you, out var oppo,
                new KeyValuePair<int, int>(Daiyousei2.ID, 2)
            );
            game.skipTurnUntil(() => game.currentPlayer == oppo && oppo.gem >= 1);
            oppo.cmdUse(game, oppo.hand.First());
            oppo.cmdTurnEnd(game);
            you.cmdUse(game, you.hand.getCard<Daiyousei2>());
            you.cmdAttack(game, you.field[0], oppo.field[0]);
            Assert.AreEqual(1, you.gem);//获得一个仅限本回合使用的法力水晶
            Assert.AreEqual(1, you.maxGem);//最大法力值不变
            game.Dispose();
        }

        [Test]
        public void Cirno2Test()
        {
            TestGameflow.createGame(out var game, out var you, out var oppo,
                new KeyValuePair<int, int>(Cirno2.ID, 2)
            );
            game.skipTurnUntil(() => game.currentPlayer == oppo && oppo.gem >= 1);
            oppo.cmdUse(game, oppo.hand.First());
            oppo.cmdTurnEnd(game);
            you.cmdUse(game, you.hand.getCard<Cirno2>(),targets: oppo.field[0]);
            you.cmdTurnEnd(game);
            Assert.True(oppo.field[0].canAttack(game)==false);
            oppo.cmdAttack(game, oppo.field[0], you.master);//尝试攻击
            game.Dispose();
        }

        [Test]
        public void Cirno3Test()
        {
            TestGameflow.createGame(out var game, out var you, out var oppo,
                new KeyValuePair<int, int>(Cirno3.ID, 2)
            );
            game.skipTurnUntil(() => game.currentPlayer == oppo && oppo.gem >= 3);
            for(int i=0;i<3;i++)
                oppo.cmdUse(game, oppo.hand.First());
            oppo.cmdTurnEnd(game);
            you.cmdUse(game, you.hand.getCard<Cirno3>(), targets: oppo.field[1]);
            you.cmdTurnEnd(game);
            oppo.cmdAttack(game, oppo.field[0], you.master);//攻击失败
            oppo.cmdAttack(game, oppo.field[1], you.master);//可以攻击
            oppo.cmdAttack(game, oppo.field[2], you.master);//攻击失败
            game.Dispose();
        }

        [Test]
        public void LettyTest()
        {
            THHGame game = TestGameflow.initGameWithoutPlayers(null, new GameOption(){shuffle = false});
            THHPlayer you = game.createPlayer(0, "玩家0", game.getCardDefine<Reimu>(), Enumerable.Repeat(game.getCardDefine<Letty>() as CardDefine, 4)
                .Concat(Enumerable.Repeat(game.getCardDefine<SummerFire>(), 4)));
            THHPlayer oppo = game.createPlayer(1, "玩家1", game.getCardDefine<Reimu>(), Enumerable.Repeat(game.getCardDefine<SummerFire>() as CardDefine, 30));

            game.skipTurnWhen(() => you.gem < 3);

            you.cmdUse(game, you.hand.getCard<Letty>());
            Assert.True(you.hand.Where(c => c.define is SummerFire).All(c => c.getCost(game) == 3));
            you.cmdTurnEnd(game);
            Assert.True(you.hand.Where(c => c.define is SummerFire).All(c => c.getCost(game) == 2));
            Assert.True(oppo.hand.Where(c => c.define is SummerFire).All(c => c.getCost(game) == 3));
            oppo.cmdUse(game, oppo.hand.First(), targets: you.master);
            Assert.True(oppo.hand.Where(c => c.define is SummerFire).All(c => c.getCost(game) == 2));
            oppo.cmdTurnEnd(game);
            you.cmdUse(game, you.hand.getCard<SummerFire>(), targets: you.master);
        }

        [Test]
        public void ChenTest()
        {
            TestGameflow.createGame(out var game, out var you, out var oppo,
                new KeyValuePair<int, int>(Chen.ID, 2)
            );
            game.skipTurnUntil(() => game.currentPlayer == oppo && oppo.gem >= 2);
            oppo.cmdUse(game, oppo.hand.First());
            oppo.cmdTurnEnd(game);
            you.cmdUse(game, you.hand.getCard<Chen>());
            Assert.True(you.field[0].isRush(game)==true);
            you.cmdAttack(game, you.field[0], oppo.field[0]);
            you.cmdTurnEnd(game);
            game.Dispose();
        }

        [Test]
        public void Koakuma2Test()
        {
            TestGameflow.createGame(out var game, out var you, out var oppo,
                new KeyValuePair<int, int>(Koakuma2.ID, 2),
                new KeyValuePair<int, int>(SummerFire.ID, 2),
                new KeyValuePair<int, int>(DoyouSpear.ID, 2)
            );
            game.skipTurnUntil(() => game.currentPlayer == you && you.gem >= 6);
            you.cmdUse(game, you.hand.getCard<SummerFire>());
            you.cmdUse(game, you.hand.getCard<DoyouSpear>());
            you.cmdUse(game, you.hand.getCard<Koakuma2>());
        }

        [Test]
        public void Alice_MargatroidTest()
        {
            TestGameflow.createGame(out var game, out var you, out var oppo,
                new KeyValuePair<int, int>(Alice_Margatroid.ID, 5)
            );
            game.skipTurnUntil(() => game.currentPlayer == you && you.gem >= 3);
            you.cmdUse(game, you.hand.getCard<Alice_Margatroid>());
            game.skipTurnWhen(() => you.gem < 5);
        }

        [Test]
        public void Lily_WhiteTest()
        {
            TestGameflow.createGame(out var game, out var you, out var oppo,
                new KeyValuePair<int, int>(Lily_White.ID, 5),
                new KeyValuePair<int, int>(Dolls.ID, 4)
            );
            game.skipTurnUntil(() => game.currentPlayer == you && you.gem >= 6);
            you.cmdUse(game, you.hand.getCard<Dolls>());
            you.cmdUse(game, you.hand.getCard<Dolls>());
            you.cmdUse(game, you.hand.getCard<Lily_White>());
            game.skipTurnWhen(() => you.gem < 9);
        }

        [Test]
        public void YoumuTest()
        {
            TestGameflow.createGame(out var game, out var you, out var oppo,
                new KeyValuePair<int, int>(Youmu.ID, 6)
            );
            game.skipTurnUntil(() => game.currentPlayer == oppo && oppo.gem >= 5);
            oppo.cmdUse(game, oppo.hand.First());
            oppo.cmdTurnEnd(game);
            you.cmdUse(game, you.hand.getCard<Youmu>());
            you.cmdAttack(game, you.field[0], oppo.field[0]);
            you.cmdAttack(game, you.field[0], oppo.field[0]);
            you.cmdAttack(game, you.field[0], oppo.field[0]);
        }

        [Test]
        public void RemiliaScarletTest()
        {
            TestGameflow.createGame(out var game, out var you, out var oppo,
                new KeyValuePair<int, int>(RemiliaScarlet.ID, 5),
                new KeyValuePair<int, int>(Dolls.ID, 4)
            );
            game.skipTurnUntil(() => game.currentPlayer == you && you.gem >= 6);
            you.cmdUse(game, you.hand.getCard<RemiliaScarlet>());
        }

        [Test]
        public void FlandreScarletTest()
        {
            TestGameflow.createGame(out var game, out var you, out var oppo,
                new KeyValuePair<int, int>(FlandreScarlet.ID, 8)
            );
            game.skipTurnUntil(() => game.currentPlayer == oppo && oppo.gem >= 6);
            oppo.cmdUse(game, oppo.hand.First());
            oppo.cmdTurnEnd(game);
            you.cmdUse(game, you.hand.getCard<FlandreScarlet>());
        }

        [Test]
        public void IzayoiSakuyaTest()
        {
            TestGameflow.createGame(out var game, out var you, out var oppo,
                new KeyValuePair<int, int>(IzayoiSakuya.ID, 5),
                new KeyValuePair<int, int>(Dolls.ID, 4)
            );
            game.skipTurnUntil(() => game.currentPlayer == you && you.gem >= 6);
            you.cmdUse(game, you.hand.getCard<IzayoiSakuya>());
        }

        [Test]
        public void YakumoRanTest()
        {
            TestGameflow.createGame(out var game, out var you, out var oppo,
                new KeyValuePair<int, int>(YakumoRan.ID, 8)
            );
            game.skipTurnUntil(() => game.currentPlayer == oppo && oppo.gem >= 7);
            oppo.cmdUse(game, oppo.hand.First());
            oppo.cmdTurnEnd(game);
            you.cmdUse(game, you.hand.getCard<YakumoRan>());
        }

        [Test]
        public void SaigyoujiYuyukoTest()
        {
            TestGameflow.createGame(out var game, out var you, out var oppo,
                new KeyValuePair<int, int>(SaigyoujiYuyuko.ID, 9)
            );
            game.skipTurnUntil(() => game.currentPlayer == oppo && oppo.gem >= 8);
            oppo.cmdUse(game, oppo.hand.First());
            oppo.cmdTurnEnd(game);
            you.cmdUse(game, you.hand.getCard<SaigyoujiYuyuko>(), targets:oppo.field[0]);

        }

        [Test]
        public void YukariYakumoTest()
        {
            TestGameflow.createGame(out var game, out var you, out var oppo,
                new KeyValuePair<int, int>(YukariYakumo.ID, 6),
                new KeyValuePair<int, int>(Dolls.ID, 5)
            );
            game.skipTurnUntil(() => game.currentPlayer == oppo && oppo.gem >= 9);
            oppo.cmdUse(game, oppo.hand.First());
            oppo.cmdTurnEnd(game);
            you.cmdUse(game, you.hand.getCard<Dolls>());
            you.cmdTurnEnd(game);
            oppo.cmdTurnEnd(game);
            you.cmdUse(game, you.hand.getCard<YukariYakumo>());
        }

        [Test]
        public void FakecardTest()
        {
            TestGameflow.createGame(out var game, out var you, out var oppo,
                new KeyValuePair<int, int>(Fakecard.ID, 2),
                new KeyValuePair<int, int>(Chen.ID, 2)
            );
            game.skipTurnUntil(() => game.currentPlayer == oppo && oppo.gem >= 3);
            oppo.cmdUse(game, oppo.hand.First());
            oppo.cmdTurnEnd(game);
            you.cmdUse(game, you.hand.getCard<Chen>());
            you.cmdAttack(game, you.field.getCard<Chen>(), oppo.field[0]);
            you.field.getCard<Chen>().damage(game,null,10);
            you.field.getCard<Chen>().die(game);
            you.cmdUse(game, you.hand.getCard<Fakecard>());
            you.field.getCard<Fakecard>().damage(game, null, 10);
            you.field.getCard<Fakecard>().die(game);
            /*game.logger.log("我方场上：" + you.field[0] + "控制权：" + you.field[0].getOwner());
            game.logger.log("敌方场上：" + oppo.field[0]+ "控制权：" + oppo.field[0].getOwner());*/
        }
    }
}
