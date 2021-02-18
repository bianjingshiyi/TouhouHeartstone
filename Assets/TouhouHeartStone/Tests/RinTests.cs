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
            you.cmdUse(game,you.skill);
            Assert.AreEqual(1, you.master.getAttack(game));
            Assert.AreEqual(1, you.master.getArmor(game));
            game.Dispose();
        }

        [Test]
        public void ZoombieFairyTest()
        {
            TestGameflow.createGame(out var game, out var you, out var oppo, Rin.ID,
                new KeyValuePair<int, int>(ZoombieFairy.ID, 1));
            game.skipUntilCanUse<ZoombieFairy>(you);
            you.cmdUse<ZoombieFairy>(game, out var servant);
            servant.setDead(game, true);
            game.updateDeath(game);
            Assert.True(you.field[0].define is GhostOfHatred);
            game.Dispose();
        }

        [Test]
        public void HellFireWheelsTest()
        {
            TestGameflow.createGame(out var game, out var you, out var oppo, Rin.ID,
                new KeyValuePair<int, int>(HellFireWheels.ID, 1));
            game.skipUntilCanUse<HellFireWheels>(you);
            game.skipUntilHasGem(you, 10);
            you.cmdUse<DefaultServant>(game, out var s1);
            you.cmdUse<HellFireWheels>(game, out var wheels);
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
            game.updateDeath(game);
            Assert.AreEqual(27, oppo.master.getCurrentLife(game));
            game.Dispose();
        }

        [Test]
        public void HellRainTest()
        {
            TestGameflow.createGame(out var game, out var you, out var oppo, Rin.ID,
                new KeyValuePair<int, int>(HellRain.ID, 1));
            for (int i = 0; i < 2; i++)
            {
                game.skipUntilCanUse<DefaultServant>(you);
                you.cmdUse<DefaultServant>(game, out var servant);
            }

            game.skipUntilCanUse<HellRain>(you);
            you.cmdUse<HellRain>(game);
            foreach (var servant in you.field)
            {
                servant.setDead(game, true);
            }

            game.updateDeath(game);
            Assert.AreEqual(28, oppo.master.getCurrentLife(game));
            game.Dispose();
        }

        [Test]
        public void RebirthTest()
        {
            TestGameflow.createGame(out var game, out var you, out var oppo, Rin.ID,
                new KeyValuePair<int, int>(Rebirth.ID, 1));
            for (int i = 0; i < 2; i++)
            {
                game.skipUntilCanUse<DefaultServant>(you);
                you.cmdUse<DefaultServant>(game, out var servant);
            }

            game.skipUntilCanUse<Rebirth>(you);
            you.cmdUse<Rebirth>(game);
            
            Assert.AreEqual(2,you.field.count);
            Assert.True(you.field[0].define is DefaultServant);
            game.Dispose();
        }

        [Test]
        public void CorpseVoyageTest()
        {
            TestGameflow.createGame(out var game, out var you, out var oppo, Rin.ID,
                new KeyValuePair<int, int>(CorpseVoyage.ID,2), new KeyValuePair<int, int>(ReisenUdongeinInaba_N.ID,1));
            game.skipUntilCanUse<ReisenUdongeinInaba_N>(you);
            you.cmdUse<ReisenUdongeinInaba_N>(game, out var s);
            s.setDead(game, true);
            game.updateDeath(game);
            game.skipUntilCanUse<CorpseVoyage>(you);
            you.cmdUse<CorpseVoyage>(game);
            Assert.AreEqual(0,you.field.count);
            
            game.skipUntilCanUse<DefaultServant>(you);
            you.cmdUse<DefaultServant>(game, out s);
            s.setDead(game, true);
            game.updateDeath(game);
            game.skipUntilCanUse<CorpseVoyage>(you);
            you.cmdUse<CorpseVoyage>(game);
            Assert.AreEqual(1,you.field.count);
            Assert.True(you.field[0].define is DefaultServant);
            
            game.Dispose();
        }
    }
}