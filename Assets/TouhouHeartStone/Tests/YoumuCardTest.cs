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
        public void StrangeHalfbodyTest()
        {
            TestGameflow.createGame(out var game, out var you, out var oppo, Youmu.ID);
            game.skipUntilCanUse<StrangeHalfbody>(you);
            you.cmdUse(game, you.skill);
            Assert.AreEqual(2, you.master.getArmor(game));
            you.master.damage(game, null, 1);
            Assert.AreEqual(30, you.master.getCurrentLife(game));
            Assert.AreEqual(1, you.master.getArmor(game));
            you.master.damage(game, null, 4);
            Assert.AreEqual(0, you.master.getArmor(game));
            Assert.AreEqual(27, you.master.getCurrentLife(game));
        }
        [Test]
        public void SlashOfPresentWorldTest()
        {
            TestGameflow.createGame(out var game, out var you, out var oppo,
                new KeyValuePair<int, int>(SlashOfPresentWorld.ID, 8)
            );
            game.skipTurnUntil(() => game.currentPlayer == you && you.gem >= game.getDefine<SlashOfPresentWorld>().cost * 2);
            Card card = you.hand.First();
            Assert.True(Youmu.isPreHeated(game, card));
            you.cmdUse(game, card);
            Assert.AreEqual(3, you.master.getAttack(game));
            card = you.hand.Last();
            Assert.False(Youmu.isPreHeated(game, card));
            you.cmdUse(game, card);
            Assert.AreEqual(5, you.master.getAttack(game));
        }

        [Test]
        public void MeditationTest()
        {
            TestGameflow.createGame(out var game, out var you, out var oppo,
                new KeyValuePair<int, int>(Meditation.ID, 2),
                new KeyValuePair<int, int>(DefaultServant.ID, 26),
                new KeyValuePair<int, int>(TestWeapon.ID, 2)
            );
            game.skipUntilCanUse<Meditation>(you);
            you.cmdUse(game, you.hand.getCard<Meditation>());//未蓄力
            Card card = game.triggers.getRecordedEvents().OfType<THHPlayer.DrawEventArg>().Last().card;
            Assert.AreEqual(3, card.getLife(game));
            game.skipUntilCanUse<Meditation>(you);
            you.cmdUse(game, you.hand.getCard<Meditation>());//蓄力
            card = game.triggers.getRecordedEvents().OfType<THHPlayer.DrawEventArg>().Last().card;
            Assert.AreEqual(4, card.getLife(game));
        }

        [Test]
        public void FantasyOfEnteringEnlightenmentTest()
        {
            TestGameflow.createGame(out var game, out var you, out var oppo,
                new KeyValuePair<int, int>(FantasyOfEnteringEnlightenment.ID, 30)
            );
            game.skipUntilCanUse<FantasyOfEnteringEnlightenment>(you);
            Card card = you.hand.getCards<FantasyOfEnteringEnlightenment>().Last();
            Assert.False(Youmu.isPreHeated(game, card));
            you.cmdUse(game, card);//未蓄力
            Assert.AreEqual(6, you.master.getArmor(game));
            you.cmdTurnEnd(game);
            oppo.cmdTurnEnd(game);
            Assert.AreEqual(3, you.master.getArmor(game));
            card = you.hand.getCards<FantasyOfEnteringEnlightenment>().First();
            Assert.True(Youmu.isPreHeated(game, card));
            you.cmdUse(game, card);
            Assert.AreEqual(9, you.master.getArmor(game));
            you.cmdTurnEnd(game);
            oppo.cmdTurnEnd(game);
            Assert.AreEqual(9, you.master.getArmor(game));
        }

        [Test]
        public void HalfPhantonSlamTest()
        {
            TestGameflow.createGame(out var game, out var you, out var oppo,
                new KeyValuePair<int, int>(HalfPhantonSlam.ID, 3)
            );
            game.skipTurnUntil(() => game.currentPlayer == oppo && oppo.gem >= 1);
            oppo.cmdUse(game, oppo.hand.First());//对手拍一张白板随从
            game.skipTurnUntil(() => game.currentPlayer == you && you.gem >= 2);
            you.master.gainArmor(game, 5);
            you.cmdUse(game, you.hand.getCard<HalfPhantonSlam>(), targets: oppo.field[0]);
            Assert.True(game.triggers.getRecordedEvents().OfType<THHCard.DamageEventArg>().Last().infoDic.First().Value.damagedValue == you.master.getArmor(game));
        }

        [Test]
        public void NetherworldInhabitantTest()
        {
            TestGameflow.createGame(out var game, out var you, out var oppo,
                new KeyValuePair<int, int>(NetherworldInhabitant.ID, 3)
            );
            game.skipTurnUntil(() => game.currentPlayer == you && you.gem >= 2);
            you.cmdUse(game, you.hand.getCard<NetherworldInhabitant>());
            you.field[0].die(game);
            Assert.True(you.hand.getCard<Ghost>() != null);
            you.cmdUse(game, you.hand.getCard<Ghost>());

        }

        [Test]
        public void RoukankenTest()
        {
            TestGameflow.createGame(out var game, out var you, out var oppo,
                new KeyValuePair<int, int>(Roukanken.ID, 3)
            );
            game.skipTurnUntil(() => game.currentPlayer == oppo && oppo.gem >= 2);
            oppo.cmdUse(game, oppo.hand.First(), 0);
            oppo.cmdUse(game, oppo.hand.First(), 1);
            oppo.cmdTurnEnd(game);
            game.skipTurnUntil(() => game.currentPlayer == you && you.gem >= 4);
            you.master.setAttack(game, 5);
            you.cmdUse(game, you.hand.getCard<Roukanken>());//使用楼观剑
            Assert.IsInstanceOf<Roukanken>(you.item.define);
            you.cmdAttack(game, you.master, target: oppo.field[0]);
            Assert.True(you.master.canAttack(game));
            you.cmdAttack(game, you.master, target: oppo.field[0]);
            Assert.True(you.master.canAttack(game));
            you.cmdAttack(game, you.master, target: oppo.master);
            Assert.False(you.master.canAttack(game));
        }

        [Test]
        public void YoukaiSmithTest()
        {
            TestGameflow.createGame(out var game, out var you, out var oppo,
                new KeyValuePair<int, int>(Roukanken.ID, 3),
                new KeyValuePair<int, int>(YoukaiSmith.ID, 3)
            );
            game.skipTurnUntil(() => game.currentPlayer == oppo && oppo.gem >= 2);
            oppo.cmdUse(game, oppo.hand.First(), 0);
            oppo.cmdUse(game, oppo.hand.First(), 1);
            oppo.cmdTurnEnd(game);
            game.skipTurnUntil(() => game.currentPlayer == you && you.gem >= 5);
            you.master.setAttack(game, 5);
            you.cmdUse(game, you.hand.getCard<Roukanken>());
            Assert.IsInstanceOf<Roukanken>(you.item.define);
            you.cmdUse(game, you.hand.getCard<YoukaiSmith>());
            Assert.True(you.item.isImmunity(game));
            you.cmdAttack(game, you.master, target: oppo.field[0]);
            Assert.AreEqual(2, you.item.getCurrentLife(game));
        }

        [Test]
        public void KonpakuYoukiTest()
        {
            TestGameflow.createGame(out var game, out var you, out var oppo,
                new KeyValuePair<int, int>(TestWeapon.ID, 1),
                new KeyValuePair<int, int>(KonpakuYouki.ID, 1)
            );
            game.skipTurnUntil(() => game.currentPlayer == oppo && oppo.gem >= 2);//对手拍两个靶子
            oppo.cmdUse(game, oppo.hand.getCard<DefaultServant>());
            oppo.cmdUse(game, oppo.hand.getCard<DefaultServant>());
            oppo.cmdTurnEnd(game);
            game.skipTurnUntil(() => game.currentPlayer == you && you.gem >= game.getDefine<KonpakuYouki>().cost + game.getDefine<TestWeapon>().cost);
            you.cmdUse(game, you.hand.getCard<KonpakuYouki>());
            you.cmdUse(game, you.hand.getCard<TestWeapon>());
            you.cmdAttack(game, you.master, oppo.field[0]);
            Assert.AreEqual(2, you.item.getAttack(game));
        }

        [Test]
        public void HalfPhantomTest()
        {
            TestGameflow.createGame(out var game, out var you, out _,
                new KeyValuePair<int, int>(HalfPhantom.ID, 1)
            );
            game.skipUntilCanUse<HalfPhantom>(you);
            int originAttack = game.getDefine<HalfPhantom>().attack;
            int originLife = game.getDefine<HalfPhantom>().life;
            you.master.gainArmor(game, 6);
            you.cmdUse(game, you.hand.getCard<HalfPhantom>());
            Assert.AreEqual(originAttack + 6, you.field[0].getAttack(game));
            Assert.AreEqual(originLife + 6, you.field[0].getCurrentLife(game));
            Assert.AreEqual(originLife + 6, you.field[0].getLife(game));
            you.master.gainArmor(game, 1);
            Assert.AreEqual(originAttack + 7, you.field[0].getAttack(game));
            Assert.AreEqual(originLife + 7, you.field[0].getCurrentLife(game));
            Assert.AreEqual(originLife + 7, you.field[0].getLife(game));
            you.field[0].damage(game, null, 1);
            Assert.AreEqual(originLife + 7 - 1, you.field[0].getCurrentLife(game));
            you.master.gainArmor(game, 1);
            Assert.AreEqual(originAttack + 8, you.field[0].getAttack(game));
            Assert.AreEqual(originLife + 8 - 1, you.field[0].getCurrentLife(game));
            Assert.AreEqual(originLife + 8, you.field[0].getLife(game));
            you.master.loseArmor(game, 2);
            Assert.AreEqual(originAttack + 6, you.field[0].getAttack(game));
            Assert.AreEqual(originLife + 6, you.field[0].getCurrentLife(game));
            Assert.AreEqual(originLife + 6, you.field[0].getLife(game));
            //特殊情况，如果受到的伤害足以杀死半灵会如何？
            you.field[0].damage(game, null, originLife);
            Assert.AreEqual(6, you.field[0].getCurrentLife(game));
            Assert.AreEqual(originLife + 6, you.field[0].getLife(game));
            you.master.loseArmor(game, 6);
            Assert.AreEqual(4, you.field[0].getCurrentLife(game));
            Assert.AreEqual(originLife, you.field[0].getLife(game));
            //沉默半灵测试
            you.field.getCard<HalfPhantom>().silence(game);
            Assert.AreEqual(0, you.field.getCard<HalfPhantom>().getAttack(game));
            Assert.AreEqual(4, you.field.getCard<HalfPhantom>().getLife(game));
        }
        
        [Test]
        public void HalfPhantomBackToHandTest()
        {
            TestGameflow.createGame(out var game, out var you, out var oppo,
                new KeyValuePair<int, int>(HalfPhantom.ID, 1));
            you.master.setArmor(game,2);
            game.skipUntilCanUse<HalfPhantom>(you);
            you.cmdUse<HalfPhantom>(game);
            Assert.AreEqual(you.master.getArmor(game),you.field.getCard<HalfPhantom>().getAttack(game));
            you.field.getCard<HalfPhantom>().fieldBackToHand(game);
            Assert.True(you.field.count==0);
            game.skipUntilCanUse<HalfPhantom>(you);
            you.cmdUse<HalfPhantom>(game);
            Assert.AreEqual(you.master.getArmor(game), you.field.getCard<HalfPhantom>().getAttack(game));
            Assert.AreEqual(4+you.master.getArmor(game), you.field.getCard<HalfPhantom>().getLife(game));
            you.master.gainArmor(game, 2);
            Assert.AreEqual(you.master.getArmor(game), you.field.getCard<HalfPhantom>().getAttack(game));
        }
        
        [Test]
        public void HalfPhantomBurgeoningRiseTest()
        {
            TestGameflow.createGame(out var game, out var you, out var oppo,
                new KeyValuePair<int, int>(HalfPhantom.ID, 1),new KeyValuePair<int, int>(BurgeoningRise.ID,1));
            you.master.setArmor(game,2);
            game.skipUntilCanUse<HalfPhantom>(you);
            you.cmdUse<HalfPhantom>(game, out var s);
            Assert.AreEqual(you.master.getArmor(game),s.getAttack(game));
            game.skipUntilCanUse<BurgeoningRise>(you);
            you.cmdUse(game, you.hand.getCard<BurgeoningRise>(), targets: s);
            s.setDead(game,true);
            game.updateDeath(game);
            Assert.AreEqual(1,you.field.count);
            s = you.field[0];
            Assert.True(s.define is HalfPhantom);
            Assert.AreEqual(3+you.master.getArmor(game), s.getAttack(game));
            you.master.gainArmor(game, 2);
            Assert.AreEqual(3+you.master.getArmor(game), s.getAttack(game));
        }

        [Test]
        public void TwoHundredYojanaOneSlashTest()
        {
            TestGameflow.createGame(out var game, out var you, out var oppo,
                new KeyValuePair<int, int>(TwoHundredYojanaOneSlash.ID, 5)
            );
            game.skipTurnUntil(() => game.currentPlayer == you && you.gem >= 1);
            you.setGem(game, 10);
            int count = you.hand.count;
            you.cmdUse(game, you.hand.getCard<TwoHundredYojanaOneSlash>());
            Assert.AreEqual(count, you.hand.count);
            count = you.hand.count;
            you.master.gainArmor(game, 1);
            you.cmdUse(game, you.hand.getCard<TwoHundredYojanaOneSlash>());
            Assert.AreEqual(count + 1, you.hand.count);
        }

        [Test]
        public void CherryBlossomFlashingTest()
        {
            TestGameflow.createGame(out var game, out var you, out var oppo,
                new KeyValuePair<int, int>(TestWeapon.ID, 1),
                new KeyValuePair<int, int>(CherryBlossomFlashing.ID, 1)
            );
            game.skipTurnUntil(() => game.currentPlayer == oppo && oppo.gem >= 5);
            for (int i = 0; i < 5; i++)
            {
                oppo.cmdUse(game, oppo.hand.getCard<DefaultServant>());
            }
            oppo.cmdTurnEnd(game);
            game.skipTurnUntil(() => game.currentPlayer == you && you.gem >= game.getDefine<TestWeapon>().cost + game.getDefine<CherryBlossomFlashing>().cost);
            you.cmdUse(game, you.hand.getCard<TestWeapon>());
            for (int i = 0; i < oppo.field.count; i++)
            {
                oppo.field[i].setCurrentLife(game, i + 3);
            }
            you.cmdUse(game, you.hand.getCard<CherryBlossomFlashing>());
            Assert.AreEqual(4, oppo.field.count);
            Assert.AreEqual(1, oppo.field[0].getCurrentLife(game));
            Assert.AreEqual(2, oppo.field[1].getCurrentLife(game));
            Assert.AreEqual(4, oppo.field[2].getCurrentLife(game));
            Assert.AreEqual(5, oppo.field[3].getCurrentLife(game));
            Assert.AreEqual(0, you.master.getAttackTimes(game));
        }

        [Test]
        public void WheelOfPainOfTheLivingAndDeadTest()
        {
            TestGameflow.createGame(out var game, out var you, out var oppo,
                new KeyValuePair<int, int>(WheelOfPainOfTheLivingAndDead.ID, 5)
            );
            game.skipTurnUntil(() => game.currentPlayer == you && you.gem >= 4);
            you.master.gainArmor(game, 30);
            Assert.AreEqual(30, you.master.getArmor(game));
            you.cmdUse(game, you.hand.getCard<WheelOfPainOfTheLivingAndDead>());
            Assert.AreEqual(0, you.master.getArmor(game));
            Assert.AreEqual(30, you.master.getAttack(game));
        }

        [Test]
        public void FiveSignsOfTheDyingDevaTest()
        {
            TestGameflow.createGame(out var game, out var you, out var oppo,
                new KeyValuePair<int, int>(FiveSignsOfTheDyingDeva.ID, 1)
            );
            game.skipUntilCanUse<FiveSignsOfTheDyingDeva>(you);
            you.cmdUse(game, you.hand.getCard<FiveSignsOfTheDyingDeva>());
            Assert.AreEqual(5, you.master.getAttack(game));
            Assert.AreEqual(5, you.master.getArmor(game));
        }

        [Test]
        public void ProcessingPhantomsTest()
        {
            TestGameflow.createGame(out var game, out var you, out var oppo,
                new KeyValuePair<int, int>(ProcessingPhantoms.ID, 5)
            );
            game.skipTurnUntil(() => game.currentPlayer == you && you.gem >= 3);
            you.cmdUse(game, you.hand.First());
            you.cmdTurnEnd(game);
            Assert.AreEqual(0, game.triggers.getRecordedEvents().OfType<THHPlayer.CreateTokenEventArg>().Count());
            oppo.cmdTurnEnd(game);
            you.master.gainArmor(game, 5);
            you.cmdTurnEnd(game);
            Assert.IsInstanceOf<Ghost>(you.field[you.field.count - 1].define);
        }

        [Test]
        public void HakuroukenTest()
        {
            TestGameflow.createGame(out var game, out var you, out var oppo,
                new KeyValuePair<int, int>(Hakurouken.ID, 2),
                new KeyValuePair<int, int>(TestWeapon.ID, 1)
            );
            game.skipUntilCanUse<Hakurouken>(you);
            you.cmdUse(game, you.hand.getCard<Hakurouken>());
            Assert.IsInstanceOf<Hakurouken>(you.item.define);

            game.skipUntilCanUse<TestWeapon>(you);
            you.cmdUse(game, you.hand.getCard<TestWeapon>());
            Assert.IsInstanceOf<TestWeapon>(you.item.define);

            game.skipUntilCanUse<Hakurouken>(you);
            Assert.AreEqual(3, you.item.getCurrentLife(game));
            you.cmdUse(game, you.hand.getCard<Hakurouken>());
            Assert.AreEqual(6, you.item.getCurrentLife(game));
            Assert.True(!you.hand.Any(c => c.define is Hakurouken));
            Assert.AreEqual(2, you.grave.Where(c => c.define is Hakurouken).Count());
        }
        [Test]
        public void caseCoinRoukankenTest()
        {
            TestGameflow.createGame(out var game, out var you, out var oppo, new GameOption()
            {
                shuffle = false,
                sortedPlayers = new int[] { 2, 1 }
            }, new KeyValuePair<int, int>(Roukanken.ID, 30));
            game.skipUntilYourTurn(you);
            you.cmdUse(game, you.hand.getCard<LuckyCoin>());
            you.cmdUse(game, you.hand.getCard<Roukanken>());
            Assert.True(you.master.canAttack(game));
            Assert.AreEqual(3, you.master.getAttack(game));
            you.cmdAttack(game, you.master, oppo.master);
            Assert.AreEqual(27, oppo.master.getCurrentLife(game));
            Assert.AreEqual(1, you.item.getCurrentLife(game));
        }
    }
}

