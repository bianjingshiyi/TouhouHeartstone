using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using System.Linq;
using System;
using TouhouHeartstone;
using TouhouHeartstone.Builtin;
using TouhouCardEngine;
using TouhouCardEngine.Interfaces;

namespace Tests
{
    public class CardSystemTests
    {
        [Test]
        public void buffTest()
        {
            THHGame game = TestGameflow.initStandardGame(null, new int[] { 0, 1 },
            Enumerable.Repeat(new TestMaster(), 2).ToArray(),
            Enumerable.Repeat(Enumerable.Repeat(new TestServant_Buff(), 30).ToArray(), 2).ToArray(),
            new GameOption() { });
            game.run();
            game.sortedPlayers[0].cmdInitReplace(game);
            game.sortedPlayers[1].cmdInitReplace(game);

            game.sortedPlayers[0].cmdUse(game, game.sortedPlayers[0].hand[0], 0);

            Assert.AreEqual(2, game.sortedPlayers[0].field[0].getAttack(game));
            Assert.AreEqual(2, game.sortedPlayers[0].field[0].getLife(game));
            Assert.AreEqual(2, game.sortedPlayers[0].field[0].getCurrentLife(game));
        }
        /// <summary>
        /// 加攻Buff测试
        /// </summary>
        [Test]
        public void addATKBuffTest()
        {
            Card card = new Card(0);
            TestBuff buff = new TestBuff(new AttackModifier(2));
            card.addBuff(null, buff);
            buff = new TestBuff(new AttackModifier(2));
            card.addBuff(null, buff);

            Assert.AreEqual(4, card.getAttack(null));
        }
        /// <summary>
        /// 加攻Buff移除测试
        /// </summary>
        [Test]
        public void addATKBuffRevertTest()
        {
            Card card = new Card(0);
            TestBuff[] buffs = new TestBuff[2];
            for (int i = 0; i < buffs.Length; i++)
            {
                buffs[i] = new TestBuff(new AttackModifier(2));
                card.addBuff(null, buffs[i]);
            }

            Assert.AreEqual(4, card.getAttack(null));
        }
        /// <summary>
        /// 加攻和设置攻击力Buff测试
        /// </summary>
        [Test]
        public void addAndSetATKBuffTest()
        {
            Card card = new Card(0);
            TestBuff[] buffs = new TestBuff[3];
            buffs[0] = new TestBuff(new AttackModifier(2));
            card.addBuff(null, buffs[0]);
            buffs[1] = new TestBuff(new AttackModifier(0, true));
            card.addBuff(null, buffs[1]);
            buffs[2] = new TestBuff(new AttackModifier(2));
            card.addBuff(null, buffs[2]);

            Assert.AreEqual(2, card.getAttack(null));
        }
        /// <summary>
        /// 加攻和设置攻击力Buff移除测试
        /// </summary>
        [Test]
        public void addAndSetATKBuffRevertTest()
        {
            Card card = new Card(0);
            TestBuff[] buffs = new TestBuff[3];
            buffs[0] = new TestBuff(new AttackModifier(2));
            card.addBuff(null, buffs[0]);
            buffs[1] = new TestBuff(new AttackModifier(0, true));
            card.addBuff(null, buffs[1]);
            buffs[2] = new TestBuff(new AttackModifier(2));
            card.addBuff(null, buffs[2]);

            Assert.AreEqual(2, card.getAttack(null));

            card.removeBuff(null, buffs[1]);

            Assert.AreEqual(4, card.getAttack(null));
        }
        /// <summary>
        /// 添加减攻Buff直到随从攻击力会被减为负数，获取随从攻击力为0，但是移除Buff的时候可以恢复成正确的数值。
        /// </summary>
        [Test]
        public void subAttackBuffApplyAndRevertTest()
        {
            Card card = new Card(0);
            card.setAttack(2);
            TestBuff[] buffs = new TestBuff[3];
            for (int i = 0; i < buffs.Length; i++)
            {
                buffs[i] = new TestBuff(new AttackModifier(-2));
                card.addBuff(null, buffs[i]);
            }
            Assert.AreEqual(0, card.getAttack(null));

            for (int i = 0; i < buffs.Length; i++)
            {
                card.removeBuff(null, buffs[i]);
            }
            Assert.AreEqual(2, card.getAttack(null));
        }
        /// <summary>
        /// 领军生平测试
        /// </summary>
        [Test]
        public void haloAndSetBuffTest()
        {
            THHGame game = new THHGame()
            {
                logger = new UnityLogger()
            };
            Card card = new Card(0);
            card.setLife(1);
            card.setCurrentLife(1);
            TestBuff haloBuff = new TestBuff(new LifeModifier(1));
            card.addBuff(game, haloBuff);
            TestBuff setBuff = new TestBuff(new LifeModifier(1, true));
            card.addBuff(game, setBuff);
            //刷新Buff
            card.removeBuff(game, haloBuff);
            card.addBuff(game, haloBuff);

            Assert.AreEqual(2, card.getCurrentLife(game));
            Assert.AreEqual(2, card.getLife(game));
        }
        /// <summary>
        /// 在移除Buff的时候，如果随从的生命值是满的，那么当前生命值变更为移除Buff之后的生命值。
        /// </summary>
        [Test]
        public void removeBuffWhenInjureTest()
        {
            THHGame game = new THHGame()
            {
                logger = new UnityLogger()
            };
            Card card = new Card(0);
            card.setLife(5);
            card.setCurrentLife(5);
            TestBuff buff = new TestBuff(new LifeModifier(3, true));
            card.addBuff(game, buff);
            Assert.AreEqual(3, card.getLife(game));
            Assert.AreEqual(3, card.getCurrentLife(game));
            card.removeBuff(game, buff);
            Assert.AreEqual(5, card.getLife(game));
            Assert.AreEqual(5, card.getCurrentLife(game));
            card.addBuff(game, buff);
            Assert.AreEqual(3, card.getLife(game));
            Assert.AreEqual(3, card.getCurrentLife(game));
            //不满血
            card.setCurrentLife(1);
            card.removeBuff(game, buff);
            Assert.AreEqual(5, card.getLife(game));
            Assert.AreEqual(1, card.getCurrentLife(game));
        }
        [Test]
        public void mountainGaintTest()
        {
            THHGame game = TestGameflow.initGameWithoutPlayers(null, new GameOption()
            {
                shuffle = false
            });
            game.createPlayer(0, "玩家0", game.getCardDefine<TestMaster>(), Enumerable.Repeat(game.getCardDefine<MountainGaint>() as CardDefine, 30));
            game.createPlayer(1, "玩家1", game.getCardDefine<TestMaster>(), Enumerable.Repeat(game.getCardDefine<MountainGaint>() as CardDefine, 30));

            game.skipTurnWhen(() => game.sortedPlayers[0].hand.count < 10);

            Assert.AreEqual(3, game.sortedPlayers[0].hand[0].getCost(game));
            int gemNow = game.sortedPlayers[0].gem;
            game.sortedPlayers[0].cmdUse(game, game.sortedPlayers[0].hand[0], 0);
            Assert.AreEqual(gemNow - 3, game.sortedPlayers[0].gem);
        }
        class TestBuff : Buff
        {
            public override int id { get; } = 0;
            public override PropModifier[] modifiers { get; } = null;
            public override IPassiveEffect[] effects { get; }
            public TestBuff(params PropModifier[] modifiers)
            {
                this.modifiers = modifiers;
            }
            TestBuff(TestBuff origin)
            {
                id = origin.id;
                modifiers = origin.modifiers;
            }
            public override Buff clone()
            {
                return new TestBuff(this);
            }
        }
        [UnityTest]
        public IEnumerator fireBallTest()
        {
            THHGame game = TestGameflow.initGameWithoutPlayers(null, new GameOption()
            {
                shuffle = false
            });
            game.createPlayer(0, "玩家0", game.getCardDefine<TestMaster>(), Enumerable.Repeat(game.getCardDefine<FireBall>() as CardDefine, 30));
            game.createPlayer(1, "玩家1", game.getCardDefine<TestMaster>(), Enumerable.Repeat(game.getCardDefine<FireBall>() as CardDefine, 30));

            game.skipTurnUntil(() => game.players[0].hand.count > 0);
            Assert.False(game.players[0].hand[0].isUsable(game, game.players[0], out _));
            game.skipTurnUntil(() => game.players[0].gem == 4);
            Assert.True(game.players[0].hand[0].isUsable(game, game.players[0], out _));
            Assert.True(game.players[0].hand[0].isValidTarget(game, game.players[1].master));
            game.players[0].cmdUse(game, game.players[0].hand[0], 0, game.players[1].master);
            yield return new WaitUntil(() => game.triggers.getRecordedEvents().Any(e => e is THHCard.DamageEventArg));
            Assert.NotNull(game.triggers.getRecordedEvents().OfType<THHCard.DamageEventArg>().Last());
        }
        [UnityTest]
        public IEnumerator sorcererApprenticeTest()
        {
            THHGame game = TestGameflow.initGameWithoutPlayers(null, new GameOption()
            {
                shuffle = false
            });
            game.createPlayer(0, "玩家0", game.getCardDefine<TestMaster>(),
                Enumerable.Repeat(game.getCardDefine<FireBall>(), 29).Cast<CardDefine>()
                .Concat(Enumerable.Repeat(game.getCardDefine<SorcererApprentice>(), 1).Cast<CardDefine>()));
            game.createPlayer(1, "玩家1", game.getCardDefine<TestMaster>(),
                Enumerable.Repeat(game.getCardDefine<FireBall>(), 29).Cast<CardDefine>()
                .Concat(Enumerable.Repeat(game.getCardDefine<SorcererApprentice>(), 1).Cast<CardDefine>()));

            game.skipTurnUntil(() =>
                game.currentPlayer == game.players[0] &&
                game.players[0].gem >= game.getCardDefine<SorcererApprentice>().cost &&
                game.players[0].hand.Any(c => c.define is SorcererApprentice));
            Assert.True(game.players[0].hand.Where(c => c.define is FireBall).All(c => c.getCost(game) == 4));//火球术全是4费
            var task = game.players[0].cmdUse(game, game.players[0].hand.getCard<SorcererApprentice>());//使用哀绿
            yield return TestHelper.waitTask(task);
            Assert.True(game.players[0].field.Any(c => c.define is SorcererApprentice));
            Assert.True(game.players[0].hand.Where(c => c.define is FireBall).All(c => c.getCost(game) == 3));//火球术全是3费
            task = game.players[0].draw(game);//抽一张火球
            yield return TestHelper.waitTask(task);
            Card card = game.players[0].hand.right;
            Assert.AreEqual(3, card.getCost(game));
            task = card.pile.moveTo(game, card, game.players[0].grave);//把火球送入墓地
            yield return TestHelper.waitTask(task);
            Assert.AreEqual(4, card.getCost(game));
            yield return game.players[0].field[0].die(game).wait();//杀死哀绿
            Assert.True(game.players[0].hand.getCards<FireBall>().All(c => c.getCost(game) == 4));
            yield return game.players[0].draw(game).wait();//再抽一张火球术
            Assert.True(game.players[0].hand.getCards<FireBall>().All(c => c.getCost(game) == 4));
        }
        [Test]
        public void idTest()
        {
            Assert.AreEqual(TestMaster.ID, CardCategory.getCharacterID(TestSkill.ID));
        }
        [UnityTest]
        public IEnumerator discoverTest()
        {
            TestGameflow.createGame(out var game, out var you, out var oppo);
            var cards = you.deck.randomTake(game, 3);
            var task = you.discover(game, cards);
            yield return new WaitForSeconds(.5f);
            you.cmdDiscover(game, cards.First().id);
            Assert.True(task.IsCompleted);
            Assert.AreEqual(cards.First(), task.Result);
        }
        [UnityTest]
        public IEnumerator discoverTest_Cancel()
        {
            TestGameflow.createGame(out var game, out var you, out var oppo);
            var cards = you.deck.randomTake(game, 3);
            var task = you.discover(game, cards);
            yield return new WaitForSeconds(.5f);
            game.answers.cancelAll();
            yield return task.wait();
            Assert.True(cards.Contains(task.Result));
        }
        [UnityTest]
        public IEnumerator discoverTest_TurnEnd()
        {
            TestGameflow.createGame(out var game, out var you, out var _, new GameOption()
            {
                timeoutForTurn = 1
            });
            game.skipTurnUntil(() => game.currentPlayer == you);
            var cards = you.hand.randomTake(game, 3);
            var task = you.discover(game, cards);
            Assert.NotNull(game.answers.getRequest<DiscoverRequest>(you.id));
            THHGame.TurnEndEventArg turnEnd = null;
            game.triggers.onEventBefore += obj =>
            {
                if (obj is THHGame.TurnEndEventArg)
                    turnEnd = obj as THHGame.TurnEndEventArg;
            };
            DiscoverResponse discoverResponse = null;
            game.answers.onResponse += response =>
            {
                if (response is DiscoverResponse)
                    discoverResponse = response as DiscoverResponse;
            };
            yield return new WaitForSeconds(1.1f);
            Assert.NotNull(turnEnd);
            Assert.NotNull(discoverResponse);
            Assert.AreNotEqual(you, game.currentPlayer);
            Assert.Null(game.answers.getRequest<DiscoverRequest>(you.id));
            Assert.True(task.IsCompleted);
            Assert.True(cards.Contains(task.Result));
        }
    }
    static class TestExtension
    {
        public static void skipTurnWhen(this THHGame game, Func<bool> condition)
        {
            if (!game.isRunning)
            {
                game.run();
            }
            if (!game.triggers.getRecordedEvents().Any(e => e is THHGame.StartEventArg))
            {
                game.sortedPlayers[0].cmdInitReplace(game);
                game.sortedPlayers[1].cmdInitReplace(game);
            }
            while (condition())
            {
                if (game.currentPlayer == game.sortedPlayers[0])
                    game.sortedPlayers[0].cmdTurnEnd(game);
                if (!condition())
                    return;
                if (game.currentPlayer == game.sortedPlayers[1])
                    game.sortedPlayers[1].cmdTurnEnd(game);
            }
        }
        public static void skipTurnUntil(this THHGame game, Func<bool> condition)
        {
            if (!game.isRunning)
            {
                game.run();
                game.sortedPlayers[0].cmdInitReplace(game);
                game.sortedPlayers[1].cmdInitReplace(game);
            }
            int count = 0;
            while (!condition())
            {
                if (game.currentPlayer == game.sortedPlayers[0])
                    game.sortedPlayers[0].cmdTurnEnd(game);
                if (condition())
                    return;
                if (game.currentPlayer == game.sortedPlayers[1])
                    game.sortedPlayers[1].cmdTurnEnd(game);
                count++;
                if (count > 1000)
                    throw new StackOverflowException();
            }
        }
    }
}