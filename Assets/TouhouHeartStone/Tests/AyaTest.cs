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

namespace Tests
{
    public class AyaCardTest
    {
        [Test]
        public void SpareFilmTest()
        {
            CardEngine game = TestGameflow.initGameWithoutPlayers(null, new HearthstoneOption()
            {
                shuffle = false
            });
            Player oppo = game.createPlayer(0, "玩家0", game.getDefine<TestMaster2>(),
                Enumerable.Repeat(game.getDefine<DefaultServant>() as CardDefine, 20));
            Player you = game.createPlayer(1, "玩家1", game.getDefine<Aya>(),
                Enumerable.Repeat(game.getDefine<DefaultServant>() as CardDefine, 20));

            game.skipUntilHasGem(you, 10);
            you.cmdUse(game, you.getSkill());
            Assert.True(you.getHand()[0].getCost(game) == 0);
            Assert.True(you.getHand()[1].getCost(game) == 0);
            Assert.True(you.getHand()[2].getCost(game) == 0);

            you.cmdUse(game, you.getHand().getCard<DefaultServant>());
            Debug.Log("法力：" + you.getHand()[0].getCost(game));
            Debug.Log("法力：" + you.getHand()[1].getCost(game));
            Debug.Log("法力：" + you.getHand()[2].getCost(game));

            game.Dispose();
        }

        [Test]
        public void CrowTest()
        {
            CardEngine game = TestGameflow.initGameWithoutPlayers(null, new HearthstoneOption()
            {
                shuffle = false
            });
            Player oppo = game.createPlayer(0, "玩家0", game.getDefine<TestMaster2>(),
                Enumerable.Repeat(game.getDefine<DefaultServant>() as CardDefine, 20));
            Player you = game.createPlayer(1, "玩家1", game.getDefine<Aya>(),
                Enumerable.Repeat(game.getDefine<Crow>() as CardDefine, 20));

            game.skipUntilCanUse<Crow>(you);
            you.cmdUse(game, you.getHand().getCard<Crow>());

            you.getField()[0].setDead(game, true);
            game.updateDeath();
            foreach (Card c in you.getHand())
            {
                Debug.Log("法力：" + c.getCost(game));
            }

            game.Dispose();
        }

        [Test]
        public void NewspaperDistributionTest()
        {
            CardEngine game = TestGameflow.initGameWithoutPlayers(null, new HearthstoneOption()
            {
                shuffle = false
            });
            Player oppo = game.createPlayer(0, "玩家0", game.getDefine<TestMaster2>(),
                Enumerable.Repeat(game.getDefine<DefaultServant>() as CardDefine, 9));
            Player you = game.createPlayer(1, "玩家1", game.getDefine<Aya>(),
                Enumerable.Repeat(game.getDefine<NewspaperDistribution>() as CardDefine, 1).Concat(Enumerable.Repeat(game.getDefine<DefaultServant>() as CardDefine, 9)));

            for (int i = 0; i < 5; i++)
            {
                game.skipUntilCanUse<DefaultServant>(oppo);
                oppo.cmdUse(game, oppo.getHand().getCard<DefaultServant>());
            }

            game.skipUntilHasGem(you, 8);
            Card ND = you.getHand().getCard<NewspaperDistribution>();
            you.cmdUse(game, ND, targets: oppo.getField()[0]);

            Assert.True(you.getHand().Any(u => u == ND));
            Assert.False(ND.isValidTarget(game, oppo.getField()[0]));
            Assert.True(ND.isValidTarget(game, oppo.getField()[1]));

            you.cmdUse(game, ND, targets: oppo.getField()[1]);

            you.cmdTurnEnd(game);
            Assert.True(you.getGrave().Any(u => u == ND));

            game.Dispose();
        }

        [Test]
        public void ScoopTest()
        {
            CardEngine game = TestGameflow.initGameWithoutPlayers(null, new HearthstoneOption()
            {
                shuffle = false
            });
            Player oppo = game.createPlayer(0, "玩家0", game.getDefine<TestMaster2>(),
                Enumerable.Repeat(game.getDefine<DefaultServant>() as CardDefine, 20));
            Player you = game.createPlayer(1, "玩家1", game.getDefine<Aya>(),
                Enumerable.Repeat(game.getDefine<Scoop>() as CardDefine, 5).Concat(Enumerable.Repeat(game.getDefine<FireBall>() as CardDefine, 5)));

            game.skipUntilHasGem(you, 10);
            you.cmdUse(game, you.getHand().getCard<Scoop>());
            Debug.Log("法力：" + you.getHand()[0].getCost(game));
            Debug.Log("法力：" + you.getHand()[1].getCost(game));
            Debug.Log("法力：" + you.getHand()[2].getCost(game));

            you.cmdUse(game, you.getHand().getCard<FireBall>(), targets: oppo.getMaster());
            Debug.Log("法力：" + you.getHand()[0].getCost(game));
            Debug.Log("法力：" + you.getHand()[1].getCost(game));
            Debug.Log("法力：" + you.getHand()[2].getCost(game));

            game.Dispose();
        }

        [Test]
        public void LittleAyaTest()
        {
            CardEngine game = TestGameflow.initGameWithoutPlayers(null, new HearthstoneOption()
            {
                shuffle = false
            });
            Player oppo = game.createPlayer(0, "玩家0", game.getDefine<TestMaster2>(),
                Enumerable.Repeat(game.getDefine<DefaultServant>() as CardDefine, 20));
            Player you = game.createPlayer(1, "玩家1", game.getDefine<Aya>(),
                Enumerable.Repeat(game.getDefine<LittleAya>() as CardDefine, 5).Concat(Enumerable.Repeat(game.getDefine<FireBall>() as CardDefine, 5)));

            game.skipUntilHasGem(you, 5);
            you.cmdUse(game, you.getSkill());
            you.cmdUse(game, you.getHand().getCard<LittleAya>());
            you.cmdTurnEnd(game);
            oppo.cmdTurnEnd(game);
            you.cmdUse(game, you.getSkill());
            you.getField()[0].setDead(game, true);
            you.cmdTurnEnd(game);
            oppo.cmdTurnEnd(game);
            you.cmdUse(game, you.getSkill());

            game.Dispose();
        }

        [Test]
        public void BunbunmaruNewsTest()
        {
            CardEngine game = TestGameflow.initGameWithoutPlayers(null, new HearthstoneOption()
            {
                shuffle = false
            });
            Player oppo = game.createPlayer(0, "玩家0", game.getDefine<TestMaster2>(),
                Enumerable.Repeat(game.getDefine<DefaultServant>() as CardDefine, 30));
            Player you = game.createPlayer(1, "玩家1", game.getDefine<Aya>(),
                Enumerable.Repeat(game.getDefine<BunbunmaruNews>() as CardDefine, 30));

            game.skipUntilHasGem(you, 4);
            you.cmdUse(game, you.getHand().getCard<BunbunmaruNews>());
            Card c = game.triggers.getRecordedEvents().OfType<HearthStoneRule.DrawEventArg>().Last().card;
            you.cmdUse(game, c);

            int x = game.triggers.getRecordedEvents().
                        skipUntilLast(e => e is HearthStoneRule.TurnStartEventArg TurnStart).
                        OfType<HearthStoneRule.DrawEventArg>().
                        Where(e2 => e2.player == c.getOwner()).Count();
            Debug.Log("抽牌：" + x);
            game.Dispose();
        }

        [Test]
        public void MapleFansWindTest()
        {
            CardEngine game = TestGameflow.initGameWithoutPlayers(null, new HearthstoneOption()
            {
                shuffle = false
            });
            Player oppo = game.createPlayer(0, "玩家0", game.getDefine<TestMaster2>(),
                Enumerable.Repeat(game.getDefine<DefaultServant>() as CardDefine, 9));
            Player you = game.createPlayer(1, "玩家1", game.getDefine<Aya>(),
                Enumerable.Repeat(game.getDefine<MapleFansWind>() as CardDefine, 1).Concat(Enumerable.Repeat(game.getDefine<DefaultServant>() as CardDefine, 9)));

            for (int i = 0; i < 5; i++)
            {
                game.skipUntilCanUse<DefaultServant>(oppo);
                oppo.cmdUse(game, oppo.getHand().getCard<DefaultServant>());
            }

            game.skipUntilHasGem(you, 10);
            you.cmdUse(game, you.getHand().getCard<DefaultServant>());
            you.cmdUse(game, you.getHand().getCard<DefaultServant>());
            you.cmdUse(game, you.getHand().getCard<MapleFansWind>());

            game.Dispose();

        }

        [Test]
        public void WindGodFanTest()
        {
            CardEngine game = TestGameflow.initGameWithoutPlayers(null, new HearthstoneOption()
            {
                shuffle = false
            });
            Player oppo = game.createPlayer(0, "玩家0", game.getDefine<TestMaster2>(),
                Enumerable.Repeat(game.getDefine<DefaultServant>() as CardDefine, 10));
            Player you = game.createPlayer(1, "玩家1", game.getDefine<Aya>(),
                Enumerable.Repeat(game.getDefine<WindGodFan>() as CardDefine, 10));

            game.skipUntilHasGem(you, 10);

            you.cmdUse(game, you.getSkill());
            you.cmdUse(game, you.getHand().getCard<WindGodFan>(), targets: oppo.getMaster());
            you.cmdUse(game, you.getHand().getCard<WindGodFan>(), targets: oppo.getMaster());

            game.Dispose();
        }

        [Test]
        public void CameraTest()
        {
            CardEngine game = TestGameflow.initGameWithoutPlayers(null, new HearthstoneOption()
            {
                shuffle = false
            });
            Player oppo = game.createPlayer(0, "玩家0", game.getDefine<TestMaster2>(),
                Enumerable.Repeat(game.getDefine<DefaultServant>() as CardDefine, 10));
            Player you = game.createPlayer(1, "玩家1", game.getDefine<Aya>(),
                Enumerable.Repeat(game.getDefine<ReporterCamera>() as CardDefine, 10));

            game.skipUntilCanUse<ReporterCamera>(you);
            you.cmdUse(game, you.getHand().getCard<ReporterCamera>());

            oppo.draw(game);
            oppo.draw(game);

            Assert.True(you.getHand().Any(e => e.define == game.getDefine<DefaultServant>()));
            game.Dispose();
        }

        [Test]
        public void HimekaidouHatateTest()
        {
            CardEngine game = TestGameflow.initGameWithoutPlayers(null, new HearthstoneOption()
            {
                shuffle = false
            });
            Player oppo = game.createPlayer(0, "玩家0", game.getDefine<TestMaster2>(),
                Enumerable.Repeat(game.getDefine<DefaultServant>() as CardDefine, 10));
            Player you = game.createPlayer(1, "玩家1", game.getDefine<Aya>(),
                Enumerable.Repeat(game.getDefine<HimekaidouHatate>() as CardDefine, 20));

            game.skipUntilCanUse<HimekaidouHatate>(you);
            you.cmdUse(game, you.getHand().getCard<HimekaidouHatate>());
            game.skipUntilCanUse<HimekaidouHatate>(you);
            you.cmdUse(game, you.getSkill());
            you.cmdUse(game, you.getHand().getCard<HimekaidouHatate>());

            game.Dispose();
        }
    }
}

    