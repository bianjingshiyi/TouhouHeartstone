using NUnit.Framework;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TouhouCardEngine;
using TouhouHeartstone;
using TouhouHeartstone.Builtin;
using UnityEngine;
using UnityEngine.TestTools;
using System;
using System.Threading.Tasks;
using UI;
using Card = TouhouCardEngine.Card;


namespace Tests
{
    public class RinTests
    {
        [Test]
        public void NekomatarizeTest()
        {
            TestGameflow.createGame(out var game, out var you, out var oppo, Rin.ID);
            game.skipUntilCanUse<Nekomatarize>(you);
            you.cmdUse(game, you.getSkill());
            Assert.AreEqual(1, you.getMaster().getAttack(game));
            Assert.AreEqual(1, you.getMaster().getArmor(game));
            game.Dispose();
        }

        [Test]
        public void ZombieFairyTest()
        {
            TestGameflow.createGame(out var game, out var you, out var oppo, Rin.ID,
                new KeyValuePair<int, int>(ZombieFairy.ID, 1));
            game.skipUntilCanUse<ZombieFairy>(you);
            you.cmdUse<ZombieFairy>(game, out var servant);
            servant.setDead(game, true);
            game.updateGameState();
            Assert.True(you.getField()[0].define is GhostOfHatred);
            game.Dispose();
        }

        [Test]
        public void BlazingWheelTest()
        {
            TestGameflow.createGame(out var game, out var you, out var oppo, Rin.ID,
                new KeyValuePair<int, int>(BlazingWheel.ID, 1));
            game.skipUntilCanUse<BlazingWheel>(you);
            game.skipUntilHasGem(you, 10);
            you.cmdUse<DefaultServant>(game, out var s1);
            you.cmdUse<BlazingWheel>(game, out var wheels);
            you.cmdUse<DefaultServant>(game, out var s2);
            Assert.True(s1.canAttack(game));
            Assert.True(wheels.canAttack(game));
            Assert.True(s2.canAttack(game));
            game.Dispose();
        }

        [Test]
        public void GhostFireTest()
        {
            TestGameflow.createGame(out var game, out var you, out var oppo, Rin.ID,
                new KeyValuePair<int, int>(GhostFire.ID, 1));
            game.skipUntilCanUse<GhostFire>(you);
            you.cmdUse<GhostFire>(game, out var s);
            s.setDead(game, true);
            game.updateGameState();
            Assert.AreEqual(28, oppo.getMaster().getCurrentLife(game));
            game.Dispose();
        }

        [Test]
        public void NeedleMountainOfFormerHellTest()
        {
            TestGameflow.createGame(out var game, out var you, out var oppo, Rin.ID,
                new KeyValuePair<int, int>(NeedleMountainOfFormerHell.ID, 1));
            for (int i = 0; i < 2; i++)
            {
                game.skipUntilCanUse<DefaultServant>(you);
                you.cmdUse<DefaultServant>(game, out var servant);
            }

            game.skipUntilCanUse<NeedleMountainOfFormerHell>(you);
            you.cmdUse<NeedleMountainOfFormerHell>(game);
            foreach (var servant in you.getField())
            {
                servant.setDead(game, true);
            }

            game.updateGameState();
            Assert.AreEqual(28, oppo.getMaster().getCurrentLife(game));
            game.Dispose();
        }

        [Test]
        public void RekindlingOfDeadAshesTest()
        {
            TestGameflow.createGame(out var game, out var you, out var oppo, Rin.ID,
                new KeyValuePair<int, int>(RekindlingOfDeadAshes.ID, 1));
            for (int i = 0; i < 2; i++)
            {
                game.skipUntilCanUse<DefaultServant>(you);
                you.cmdUse<DefaultServant>(game, out var servant);
            }

            game.skipUntilCanUse<RekindlingOfDeadAshes>(you);
            you.cmdUse<RekindlingOfDeadAshes>(game);

            Assert.AreEqual(2, you.getField().count);
            Assert.True(you.getField()[0].define is DefaultServant);
            game.Dispose();
        }
        
        [Test]
        public void RekindlingOfDeadAshesPassiveEffectTest()
        {
            TestGameflow.createGame<CorpseTruck, DefaultServant>(out var game, out var you, out var oppo,
                new KeyValuePair<int, int>(RekindlingOfDeadAshes.ID, 2));
            for (int i = 0; i < 2; i++)
            {
                game.skipUntilCanUse<CorpseTruck>(you);
                you.cmdUse<CorpseTruck>(game, out var servant);
            }
            you.getMaster().addModifier(game, new AttackModifier(1));
            you.cmdAttack(game, you.getMaster(), oppo.getMaster());
            game.skipUntilCanUse<RekindlingOfDeadAshes>(you);
            you.cmdUse<RekindlingOfDeadAshes>(game);
            game.skipUntilCanUse<RekindlingOfDeadAshes>(you);
            you.cmdUse<RekindlingOfDeadAshes>(game);
            int count = you.getHand().count;
            you.cmdAttack(game, you.getMaster(), oppo.getMaster());
            Assert.AreEqual((count + 2 > 10) ? 10 : count + 2, you.getHand().count);

            Assert.AreEqual(2, you.getField().count);
            game.Dispose();
        }

        [Test]
        public void CorpseVoyageTest()
        {
            TestGameflow.createGame(out var game, out var you, out var oppo, Rin.ID,
                new KeyValuePair<int, int>(CorpseVoyage.ID, 2),
                new KeyValuePair<int, int>(ReisenUdongeinInaba_N.ID, 1));
            game.skipUntilCanUse<ReisenUdongeinInaba_N>(you);
            you.cmdUse<ReisenUdongeinInaba_N>(game, out var s);
            s.setDead(game, true);
            game.updateGameState();
            Assert.AreEqual(1, you.getGrave().count);
            Assert.True(you.getGrave()[0].define is ReisenUdongeinInaba_N);
            game.skipUntilCanUse<CorpseVoyage>(you);
            you.cmdUse<CorpseVoyage>(game);
            Assert.AreEqual(0, you.getField().count);
            Assert.AreEqual(1, you.getGrave().Count(c => c.isServant()));
            game.skipUntilCanUse<DefaultServant>(you);
            you.cmdUse<DefaultServant>(game, out s);
            s.setDead(game, true);
            game.updateGameState();
            game.skipUntilCanUse<CorpseVoyage>(you);
            you.cmdUse<CorpseVoyage>(game);
            Assert.AreEqual(1, you.getField().count);
            Assert.True(you.getField()[0].define is DefaultServant);
            Assert.AreEqual(1, you.getGrave().Count(c => c.isServant()));
            Assert.AreEqual(0, you.getGrave().Count(c => c.getCost(game) <= 3 && c.hasKeyword(game, KEYWORD.DEATHRATTLE)));
            game.Dispose();
        }

        [Test]
        public void VengefulCannibalSpiritTest()
        {
            TestGameflow.createGame(out var game, out var you, out var oppo, Rin.ID,
                new KeyValuePair<int, int>(VengefulCannibalSpirit.ID, 2));
            game.skipUntilCanUse<DefaultServant>(you);
            you.cmdUse<DefaultServant>(game, out var s1);
            game.skipUntilCanUse<VengefulCannibalSpirit>(you);
            you.cmdUse<VengefulCannibalSpirit>(game, out var s2, 0, s1);
            Assert.AreEqual(1, you.getField().count);
            s2.setDead(game, true);
            game.updateGameState();
            Assert.AreEqual(2, you.getField().count);
            Assert.True(you.getField()[0].define is GhostOfHatred);
            game.Dispose();
        }

        [Test]
        public void CoffinCarrierTest()
        {
            TestGameflow.createGame(out var game, out var you, out var oppo, Rin.ID,
                new KeyValuePair<int, int>(CoffinCarrier.ID, 20));
            game.skipUntilCanUse<DefaultServant>(oppo);
            oppo.cmdUse<DefaultServant>(game);
            game.skipUntilCanUse<CoffinCarrier>(you);
            you.cmdUse<CoffinCarrier>(game, out var s);
            Assert.True(s.canAttack(game));
            int count = you.getHand().count;
            s.setDead(game, true);
            game.updateGameState();
            Assert.AreEqual(count + 1, you.getHand().count);
            Assert.True(you.getHand()[count].hasKeyword(game, KEYWORD.DEATHRATTLE));
            game.Dispose();
        }

        [Test]
        public void CatWalkTest()
        {
            TestGameflow.createGame(out var game, out var you, out var oppo, Rin.ID,
                new KeyValuePair<int, int>(CatWalk.ID, 2));
            game.skipUntilCanUse<DefaultServant>(oppo);
            oppo.cmdUse<DefaultServant>(game, out var servant);
            servant.setCurrentLife(game, 3);
            game.skipUntilCanUse<CatWalk>(you);
            you.cmdUse<CatWalk>(game);
            Assert.AreEqual(3, you.getMaster().getAttack(game));
            Assert.True(you.getMaster().canAttack(game));
            you.cmdAttack(game, you.getMaster(), servant);
            Assert.AreEqual(0, oppo.getField().count);
            Assert.True(you.getMaster().canAttack(game));
            you.cmdAttack(game, you.getMaster(), oppo.getMaster());
            Assert.False(you.getMaster().canAttack(game));
            game.Dispose();
        }

        [Test]
        public void DoubleCatWalkTest()
        {
            TestGameflow.createGame(out var game, out var you, out var oppo, Rin.ID,
                new KeyValuePair<int, int>(CatWalk.ID, 2));
            game.skipUntilCanUse<DefaultServant>(oppo);
            oppo.cmdUse<DefaultServant>(game, out var servant);
            servant.setCurrentLife(game, 3);
            game.skipTurnUntil(() => you.getHand().Count(c => c.define is CatWalk) == 2 && you.getGem() > 4);
            for (int i = 0; i < 2; i++)
            {
                you.cmdUse<CatWalk>(game);
            }
            Assert.AreEqual(1, oppo.getField().count);
            Assert.AreEqual(6, you.getMaster().getAttack(game));
            Assert.True(you.getMaster().canAttack(game));
            you.cmdAttack(game, you.getMaster(), oppo.getField()[0]);
            Assert.AreEqual(0, oppo.getField().count);
            Assert.True(you.getMaster().canAttack(game));
            you.cmdAttack(game, you.getMaster(), oppo.getMaster());
            Assert.True(you.getMaster().canAttack(game));
            you.cmdAttack(game, you.getMaster(), oppo.getMaster());
            Assert.False(you.getMaster().canAttack(game));
            game.Dispose();
        }


        [Test]
        public void GhostTownTest()
        {
            TestGameflow.createGame(out var game, out var you, out var oppo, Rin.ID,
                new KeyValuePair<int, int>(GhostTown.ID, 1), new KeyValuePair<int, int>(ZombieFairy.ID, 1));
            game.skipUntilCanUse<ZombieFairy>(you);
            you.cmdUse<ZombieFairy>(game, out var fairy);
            fairy.setDead(game, true);
            game.updateGameState();
            Assert.AreEqual(1, you.getGrave().count);
            game.skipUntilCanUse<GhostTown>(you);
            int handCount = you.getHand().count;
            you.cmdUse<GhostTown>(game);
            DiscoverRequest discoverRequest = game.answers.getRequest<DiscoverRequest>(you.id);
            Assert.NotNull(discoverRequest);
            int id = discoverRequest.cardIdArray[0];
            you.cmdDiscover(game, id);
            Assert.AreEqual(handCount, you.getHand().count);
            Assert.True(you.getHand()[handCount - 1].define is ZombieFairy);
            Assert.AreEqual(2, you.getField().count);
            Assert.AreEqual(2, you.getField().Count(c => c.define is GhostOfHatred));
            game.Dispose();
        }

        [Test]
        public void CorpseTruckTest()
        {
            TestGameflow.createGame(out var game, out var you, out var oppo, Rin.ID,
                new KeyValuePair<int, int>(CorpseTruck.ID, 2));
            game.skipUntilCanUse<CorpseTruck>(you);
            game.skipUntilHasGem(you, 10);
            you.cmdUse<CorpseTruck>(game, out var card);
            int handCount = you.getHand().count;
            you.cmdUse(game, you.getSkill());
            you.cmdAttack(game, you.getMaster(), oppo.getMaster());
            Assert.AreEqual(handCount + 1, you.getHand().count);
            you.cmdUse<CorpseTruck>(game);
            you.cmdUse<DefaultServant>(game);
            handCount = you.getHand().count;
            Assert.AreEqual(2, you.getField().Count(c => c.define is CorpseTruck));
            you.cmdTurnEnd(game);
            oppo.cmdTurnEnd(game);
            you.cmdUse<DefaultServant>(game);
            you.cmdUse(game, you.getSkill());
            you.cmdAttack(game, you.getMaster(), oppo.getMaster());
            Assert.AreEqual(handCount + 2, you.getHand().count);
            game.Dispose();
        }

        [Test]
        public void SpleenEaterTest()
        {
            TestGameflow.createGame(out var game, out var you, out var oppo, Rin.ID,
                new KeyValuePair<int, int>(SpleenEater.ID, 1));
            game.skipUntilCanUse<SpleenEater>(you);
            you.cmdUse<SpleenEater>(game);
            game.skipUntilHasGem(you, 10);
            int handCount = you.getHand().count;
            you.cmdUse<DefaultServant>(game, out var card);
            card.setDead(game, true);
            game.updateGameState();
            Assert.AreEqual(handCount, you.getHand().count);
            Assert.True(you.getHand()[handCount - 1].define is GhostOfHatred);

            you.getField()[0].setDead(game, true);
            game.updateGameState();
            you.cmdUse<DefaultServant>(game, out card);
            card.setDead(game, true);
            game.updateGameState();
            Assert.AreEqual(handCount - 1, you.getHand().count);

            game.Dispose();
        }

        [Test]
        public void CorpseShoppingDistrictTest()
        {
            TestGameflow.createGame(out var game, out var you, out var oppo, Rin.ID,
                new KeyValuePair<int, int>(CorpseShoppingDistrict.ID, 2),
                new KeyValuePair<int, int>(ZombieFairy.ID, 1));
            game.skipUntilCanUse<DefaultServant>(you);
            you.cmdUse<DefaultServant>(game, out var s);
            s.setDead(game, true);
            game.updateGameState();
            //此时墓地有一个白板，场上无随从
            Assert.AreEqual(1, you.getGrave().count);
            Assert.True(you.getGrave()[0].define is DefaultServant);
            game.skipUntilCanUse<CorpseShoppingDistrict>(you);
            you.cmdUse<CorpseShoppingDistrict>(game);
            //墓地中的白板没有被尸体街复活
            Assert.AreEqual(0, you.getField().count);
            Assert.AreEqual(1, you.getGrave().Count(c => c.isServant()));
            game.skipUntilCanUse<ZombieFairy>(you);
            you.cmdUse<ZombieFairy>(game, out s);
            s.setDead(game, true);
            game.updateGameState();
            //此时场上有一个由丧失妖精召唤的怨灵
            Assert.AreEqual(1, you.getField().count);
            game.skipUntilCanUse<CorpseShoppingDistrict>(you);
            you.cmdUse<CorpseShoppingDistrict>(game);
            //丧失妖精复活，场上只有一个怨灵
            Assert.AreEqual(2, you.getField().count);
            Assert.AreEqual(1, you.getField().Count(c => c.define is GhostOfHatred));
            //白板仍未被复活，且墓地中无亡语随从
            Assert.AreEqual(1, you.getGrave().Count(c => c.isServant()));
            Assert.AreEqual(0,
                you.getGrave().Count(c => c.isServant() && c.hasKeyword(game, KEYWORD.DEATHRATTLE)));
            you.cmdTurnEnd(game);
            game.skipUntilYourTurn(you);
            //丧失妖精死亡，召唤一只怨灵
            Assert.AreEqual(2, you.getField().count);
            Assert.AreEqual(2, you.getField().Count(c => c.define is GhostOfHatred));
            Debug.Log("测试完成");
            game.Dispose();
        }

        //[Test]
        //public void CatRandomWalkTest()
        //{
        //    TestGameflow.createGame(out var game, out var you, out var oppo, Rin.ID,
        //        new KeyValuePair<int, int>(CatRandomWalk.ID, 2),
        //        new KeyValuePair<int, int>(FireBall.ID, 20));
        //    game.skipUntilCanUse<CatRandomWalk>(you);
        //    you.cmdUse<CatRandomWalk>(game);
        //    Assert.AreEqual(4, you.getGrave().count);
        //    Assert.AreEqual(3, you.getGrave().Count(c => c.define is DefaultServant));
        //    Assert.AreEqual(3, you.getMaster().getAttack(game));
        //    game.Dispose();
        //}

        [Test]
        public void HellTrafficAccidentTest()
        {
            TestGameflow.createGame(out var game, out var you, out var oppo, Rin.ID,
                new KeyValuePair<int, int>(HellTrafficAccident.ID, 2));
            game.skipUntilCanUse<HellTrafficAccident>(you);
            you.cmdUse<HellTrafficAccident>(game);
            Assert.AreEqual(0, oppo.getGrave().count);
            Assert.AreEqual(1, you.getGrave().count);
            for (int i = 0; i < 3; i++)
            {
                game.skipUntilCanUse<DefaultServant>(oppo);
                oppo.cmdUse<DefaultServant>(game, out var s);
                s.setDead(game, true);
            }

            game.updateGameState();
            Assert.AreEqual(3, oppo.getGrave().count);
            game.skipUntilCanUse<HellTrafficAccident>(you);
            you.cmdUse<HellTrafficAccident>(game);
            Assert.AreEqual(5, you.getGrave().count);
            Assert.AreEqual(you, you.getGrave()[0].getOwner());
            game.Dispose();
        }
        
        [Test]
        public void GhostTownCoffinCarrierBlazingWheelTest()
        {
            TestGameflow.createGame(out var game, out var you, out var oppo, Rin.ID,
                new KeyValuePair<int, int>(BlazingWheel.ID, 1), new KeyValuePair<int, int>(GhostTown.ID,1), new KeyValuePair<int, int>(CoffinCarrier.ID,1));
            game.skipUntilCanUse<BlazingWheel>(you);
            you.cmdUse<BlazingWheel>(game);
            game.skipUntilCanUse<CoffinCarrier>(you);
            you.cmdUse<CoffinCarrier>(game, out var carrier);
            carrier.setDead(game, true);
            game.updateGameState();
            game.skipUntilCanUse<GhostTown>(you);
            you.cmdUse<GhostTown>(game);
            DiscoverRequest discoverRequest = game.answers.getRequest<DiscoverRequest>(you.id);
            Assert.NotNull(discoverRequest);
            int id = discoverRequest.cardIdArray[0];
            you.cmdDiscover(game, id);
            Assert.True(you.getHand().Contains(carrier));
            game.skipUntilCanUse<CoffinCarrier>(you);
            you.cmdUse(game,carrier);
            Assert.True(carrier.hasKeyword(game,KEYWORD.CHARGE));
            game.Dispose();
        }
    }
}