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
    public class YoumuCardTest
    {
        [Test]
        public void WonderfulhalfbodyTest()
        {
            TestGameflow.createGame(out var game, out var you, out var oppo,
                new KeyValuePair<int, int>(Wonderfulhalfbody.ID, 8)
            );
            game.skipTurnUntil(() => game.currentPlayer == you && you.gem >= 2);
            you.cmdUse(game, you.hand.First());
            Assert.AreEqual(2, you.armor);
            you.master.damage(game, null, 1);
            Assert.AreEqual(30, you.master.getCurrentLife(game));
            Assert.AreEqual(1, you.armor);
            you.master.damage(game, null, 4);
            Assert.AreEqual(0, you.armor);
            Assert.AreEqual(27, you.master.getCurrentLife(game));
        }

        /*[Test]
        public void MasterAttackTest()
        {
            TestGameflow.createGame(out var game, out var you, out var oppo,
                new KeyValuePair<int, int>(Wonderfulhalfbody.ID, 8)
            );
            game.skipTurnUntil(() => game.currentPlayer == oppo && oppo.gem >= 1);
            oppo.cmdUse(game, oppo.hand.First());//对手拍一张白板随从
            oppo.cmdTurnEnd(game);
            you.master.setAttack(5);
            you.cmdAttack(game, you.master, target: oppo.field[0]);
        }*/

        [Test]
        public void CutInTheWorldTest()
        {
            TestGameflow.createGame(out var game, out var you, out var oppo,
                new KeyValuePair<int, int>(CutInTheWorld.ID, 8)
            );
            game.skipTurnUntil(() => game.currentPlayer == you && you.gem >= 4);
            Card card = you.hand.First();
            you.cmdUse(game, card);
            Assert.True(card.isAccumulate(game));
            Assert.AreEqual(3, you.master.getAttack(game));
            card = you.hand.Last();
            you.cmdUse(game,card);
            Assert.False(card.isAccumulate(game));
            Assert.AreEqual(5, you.master.getAttack(game));
        }

        [Test]
        public void MeditationSlashTest()
        {
            TestGameflow.createGame(out var game, out var you, out var oppo,
                new KeyValuePair<int, int>(MeditationSlash.ID, 8),
                new KeyValuePair<int, int>(PhilosopherStone.ID,8)
            );
            game.skipTurnUntil(() => game.currentPlayer == you && you.gem >= 1);
            you.cmdUse(game, you.hand.getCard<MeditationSlash>());//未蓄力
            Card card = game.triggers.getRecordedEvents().OfType<THHPlayer.DrawEventArg>().Last().card;
            Assert.AreEqual(0, card.getAttack(game));
            game.skipTurnUntil(() => game.currentPlayer == you && you.gem >= 3);
            you.cmdUse(game, you.hand.First(c=>c.define is MeditationSlash));//蓄力
            card = game.triggers.getRecordedEvents().OfType<THHPlayer.DrawEventArg>().Last().card;
            Assert.AreEqual(1, card.getAttack(game));
        }

        [Test]
        public void EnlightenmentTest()
        {
            TestGameflow.createGame(out var game, out var you, out var oppo,
                new KeyValuePair<int, int>(Enlightenment.ID, 8)
            );
            game.skipTurnUntil(() => game.currentPlayer == you && you.gem >= 2);
            you.cmdUse(game, you.hand.Last());//未蓄力
            Assert.AreEqual(6, you.armor);
            game.skipTurnUntil(() => game.currentPlayer == you && you.gem >= 4);
            Assert.AreEqual(0, you.armor);
            you.cmdUse(game, you.hand.First());
            Assert.AreEqual(6, you.armor);
            game.skipTurnUntil(() => game.currentPlayer == you && you.gem >= 5);
            Assert.AreEqual(6, you.armor);
        }

        [Test]
        public void HalfSpiritSlamTest()
        {
            TestGameflow.createGame(out var game, out var you, out var oppo,
                new KeyValuePair<int, int>(HalfSpiritSlam.ID, 3)
            );
            game.skipTurnUntil(() => game.currentPlayer == oppo && oppo.gem >= 1);
            oppo.cmdUse(game, oppo.hand.First());//对手拍一张白板随从
            game.skipTurnUntil(() => game.currentPlayer == you && you.gem >= 2);
            you.changeArmor(game, 5);
            you.cmdUse(game, you.hand.getCard<HalfSpiritSlam>(),targets:oppo.field[0]);
            Assert.True(game.triggers.getRecordedEvents().OfType<THHCard.DamageEventArg>().Last().value == you.armor);
        }

        [Test]
        public void UnderworlddwellerTest()
        {
            TestGameflow.createGame(out var game, out var you, out var oppo,
                new KeyValuePair<int, int>(Underworlddweller.ID, 3)
            );
            game.skipTurnUntil(() => game.currentPlayer == you && you.gem >= 2);
            you.cmdUse(game, you.hand.getCard<Underworlddweller>());
            you.field[0].die(game);
            Assert.True(you.hand.getCard<Ghost>() != null);
            you.cmdUse(game, you.hand.getCard<Ghost>());

        }

        [Test]
        public void LouGuanjianTest()
        {
            TestGameflow.createGame(out var game, out var you, out var oppo,
                new KeyValuePair<int, int>(LouGuanjian.ID, 3)
            );
            game.skipTurnUntil(() => game.currentPlayer == oppo && oppo.gem >= 2);
            oppo.cmdUse(game, oppo.hand.First(),0);
            oppo.cmdUse(game, oppo.hand.First(),1);
            oppo.cmdTurnEnd(game);
            game.skipTurnUntil(() => game.currentPlayer == you && you.gem >= 4);
            you.master.setAttack(5);
            you.cmdUse(game, you.hand.First());//使用楼观剑
            Assert.IsInstanceOf<LouGuanjian>(you.item.define);
            you.cmdAttack(game, you.master, target: oppo.field[0]);
            Assert.True(you.master.canAttack(game));
            you.cmdAttack(game, you.master, target: oppo.field[0]);
            Assert.True(you.master.canAttack(game));
            you.cmdAttack(game, you.master, target: oppo.master);
            Assert.False(you.master.canAttack(game));
        }

        [Test]
        public void MonsterBlacksmithTest()
        {
            TestGameflow.createGame(out var game, out var you, out var oppo,
                new KeyValuePair<int, int>(LouGuanjian.ID, 3),
                new KeyValuePair<int, int>(MonsterBlacksmith.ID, 3)
            );
            game.skipTurnUntil(() => game.currentPlayer == oppo && oppo.gem >= 2);
            oppo.cmdUse(game, oppo.hand.First(), 0);
            oppo.cmdUse(game, oppo.hand.First(), 1);
            oppo.cmdTurnEnd(game);
            game.skipTurnUntil(() => game.currentPlayer == you && you.gem >= 5);
            you.master.setAttack(5);
            you.cmdUse(game, you.hand.First(c=>c.define is LouGuanjian));
            Assert.IsInstanceOf<LouGuanjian>(you.item.define);
            you.cmdUse(game, you.hand.getCard<MonsterBlacksmith>());
            Assert.True(you.item.isImmunity(game));
            you.cmdAttack(game, you.master, target: oppo.field[0]);
            Assert.Equals(30,you.master.getCurrentLife(game));
        }
        
        [Test]
        public void YoukiKonpakuTest()
        {
            TestGameflow.createGame(out var game, out var you, out var oppo,
                new KeyValuePair<int, int>(LouGuanjian.ID, 3),
                new KeyValuePair<int, int>(YoukiKonpaku.ID, 3)
            );
            game.skipTurnUntil(() => game.currentPlayer == oppo && oppo.gem >= 2);
            oppo.cmdUse(game, oppo.hand.First(), 0);
            oppo.cmdUse(game, oppo.hand.First(), 1);
            oppo.cmdTurnEnd(game);
            game.skipTurnUntil(() => game.currentPlayer == you && you.gem >= 5);
            you.cmdUse(game, you.hand.getCard<YoukiKonpaku>());
            you.master.setAttack(5);
            you.cmdUse(game, you.hand.First(c => c.define is LouGuanjian));
            Assert.IsInstanceOf<LouGuanjian>(you.item.define);
            you.cmdAttack(game, you.master, target: oppo.field[0]);
            Assert.AreEqual(4, you.item.getAttack(game));
            you.cmdAttack(game, you.master, target: oppo.field[1]);
            Assert.AreEqual(5, you.item.getAttack(game));
        }

        [Test]
        public void HalfGhostTest()
        {
            TestGameflow.createGame(out var game, out var you, out var oppo,
                new KeyValuePair<int, int>(HalfGhost.ID, 5)
            );
            game.skipTurnUntil(() => game.currentPlayer == you && you.gem >= 5);
            you.changeArmor(game, 6);
            you.cmdUse(game, you.hand.getCard<HalfGhost>());
            Assert.AreEqual(7, you.field[0].getAttack(game));
            Assert.AreEqual(7, you.field[0].getLife(game));
        }

        [Test]
        public void TwoHundredYushunInAFlashTest()
        {
            TestGameflow.createGame(out var game, out var you, out var oppo,
                new KeyValuePair<int, int>(TwoHundredYushunInAFlash.ID, 5)
            );
            game.skipTurnUntil(() => game.currentPlayer == you && you.gem >= 1);
            you.setGem(game,10);
            int count = you.hand.count;
            you.cmdUse(game, you.hand.getCard<TwoHundredYushunInAFlash>());
            Assert.AreEqual(count, you.hand.count);
            count = you.hand.count;
            you.changeArmor(game, 1);
            you.cmdUse(game, you.hand.getCard<TwoHundredYushunInAFlash>());
            Assert.AreEqual(count+1, you.hand.count);
        }

        [Test]
        public void CherryBlossomTest()
        {
            TestGameflow.createGame(out var game, out var you, out var oppo,
                new KeyValuePair<int, int>(CherryBlossom.ID, 5)
            );
            game.skipTurnUntil(() => game.currentPlayer == oppo && oppo.gem >= 3);
            for (int i = 0; i < 3; i++)
            {
                oppo.cmdUse(game, oppo.hand.First(), i);
                oppo.field[i].setCurrentLife(2+i);
            }
            oppo.cmdTurnEnd(game);
            you.master.setAttack(1);
            you.cmdUse(game, you.hand.getCard<CherryBlossom>());
            THHCard.AttackEventArg[] arg = game.triggers.getRecordedEvents().OfType<THHCard.AttackEventArg>().ToArray();
            for(int i= 0;i<3;i++)
            {
                Assert.True(arg[i].target == oppo.field[i]);
            }
        }

        [Test]
        public void FaintBitterWheelTest()
        {
            TestGameflow.createGame(out var game, out var you, out var oppo,
                new KeyValuePair<int, int>(FaintBitterWheel.ID, 5)
            );
            game.skipTurnUntil(() => game.currentPlayer == you && you.gem >= 4);
            you.changeArmor(game, 30);
            Assert.AreEqual(30, you.armor);
            you.cmdUse(game, you.hand.getCard<FaintBitterWheel>());
            Assert.AreEqual(0, you.armor);
            Assert.AreEqual(30, you.master.getAttack(game));
        }

        [Test]
        public void HeavenAndManAlsoFailTest()
        {
            TestGameflow.createGame(out var game, out var you, out var oppo,
                new KeyValuePair<int, int>(HeavenAndManAlsoFail.ID, 10)
            );
            game.skipTurnUntil(() => game.currentPlayer == you && you.gem >= 5);
            you.cmdUse(game, you.hand.Last());//刚抽的没有蓄力
            Assert.AreEqual(5, you.master.getAttack(game));

            game.skipTurnUntil(() => game.currentPlayer == you && you.gem >= 7);
            you.cmdUse(game, you.hand.First());//蓄力了
            Assert.AreEqual(7, you.master.getAttack(game));
        }

        [Test]
        public void LinedUpTest()
        {
            TestGameflow.createGame(out var game, out var you, out var oppo,
                new KeyValuePair<int, int>(LinedUp.ID, 5)
            );
            game.skipTurnUntil(() => game.currentPlayer == you && you.gem >= 3);
            you.cmdUse(game, you.hand.First());
            you.cmdTurnEnd(game);
            Assert.AreEqual(0, game.triggers.getRecordedEvents().OfType<THHPlayer.CreateTokenEventArg>().Count());
            oppo.cmdTurnEnd(game);
            you.changeArmor(game, 5);
            you.cmdTurnEnd(game);
            Assert.IsInstanceOf<Ghost>(you.field[you.field.count-1].define);
        }

        [Test]
        public void BailoujianTest()
        {
            TestGameflow.createGame(out var game, out var you, out var oppo,
                new KeyValuePair<int, int>(Bailoujian.ID, 5),
                new KeyValuePair<int, int>(LouGuanjian.ID, 5)
            );
            game.skipTurnUntil(() => game.currentPlayer == you && you.gem >= 6);
            you.cmdUse(game, you.hand.getCard<LouGuanjian>());
            Assert.IsInstanceOf<LouGuanjian>(you.item.define);
            Assert.AreEqual(2, you.item.getCurrentLife(game));
            you.cmdUse(game, you.hand.getCard<Bailoujian>());
            Assert.IsInstanceOf<LouGuanjian>(you.item.define);
            Assert.AreEqual(4, you.item.getCurrentLife(game));
        }
    }
}

