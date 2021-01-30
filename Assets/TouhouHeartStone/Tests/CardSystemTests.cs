using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using System.Linq;
using TouhouHeartstone;
using TouhouHeartstone.Builtin;
using TouhouCardEngine;
using TouhouCardEngine.Interfaces;
using System.Collections.Generic;
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
            card.setAttack(null, 2);
            TestBuff[] buffs = new TestBuff[3];
            for (int i = 0; i < buffs.Length; i++)
            {
                buffs[i] = new TestBuff(new AttackModifier(-2));
                _ = card.addBuff(null, buffs[i]);
            }
            Assert.AreEqual(0, card.getAttack(null));

            for (int i = 0; i < buffs.Length; i++)
            {
                _ = card.removeBuff(null, buffs[i]);
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
            card.setLife(game, 1);
            card.setCurrentLife(game, 1);
            TestBuff haloBuff = new TestBuff(new LifeModifier(1));
            _ = card.addBuff(game, haloBuff);
            TestBuff setBuff = new TestBuff(new LifeModifier(1, true));
            _ = card.addBuff(game, setBuff);
            //刷新Buff
            _ = card.removeBuff(game, haloBuff);
            _ = card.addBuff(game, haloBuff);

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
            card.setLife(game, 5);
            card.setCurrentLife(game, 5);
            TestBuff buff = new TestBuff(new LifeModifier(3, true));
            _ = card.addBuff(game, buff);
            Assert.AreEqual(3, card.getLife(game));
            Assert.AreEqual(3, card.getCurrentLife(game));
            _ = card.removeBuff(game, buff);
            Assert.AreEqual(5, card.getLife(game));
            Assert.AreEqual(5, card.getCurrentLife(game));
            _ = card.addBuff(game, buff);
            Assert.AreEqual(3, card.getLife(game));
            Assert.AreEqual(3, card.getCurrentLife(game));
            //不满血
            card.setCurrentLife(game, 1);
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
            game.createPlayer(0, "玩家0", game.getDefine<TestMaster>(), Enumerable.Repeat(game.getDefine<MountainGaint>() as CardDefine, 30));
            game.createPlayer(1, "玩家1", game.getDefine<TestMaster>(), Enumerable.Repeat(game.getDefine<MountainGaint>() as CardDefine, 30));

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

            public override int instanceID { get; set; } = 0;

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
            game.createPlayer(0, "玩家0", game.getDefine<TestMaster>(), Enumerable.Repeat(game.getDefine<FireBall>() as CardDefine, 30));
            game.createPlayer(1, "玩家1", game.getDefine<TestMaster>(), Enumerable.Repeat(game.getDefine<FireBall>() as CardDefine, 30));

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
            game.createPlayer(0, "玩家0", game.getDefine<TestMaster>(),
                Enumerable.Repeat(game.getDefine<FireBall>(), 29).Cast<CardDefine>()
                .Concat(Enumerable.Repeat(game.getDefine<SorcererApprentice>(), 1).Cast<CardDefine>()));
            game.createPlayer(1, "玩家1", game.getDefine<TestMaster>(),
                Enumerable.Repeat(game.getDefine<FireBall>(), 29).Cast<CardDefine>()
                .Concat(Enumerable.Repeat(game.getDefine<SorcererApprentice>(), 1).Cast<CardDefine>()));

            game.skipTurnUntil(() =>
                game.currentPlayer == game.players[0] &&
                game.players[0].gem >= game.getDefine<SorcererApprentice>().cost &&
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
            Assert.AreEqual(TestMaster.ID, CATEGORY.getCharacterID(TestSkill.ID));
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
        [Test]
        public void corruptionTest()
        {
            TestGameflow.createGame(out var game, out var you, out var oppo,
                new KeyValuePair<int, int>(Corruption.ID, 1));
            game.skipUntilCanUse<DefaultServant>(oppo);
            oppo.cmdUse(game, oppo.hand.getCard<DefaultServant>());
            game.skipUntilCanUse<Corruption>(you);
            you.cmdUse(game, you.hand.getCard<Corruption>(), 0, oppo.field[0]);
            you.cmdTurnEnd(game);
            oppo.cmdTurnEnd(game);
            Assert.AreEqual(0, oppo.field.count);
        }
        [Test]
        public void equipTest()
        {
            TestGameflow.createGame(out var game, out var you, out var oppo,
                new KeyValuePair<int, int>(TestWeapon.ID, 30));
            game.skipUntilCanUse<TestWeapon>(you);
            you.cmdUse(game, you.hand.getCard<TestWeapon>());
            Assert.True(you.master.canAttack(game));
            you.cmdAttack(game, you.master, oppo.master);
            Assert.AreEqual(29, oppo.master.getCurrentLife(game));
            game.skipUntilHasGem(you, 2);
            you.cmdUse(game, you.hand.getCard<TestWeapon>());
            Assert.IsInstanceOf<TestWeapon>(you.item.define);
            you.cmdAttack(game, you.master, oppo.master);
            Assert.AreEqual(28, oppo.master.getCurrentLife(game));
            Assert.AreEqual(2, you.item.getCurrentLife(game));
            you.cmdUse(game, you.hand.getCard<TestWeapon>());
            Assert.AreEqual(2, you.grave.count);
            Assert.AreEqual(3, you.item.getCurrentLife(game));
        }
        [Test]
        public void lightspawnTest()
        {
            TestGameflow.createGame(out var game, out var you, out var oppo,
                new KeyValuePair<int, int>(Lightspawn.ID, 1));
            game.skipUntilCanUse<Lightspawn>(you);
            you.cmdUse(game, you.hand.getCard<Lightspawn>());
            Card lightspawn = you.field.getCard<Lightspawn>();
            Assert.AreEqual(5, lightspawn.getAttack(game));
            lightspawn.damage(game, null, 2);
            Assert.AreEqual(3, lightspawn.getAttack(game));
            lightspawn.heal(game, null, 1);
            Assert.AreEqual(4, lightspawn.getAttack(game));
        }
        [Test]
        public void secretTest()
        {
            TestGameflow.createGame(out var game, out var you, out var oppo,
                new KeyValuePair<int, int>[]
                {
                    new KeyValuePair<int, int>(FreezingTrap.ID, 3),
                    new KeyValuePair<int, int>(Vaporize.ID, 3)
                },
                new KeyValuePair<int, int>[]
                {
                    new KeyValuePair<int, int>(ChargeServant.ID,30)
                }
            );
            //单独测试：冰冻、蒸发
            game.skipUntilCanUse<FreezingTrap>(you);
            Card secret = you.hand.getCard<FreezingTrap>();
            you.cmdUse(game, secret);
            Assert.AreEqual(1, you.head.count);
            Assert.AreEqual(secret, you.head[0]);
            you.cmdTurnEnd(game);
            Card servant = oppo.hand.getCard<ChargeServant>();
            oppo.cmdUse(game, servant);
            oppo.cmdAttack(game, servant, you.master);
            var eventArg = game.triggers.getRecordedEvents().OfType<THHCard.SecretTriggerEventArg>().Last();
            Assert.NotNull(eventArg);
            Assert.AreEqual(secret, eventArg.card);
            Assert.True(oppo.hand.Contains(servant));
            oppo.cmdTurnEnd(game);

            game.skipUntilCanUse<Vaporize>(you);
            secret = you.hand.getCard<Vaporize>();
            you.cmdUse(game, secret);
            you.cmdTurnEnd(game);
            servant = oppo.hand.getCard<ChargeServant>();
            oppo.cmdUse(game, servant);
            oppo.cmdAttack(game, servant, you.master);
            eventArg = game.triggers.getRecordedEvents().OfType<THHCard.SecretTriggerEventArg>().Last();
            Assert.AreEqual(secret, eventArg.card);
            Assert.True(oppo.grave.Contains(servant));
            oppo.cmdTurnEnd(game);

            //组合测试：蒸发、冰冻
            game.skipUntilCanUse(you, game.getDefine<FreezingTrap>(), game.getDefine<Vaporize>());
            secret = you.hand.getCard<Vaporize>();
            you.cmdUse(game, secret);
            secret = you.hand.getCard<FreezingTrap>();
            you.cmdUse(game, secret);
            you.cmdTurnEnd(game);
            servant = oppo.hand.getCard<ChargeServant>();
            oppo.cmdUse(game, servant);
            oppo.cmdAttack(game, servant, you.master);
            //断点观察：you头上是否还有冰冻
            eventArg = game.triggers.getRecordedEvents().OfType<THHCard.SecretTriggerEventArg>().Last();
            Assert.IsInstanceOf<Vaporize>(eventArg.card.define);
            Assert.IsInstanceOf<FreezingTrap>(you.head[0].define);
            servant = oppo.hand.getCard<ChargeServant>();
            oppo.cmdUse(game, servant);
            oppo.cmdAttack(game, oppo.field[0], you.master);


            //组合测试：冰冻、蒸发
            game.skipUntilCanUse(you, game.getDefine<FreezingTrap>(), game.getDefine<Vaporize>());
            secret = you.hand.getCard<FreezingTrap>();
            you.cmdUse(game, secret);
            secret = you.hand.getCard<Vaporize>();
            you.cmdUse(game, secret);
            you.cmdTurnEnd(game);
            servant = oppo.hand.getCard<ChargeServant>();
            oppo.cmdUse(game, servant);
            oppo.cmdAttack(game, servant, you.master);
            //断点观察：you头上是否还有蒸发
            eventArg = game.triggers.getRecordedEvents().OfType<THHCard.SecretTriggerEventArg>().Last();
            Assert.IsInstanceOf<FreezingTrap>(eventArg.card.define);
            Assert.IsInstanceOf<Vaporize>(you.head[0].define);

            game.Dispose();
        }

        [Test]
        public void PolymorphTest()
        {
            TestGameflow.createGame(out var game, out var you, out var oppo,
                new KeyValuePair<int, int>[]
                {
                    new KeyValuePair<int, int>(Polymorph.ID, 6)
                },
                new KeyValuePair<int, int>[]
                {
                    new KeyValuePair<int, int>(DefaultServant.ID,30)
                }
            );
            game.skipUntilCanUse<DefaultServant>(oppo);
            Card servant = oppo.hand.getCard<DefaultServant>();
            oppo.cmdUse(game, servant);
            oppo.cmdTurnEnd(game);
            game.skipUntilCanUse<Polymorph>(you);
            Card polymorph = you.hand.getCard<Polymorph>();
            you.cmdUse(game, polymorph, targets: oppo.field[0]);
        }
        [Test]
        public void PowrOverwhelmingTest()
        {
            THHGame game = TestGameflow.initGameWithoutPlayers(null, new GameOption()
            {
                shuffle = false
            });
            THHPlayer oppo = game.createPlayer(0, "玩家0", game.getDefine<TestMaster2>(),
                Enumerable.Repeat(game.getDefine<DefaultServant>() as CardDefine, 30));
            THHPlayer you = game.createPlayer(1, "玩家1", game.getDefine<Reisen>(),
                Enumerable.Repeat(game.getDefine<DefaultServant>() as CardDefine, 27).Concat(Enumerable.Repeat(game.getDefine<PowrOverwhelming>(), 3)));

            game.skipUntilCanUse<DefaultServant>(you);
            you.cmdUse(game, you.hand.getCard<DefaultServant>());

            game.skipUntilCanUse<PowrOverwhelming>(you);
            you.cmdUse(game, you.hand.getCard<PowrOverwhelming>(), targets: you.field[0]);
            you.cmdTurnEnd(game);
            game.Dispose();
        }
        [Test]
        public void VelenTest()
        {
            THHGame game = TestGameflow.initGameWithoutPlayers(null, new GameOption()
            {
                shuffle = false
            });
            THHPlayer oppo = game.createPlayer(0, "玩家0", game.getDefine<TestMaster2>(),
                Enumerable.Repeat(game.getDefine<DefaultServant>() as CardDefine, 1));
            THHPlayer you = game.createPlayer(1, "玩家1", game.getDefine<Reisen>(),
                Enumerable.Repeat(game.getDefine<TestHeal>() as CardDefine, 1).Concat(Enumerable.Repeat(game.getDefine<Velen>(), 1)));

            game.skipUntilCanUse<Velen>(you);
            you.cmdUse(game, you.hand.getCard<Velen>());
            you.cmdUse(game, you.hand.getCard<TestHeal>(), targets: you.master);

            game.Dispose();
        }
        [Test]
        public void multipleHaloTest()
        {
            TestGameflow.createGame(out var game, out var you, out var _,
                new KeyValuePair<int, int>(SorcererApprentice.ID, 2),
                new KeyValuePair<int, int>(FireBall.ID, 1));
            game.skipUntilCanUse(you, game.getDefine<SorcererApprentice>(),
                                      game.getDefine<SorcererApprentice>(),
                                      game.getDefine<FireBall>());
            you.cmdUse(game, you.hand.getCard<SorcererApprentice>());
            you.cmdUse(game, you.hand.getCard<SorcererApprentice>());
            Assert.AreEqual(game.getDefine<FireBall>().cost - 2, you.hand.getCard<FireBall>().getCost(game));
            you.field.getCard<SorcererApprentice>().die(game);
            game.updateDeath(game);
            Assert.AreEqual(game.getDefine<FireBall>().cost - 1, you.hand.getCard<FireBall>().getCost(game));
        }
        [Test]
        public void rushServantBackToHandTest()
        {
            TestGameflow.createGame(out var game, out var you, out var oppo,
                new KeyValuePair<int, int>(RushServant.ID, 1));
            game.skipUntilCanUse<DefaultServant>(oppo);
            oppo.cmdUse(game, oppo.hand.getCard<DefaultServant>());
            game.skipUntilCanUse(you, game.getDefine<RushServant>(), game.getDefine<RushServant>());
            you.cmdUse(game, you.hand.getCard<RushServant>());
            Assert.True(you.field.getCard<RushServant>().isRush(game));
            Assert.True(you.field.getCard<RushServant>().canAttack(game));
            _ = you.field.getCard<RushServant>().fieldBackToHand(game);
            Assert.True(you.hand.getCard<RushServant>().isRush(game));
            you.cmdUse(game, you.hand.getCard<RushServant>());
            Assert.True(you.field.getCard<RushServant>().isRush(game));
            Assert.True(you.field.getCard<RushServant>().canAttack(game));
            you.cmdAttack(game, you.field.getCard<RushServant>(), oppo.field[0]);
            Assert.True(game.triggers.getRecordedEvents().OfType<THHCard.AttackEventArg>().Count() > 0);
        }
        [Test]
        public void singleTargetEffectFilterTest()
        {
            TestGameflow.createGame(out var game, out var you, out var oppo,
                new KeyValuePair<int, int>(EnemyOnlyBattleCry.ID, 1));
            game.skipUntilCanUse<EnemyOnlyBattleCry>(you);
            Assert.False(you.hand.getCard<EnemyOnlyBattleCry>().isValidTarget(game, you.master));
        }

        [Test]
        public void freezeServantTest()
        {
            TestGameflow.createGame(out var game, out var you, out var _,
                new KeyValuePair<int, int>(TestFreezeBrain.ID, 3),
                new KeyValuePair<int, int>(DefaultServant.ID, 1));

            game.skipUntilCanUse<DefaultServant>(you);
            you.cmdUse(game, you.hand.getCard<DefaultServant>());
            you.cmdUse(game, you.hand.getCard<TestFreezeBrain>(), targets: you.field[0]);
            you.cmdUse(game, you.hand.getCard<TestFreezeBrain>(), targets: you.master);
            you.cmdTurnEnd(game);


        }
        [Test]
        public void RevealTest()
        {
            TestGameflow.createGame(out var game, out var you, out var oppo,
                new KeyValuePair<int, int>(DefaultServant.ID, 30)
            );
            var reveal = you.Reveal(game, you.deck);
            Assert.True(reveal.IsCompleted);

            game.Dispose();
        }
        /// <summary>
        /// 恐惧战马测试
        /// </summary>
        [Test]
        public void dreadsteedTest()
        {
            TestGameflow.createGame(out var game, out var you, out var oppo,
                new KeyValuePair<int, int>[]
                {
                    new KeyValuePair<int, int>(Dreadsteed.ID, 30)
                },
                new KeyValuePair<int, int>[]
                {
                    new KeyValuePair<int, int>(Dreadsteed.ID, 30)
                });
            game.skipUntilCanUse<Dreadsteed>(you);
            you.cmdUse<Dreadsteed>(game);
            game.skipUntilCanUse<Dreadsteed>(oppo);
            oppo.cmdUse<Dreadsteed>(game);
            game.skipUntilYourTurn(you);
            you.cmdAttack(game, you.field.getCard<Dreadsteed>(), oppo.field.getCard<Dreadsteed>());
            //马都还在场上
            Assert.AreEqual(1, you.field.count);
            Assert.AreEqual(1, oppo.field.count);
            Assert.AreEqual(1, you.field.getCard<Dreadsteed>().getCurrentLife(game));
            Assert.AreEqual(1, oppo.field.getCard<Dreadsteed>().getCurrentLife(game));
        }
        /// <summary>
        /// 阿努巴拉克测试
        /// </summary>
        [Test]
        public void anubarakTest()
        {
            TestGameflow.createGame(out var game, out var you, out var oppo,
                new KeyValuePair<int, int>[]
                {
                    new KeyValuePair<int, int>(Anubarak.ID, 30)
                },
                new KeyValuePair<int, int>[]
                {
                    new KeyValuePair<int, int>(Anubarak.ID, 30)
                });
            game.skipUntilCanUse<Anubarak>(you);
            you.cmdUse(game, you.hand.getCard<Anubarak>());
            Card firstAnubrak = you.field.getCard<Anubarak>();
            game.skipUntilCanUse<Anubarak>(oppo);
            oppo.cmdUse(game, oppo.hand.getCard<Anubarak>());
            game.skipUntilYourTurn(you);
            you.cmdUse(game, you.hand.getCard<Anubarak>());//腾一下手牌
            Assert.AreEqual(9, you.hand.count);
            you.cmdAttack(game, firstAnubrak, oppo.field.getCard<Anubarak>());//对撞，都死，生俩小蜘蛛，回手，当然对面的手牌已经满了
            Assert.True(you.field.Any(c => c.define is Nerubian));
            Assert.AreEqual(2, you.field.count);
            Assert.True(oppo.field.Any(c => c.define is Nerubian));
            Assert.AreEqual(1, oppo.field.count);
            Assert.False(you.grave.Any(c => c.define is Anubarak));
            Assert.AreEqual(10, you.hand.count);
            Assert.True(you.hand.Contains(firstAnubrak));
            Assert.AreEqual(10, oppo.hand.count);
            Assert.True(oppo.grave.Any(c => c.define is Anubarak));
        }
        [Test]
        public void blessingOfMightTest()
        {
            TestGameflow.createGame(out var game, out var you, out _,
                new KeyValuePair<int, int>[]
                {
                    new KeyValuePair<int, int>(BlessingOfMight.ID, 1),
                    new KeyValuePair<int, int>(DefaultServant.ID, 29)
                });
            game.skipUntilCanUse<DefaultServant>(you);
            you.cmdUse<DefaultServant>(game);
            game.skipUntilCanUse<BlessingOfMight>(you);
            you.cmdUse<BlessingOfMight>(game, 0, you.field.getCard<DefaultServant>());
            Assert.AreEqual(4, you.field.getCard<DefaultServant>().getAttack(game));
            Card.PropChangeEventArg propChange = game.triggers.getRecordedEvents().OfType<Card.PropChangeEventArg>().LastOrDefault();
            Assert.NotNull(propChange);
            Assert.AreEqual(nameof(ServantCardDefine.attack), propChange.propName);
            Assert.AreEqual(4, propChange.value);
        }
    }
}