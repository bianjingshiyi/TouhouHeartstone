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
    }
}
