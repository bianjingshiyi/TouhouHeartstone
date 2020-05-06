using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using System.Linq;
using TouhouHeartstone;
using TouhouCardEngine;
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

            Assert.AreEqual(2, game.sortedPlayers[0].field[0].getAttack());
            Assert.AreEqual(2, game.sortedPlayers[0].field[0].getLife());
            Assert.AreEqual(2, game.sortedPlayers[0].field[0].getCurrentLife());
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

            Assert.AreEqual(4, card.getAttack());
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

            Assert.AreEqual(4, card.getAttack());
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

            Assert.AreEqual(2, card.getAttack());
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

            Assert.AreEqual(2, card.getAttack());

            card.removeBuff(null, buffs[1]);

            Assert.AreEqual(4, card.getAttack());
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
            Assert.AreEqual(0, card.getAttack());

            for (int i = 0; i < buffs.Length; i++)
            {
                card.removeBuff(null, buffs[i]);
            }
            Assert.AreEqual(2, card.getAttack());
        }
        /// <summary>
        /// 领军生平测试
        /// </summary>
        [Test]
        public void haloAndSetBuffTest()
        {
            CardEngine game = new CardEngine(null, null, 0)
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

            Assert.AreEqual(2, card.getCurrentLife());
            Assert.AreEqual(2, card.getLife());
        }
        /// <summary>
        /// 在移除Buff的时候，如果随从的生命值是满的，那么当前生命值变更为移除Buff之后的生命值。
        /// </summary>
        [Test]
        public void removeBuffWhenInjureTest()
        {
            CardEngine game = new CardEngine(null, null, 0)
            {
                logger = new UnityLogger()
            };
            Card card = new Card(0);
            card.setLife(5);
            card.setCurrentLife(5);
            TestBuff buff = new TestBuff(new LifeModifier(3, true));
            card.addBuff(game, buff);
            Assert.AreEqual(3, card.getLife());
            Assert.AreEqual(3, card.getCurrentLife());
            card.removeBuff(game, buff);
            Assert.AreEqual(5, card.getLife());
            Assert.AreEqual(5, card.getCurrentLife());
            card.addBuff(game, buff);
            Assert.AreEqual(3, card.getLife());
            Assert.AreEqual(3, card.getCurrentLife());
            //不满血
            card.setCurrentLife(1);
            card.removeBuff(game, buff);
            Assert.AreEqual(5, card.getLife());
            Assert.AreEqual(1, card.getCurrentLife());
        }
        class TestBuff : Buff
        {
            public override int id { get; } = 0;
            public override PropModifier[] modifiers { get; } = null;
            public TestBuff(params PropModifier[] modifiers)
            {
                this.modifiers = modifiers;
            }
        }
    }
}