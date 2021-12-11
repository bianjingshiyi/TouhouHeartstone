using System;
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
            you.cmdUse(game, you.getSkill());
            Assert.AreEqual(2, you.getMaster().getArmor(game));
            you.getMaster().damage(game, null, 1);
            Assert.AreEqual(30, you.getMaster().getCurrentLife(game));
            Assert.AreEqual(1, you.getMaster().getArmor(game));
            you.getMaster().damage(game, null, 4);
            Assert.AreEqual(0, you.getMaster().getArmor(game));
            Assert.AreEqual(27, you.getMaster().getCurrentLife(game));
        }
        [Test]
        public void SlashOfPresentWorldTest()
        {
            TestGameflow.createGame(out var game, out var you, out var oppo,
                new KeyValuePair<int, int>(SlashOfPresentWorld.ID, 8)
            );
            game.skipTurnUntil(() => game.getCurrentPlayer() == you && you.getGem() >= game.getDefine<SlashOfPresentWorld>().cost * 2);
            Card card = you.getHand().First();
            Assert.True(Youmu.isPreHeated(game, card));
            you.cmdUse(game, card);
            Assert.AreEqual(3, you.getMaster().getAttack(game));
            card = you.getHand().Last();
            Assert.False(Youmu.isPreHeated(game, card));
            you.cmdUse(game, card);
            Assert.AreEqual(5, you.getMaster().getAttack(game));
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
            you.cmdUse(game, you.getHand().getCard<Meditation>());//未蓄力
            Card card = game.triggers.getRecordedEvents().OfType<HearthStoneRule.DrawEventArg>().Last().card;
            Assert.AreEqual(3, card.getLife(game));
            game.skipUntilCanUse<Meditation>(you);
            you.cmdUse(game, you.getHand().getCard<Meditation>());//蓄力
            card = game.triggers.getRecordedEvents().OfType<HearthStoneRule.DrawEventArg>().Last().card;
            Assert.AreEqual(4, card.getLife(game));
        }

        [Test]
        public void FantasyOfEnteringEnlightenmentTest()
        {
            TestGameflow.createGame(out var game, out var you, out var oppo,
                new KeyValuePair<int, int>(FantasyOfEnteringEnlightenment.ID, 30)
            );
            game.skipUntilCanUse<FantasyOfEnteringEnlightenment>(you);
            Card card = you.getHand().getCards<FantasyOfEnteringEnlightenment>().Last();
            Assert.False(Youmu.isPreHeated(game, card));
            you.cmdUse(game, card);//未蓄力
            Assert.AreEqual(6, you.getMaster().getArmor(game));
            you.cmdTurnEnd(game);
            oppo.cmdTurnEnd(game);
            Assert.AreEqual(3, you.getMaster().getArmor(game));
            card = you.getHand().getCards<FantasyOfEnteringEnlightenment>().First();
            Assert.True(Youmu.isPreHeated(game, card));
            you.cmdUse(game, card);
            Assert.AreEqual(9, you.getMaster().getArmor(game));
            you.cmdTurnEnd(game);
            oppo.cmdTurnEnd(game);
            Assert.AreEqual(9, you.getMaster().getArmor(game));
        }

        [Test]
        public void HalfPhantonSlamTest()
        {
            TestGameflow.createGame(out var game, out var you, out var oppo,
                new KeyValuePair<int, int>(HalfPhantonSlam.ID, 3)
            );
            game.skipTurnUntil(() => game.getCurrentPlayer() == oppo && oppo.getGem() >= 1);
            oppo.cmdUse(game, oppo.getHand().First());//对手拍一张白板随从
            game.skipTurnUntil(() => game.getCurrentPlayer() == you && you.getGem() >= 2);
            you.getMaster().gainArmor(game, 5);
            you.cmdUse(game, you.getHand().getCard<HalfPhantonSlam>(), targets: oppo.getField()[0]);
            Assert.True(game.triggers.getRecordedEvents().OfType<THHCard.DamageEventArg>().Last().infoDic.First().Value.damagedValue == you.getMaster().getArmor(game));
        }

        [Test]
        public void NetherworldInhabitantTest()
        {
            TestGameflow.createGame(out var game, out var you, out var oppo,
                new KeyValuePair<int, int>(NetherworldInhabitant.ID, 3)
            );
            game.skipTurnUntil(() => game.getCurrentPlayer() == you && you.getGem() >= 2);
            you.cmdUse(game, you.getHand().getCard<NetherworldInhabitant>());
            you.getField()[0].die(game);
            Assert.True(you.getHand().getCard<Ghost>() != null);
            you.cmdUse(game, you.getHand().getCard<Ghost>());

        }

        [Test]
        public void RoukankenTest()
        {
            TestGameflow.createGame(out var game, out var you, out var oppo,
                new KeyValuePair<int, int>(Roukanken.ID, 3)
            );
            game.skipTurnUntil(() => game.getCurrentPlayer() == oppo && oppo.getGem() >= 2);
            oppo.cmdUse(game, oppo.getHand().First(), 0);
            oppo.cmdUse(game, oppo.getHand().First(), 1);
            oppo.cmdTurnEnd(game);
            game.skipTurnUntil(() => game.getCurrentPlayer() == you && you.getGem() >= 4);
            you.getMaster().addBuff(game, new CodeBuff(0, new AttackModifier(5, true)));
            you.cmdUse(game, you.getHand().getCard<Roukanken>());//使用楼观剑
            Assert.IsInstanceOf<Roukanken>(you.getItem().define);
            you.cmdAttack(game, you.getMaster(), target: oppo.getField()[0]);
            Assert.True(you.getMaster().canAttack(game));
            you.cmdAttack(game, you.getMaster(), target: oppo.getField()[0]);
            Assert.True(you.getMaster().canAttack(game));
            you.cmdAttack(game, you.getMaster(), target: oppo.getMaster());
            Assert.False(you.getMaster().canAttack(game));
        }

        [Test]
        public void YoukaiSmithTest()
        {
            TestGameflow.createGame(out var game, out var you, out var oppo,
                new KeyValuePair<int, int>(Roukanken.ID, 3),
                new KeyValuePair<int, int>(YoukaiSmith.ID, 3)
            );
            game.skipTurnUntil(() => game.getCurrentPlayer() == oppo && oppo.getGem() >= 2);
            oppo.cmdUse(game, oppo.getHand().First(), 0);
            oppo.cmdUse(game, oppo.getHand().First(), 1);
            oppo.cmdTurnEnd(game);
            game.skipTurnUntil(() => game.getCurrentPlayer() == you && you.getGem() >= 5);
            you.getMaster().setAttack(game, 5);
            you.cmdUse(game, you.getHand().getCard<Roukanken>());
            Assert.IsInstanceOf<Roukanken>(you.getItem().define);
            you.cmdUse(game, you.getHand().getCard<YoukaiSmith>());
            Assert.True(you.getItem().isImmunity(game));
            you.cmdAttack(game, you.getMaster(), target: oppo.getField()[0]);
            Assert.AreEqual(2, you.getItem().getCurrentLife(game));
        }

        [Test]
        public void KonpakuYoukiTest()
        {
            TestGameflow.createGame(out var game, out var you, out var oppo,
                new KeyValuePair<int, int>(TestWeapon.ID, 1),
                new KeyValuePair<int, int>(KonpakuYouki.ID, 1)
            );
            game.skipTurnUntil(() => game.getCurrentPlayer() == oppo && oppo.getGem() >= 2);//对手拍两个靶子
            oppo.cmdUse(game, oppo.getHand().getCard<DefaultServant>());
            oppo.cmdUse(game, oppo.getHand().getCard<DefaultServant>());
            oppo.cmdTurnEnd(game);
            game.skipTurnUntil(() => game.getCurrentPlayer() == you && you.getGem() >= game.getDefine<KonpakuYouki>().cost + game.getDefine<TestWeapon>().cost);
            you.cmdUse(game, you.getHand().getCard<KonpakuYouki>());
            you.cmdUse(game, you.getHand().getCard<TestWeapon>());
            you.cmdAttack(game, you.getMaster(), oppo.getField()[0]);
            Assert.AreEqual(2, you.getItem().getAttack(game));
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
            you.getMaster().gainArmor(game, 6);
            you.cmdUse(game, you.getHand().getCard<HalfPhantom>());
            Assert.AreEqual(originAttack + 6, you.getField()[0].getAttack(game));
            Assert.AreEqual(originLife + 6, you.getField()[0].getCurrentLife(game));
            Assert.AreEqual(originLife + 6, you.getField()[0].getLife(game));
            you.getMaster().gainArmor(game, 1);
            Assert.AreEqual(originAttack + 7, you.getField()[0].getAttack(game));
            Assert.AreEqual(originLife + 7, you.getField()[0].getCurrentLife(game));
            Assert.AreEqual(originLife + 7, you.getField()[0].getLife(game));
            you.getField()[0].damage(game, null, 1);
            Assert.AreEqual(originLife + 7 - 1, you.getField()[0].getCurrentLife(game));
            you.getMaster().gainArmor(game, 1);
            Assert.AreEqual(originAttack + 8, you.getField()[0].getAttack(game));
            Assert.AreEqual(originLife + 8 - 1, you.getField()[0].getCurrentLife(game));
            Assert.AreEqual(originLife + 8, you.getField()[0].getLife(game));
            you.getMaster().loseArmor(game, 2);
            Assert.AreEqual(originAttack + 6, you.getField()[0].getAttack(game));
            Assert.AreEqual(originLife + 6, you.getField()[0].getCurrentLife(game));
            Assert.AreEqual(originLife + 6, you.getField()[0].getLife(game));
            //特殊情况，如果受到的伤害足以杀死半灵会如何？
            you.getField()[0].damage(game, null, originLife);
            Assert.AreEqual(6, you.getField()[0].getCurrentLife(game));
            Assert.AreEqual(originLife + 6, you.getField()[0].getLife(game));
            you.getMaster().loseArmor(game, 6);
            Assert.AreEqual(4, you.getField()[0].getCurrentLife(game));
            Assert.AreEqual(originLife, you.getField()[0].getLife(game));
            //沉默半灵测试
            you.getField().getCard<HalfPhantom>().silence(game);
            Assert.AreEqual(0, you.getField().getCard<HalfPhantom>().getAttack(game));
            Assert.AreEqual(4, you.getField().getCard<HalfPhantom>().getLife(game));
        }

        [Test]
        public void HalfPhantomBackToHandTest()
        {
            TestGameflow.createGame(out var game, out var you, out var oppo,
                new KeyValuePair<int, int>(HalfPhantom.ID, 1));
            you.getMaster().setArmor(game, 2);
            game.skipUntilCanUse<HalfPhantom>(you);
            you.cmdUse<HalfPhantom>(game);
            Assert.AreEqual(you.getMaster().getArmor(game), you.getField().getCard<HalfPhantom>().getAttack(game));
            you.getField().getCard<HalfPhantom>().fieldToHand(game);
            Assert.True(you.getField().count == 0);
            game.skipUntilCanUse<HalfPhantom>(you);
            you.cmdUse<HalfPhantom>(game);
            Assert.AreEqual(you.getMaster().getArmor(game), you.getField().getCard<HalfPhantom>().getAttack(game));
            Assert.AreEqual(4 + you.getMaster().getArmor(game), you.getField().getCard<HalfPhantom>().getLife(game));
            you.getMaster().gainArmor(game, 2);
            Assert.AreEqual(you.getMaster().getArmor(game), you.getField().getCard<HalfPhantom>().getAttack(game));
        }

        [Test]
        public void HalfPhantomBurgeoningRiseTest()
        {
            TestGameflow.createGame(out var game, out var you, out var oppo,
                new KeyValuePair<int, int>(HalfPhantom.ID, 1), new KeyValuePair<int, int>(BurgeoningRise.ID, 1));
            you.getMaster().setArmor(game, 2);
            game.skipUntilCanUse<HalfPhantom>(you);
            you.cmdUse<HalfPhantom>(game, out var s);
            Assert.AreEqual(you.getMaster().getArmor(game), s.getAttack(game));
            game.skipUntilCanUse<BurgeoningRise>(you);
            you.cmdUse(game, you.getHand().getCard<BurgeoningRise>(), targets: s);
            s.setDead(game, true);
            game.updateDeath();
            Assert.AreEqual(1, you.getField().count);
            s = you.getField()[0];
            Assert.True(s.define is HalfPhantom);
            Assert.AreEqual(3 + you.getMaster().getArmor(game), s.getAttack(game));
            you.getMaster().gainArmor(game, 2);
            Assert.AreEqual(3 + you.getMaster().getArmor(game), s.getAttack(game));
        }

        [Test]
        public void TwoHundredYojanaOneSlashTest()
        {
            TestGameflow.createGame(out var game, out var you, out var oppo,
                new KeyValuePair<int, int>(TwoHundredYojanaOneSlash.ID, 5)
            );
            game.skipTurnUntil(() => game.getCurrentPlayer() == you && you.getGem() >= 1);
            you.setGem(game, 10);
            int count = you.getHand().count;
            you.cmdUse(game, you.getHand().getCard<TwoHundredYojanaOneSlash>());
            Assert.AreEqual(count, you.getHand().count);
            count = you.getHand().count;
            you.getMaster().gainArmor(game, 1);
            you.cmdUse(game, you.getHand().getCard<TwoHundredYojanaOneSlash>());
            Assert.AreEqual(count + 1, you.getHand().count);
        }

        [Test]
        public void CherryBlossomFlashingTest()
        {
            TestGameflow.createGame(out var game, out var you, out var oppo,
                new KeyValuePair<int, int>(TestWeapon.ID, 1),
                new KeyValuePair<int, int>(CherryBlossomFlashing.ID, 1)
            );
            game.skipTurnUntil(() => game.getCurrentPlayer() == oppo && oppo.getGem() >= 5);
            for (int i = 0; i < 5; i++)
            {
                oppo.cmdUse(game, oppo.getHand().getCard<DefaultServant>());
            }
            oppo.cmdTurnEnd(game);
            game.skipTurnUntil(() => game.getCurrentPlayer() == you && you.getGem() >= game.getDefine<TestWeapon>().cost + game.getDefine<CherryBlossomFlashing>().cost);
            you.cmdUse(game, you.getHand().getCard<TestWeapon>());
            for (int i = 0; i < oppo.getField().count; i++)
            {
                oppo.getField()[i].setCurrentLife(game, i + 3);
            }
            you.cmdUse(game, you.getHand().getCard<CherryBlossomFlashing>());
            Assert.AreEqual(4, oppo.getField().count);
            Assert.AreEqual(1, oppo.getField()[0].getCurrentLife(game));
            Assert.AreEqual(2, oppo.getField()[1].getCurrentLife(game));
            Assert.AreEqual(4, oppo.getField()[2].getCurrentLife(game));
            Assert.AreEqual(5, oppo.getField()[3].getCurrentLife(game));
            Assert.AreEqual(0, you.getMaster().getAttackTimes(game));
        }

        [Test]
        public void WheelOfPainOfTheLivingAndDeadTest()
        {
            TestGameflow.createGame(out var game, out var you, out var oppo,
                new KeyValuePair<int, int>(WheelOfPainOfTheLivingAndDead.ID, 5)
            );
            game.skipTurnUntil(() => game.getCurrentPlayer() == you && you.getGem() >= 4);
            you.getMaster().gainArmor(game, 30);
            Assert.AreEqual(30, you.getMaster().getArmor(game));
            you.cmdUse(game, you.getHand().getCard<WheelOfPainOfTheLivingAndDead>());
            Assert.AreEqual(0, you.getMaster().getArmor(game));
            Assert.AreEqual(30, you.getMaster().getAttack(game));
        }

        [Test]
        public void FiveSignsOfTheDyingDevaTest()
        {
            TestGameflow.createGame(out var game, out var you, out var oppo,
                new KeyValuePair<int, int>(FiveSignsOfTheDyingDeva.ID, 1)
            );
            game.skipUntilCanUse<FiveSignsOfTheDyingDeva>(you);
            you.cmdUse(game, you.getHand().getCard<FiveSignsOfTheDyingDeva>());
            Assert.AreEqual(5, you.getMaster().getAttack(game));
            Assert.AreEqual(5, you.getMaster().getArmor(game));
        }

        [Test]
        public void ProcessingPhantomsTest()
        {
            TestGameflow.createGame(out var game, out var you, out var oppo,
                new KeyValuePair<int, int>(ProcessingPhantoms.ID, 5)
            );
            game.skipTurnUntil(() => game.getCurrentPlayer() == you && you.getGem() >= 3);
            you.cmdUse(game, you.getHand().First());
            you.cmdTurnEnd(game);
            Assert.AreEqual(0, game.triggers.getRecordedEvents().OfType<HearthStoneRule.CreateTokenEventArg>().Count());
            oppo.cmdTurnEnd(game);
            you.getMaster().gainArmor(game, 5);
            you.cmdTurnEnd(game);
            Assert.IsInstanceOf<Ghost>(you.getField()[you.getField().count - 1].define);
        }

        [Test]
        public void HakuroukenTest()
        {
            TestGameflow.createGame(out var game, out var you, out var oppo,
                new KeyValuePair<int, int>(Hakurouken.ID, 2),
                new KeyValuePair<int, int>(TestWeapon.ID, 1)
            );
            game.skipUntilCanUse<Hakurouken>(you);
            you.cmdUse(game, you.getHand().getCard<Hakurouken>());
            Assert.IsInstanceOf<Hakurouken>(you.getItem().define);

            game.skipUntilCanUse<TestWeapon>(you);
            you.cmdUse(game, you.getHand().getCard<TestWeapon>());
            Assert.IsInstanceOf<TestWeapon>(you.getItem().define);

            game.skipUntilCanUse<Hakurouken>(you);
            Assert.AreEqual(3, you.getItem().getCurrentLife(game));
            you.cmdUse(game, you.getHand().getCard<Hakurouken>());
            Assert.AreEqual(6, you.getItem().getCurrentLife(game));
            Assert.True(!you.getHand().Any(c => c.define is Hakurouken));
            Assert.AreEqual(2, you.getGrave().Where(c => c.define is Hakurouken).Count());
        }
        [Test]
        public void caseCoinRoukankenTest()
        {
            TestGameflow.createGame(out var game, out var you, out var oppo, new HearthstoneOption()
            {
                shuffle = false,
                sortedPlayers = new int[] { 2, 1 }
            }, new KeyValuePair<int, int>(Roukanken.ID, 30));
            game.skipUntilYourTurn(you);
            you.cmdUse(game, you.getHand().getCard<LuckyCoin>());
            you.cmdUse(game, you.getHand().getCard<Roukanken>());
            Assert.True(you.getMaster().canAttack(game));
            Assert.AreEqual(3, you.getMaster().getAttack(game));
            you.cmdAttack(game, you.getMaster(), oppo.getMaster());
            Assert.AreEqual(27, oppo.getMaster().getCurrentLife(game));
            Assert.AreEqual(1, you.getItem().getCurrentLife(game));
        }

        [Test]
        public void HalfPhatomPropChangeEventTest()
        {
            TestGameflow.createGame<HoshigumaYugi, HalfPhantom>(out var game, out var you, out var oppo);
            game.skipUntilCanUse<HalfPhantom>(oppo);
            oppo.getMaster().setArmor(game, 2);
            oppo.cmdUse<HalfPhantom>(game, out var s);
            var yugi = you.getHand().getCard<HoshigumaYugi>();
            Assert.AreEqual(yugi.define.getCost(), yugi.getCost(game) + s.getAttack(game));
            oppo.getMaster().setArmor(game, 4);
            Assert.AreEqual(yugi.define.getCost() - s.getAttack(game), yugi.getCost(game));
        }
    }
}

