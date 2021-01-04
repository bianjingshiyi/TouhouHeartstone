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
            THHGame game = TestGameflow.initGameWithoutPlayers(null, new GameOption()
            {
                shuffle = false
            });
            THHPlayer oppo = game.createPlayer(0, "玩家0", game.getDefine<TestMaster2>(),
                Enumerable.Repeat(game.getDefine<DefaultServant>() as CardDefine, 20));
            THHPlayer you = game.createPlayer(1, "玩家1", game.getDefine<Aya>(),
                Enumerable.Repeat(game.getDefine<DefaultServant>() as CardDefine, 20));

            game.skipUntilHasGem(you, 10);
            you.cmdUse(game, you.skill);
            Assert.True(you.hand[0].getCost(game) == 0);
            Assert.True(you.hand[1].getCost(game) == 0);
            Assert.True(you.hand[2].getCost(game) == 0);

            you.cmdUse(game, you.hand.getCard<DefaultServant>());
            Debug.Log("法力：" + you.hand[0].getCost(game));
            Debug.Log("法力：" + you.hand[1].getCost(game));
            Debug.Log("法力：" + you.hand[2].getCost(game));

            game.Dispose();
        }

        [Test]
        public void CrowTest()
        {
            THHGame game = TestGameflow.initGameWithoutPlayers(null, new GameOption()
            {
                shuffle = false
            });
            THHPlayer oppo = game.createPlayer(0, "玩家0", game.getDefine<TestMaster2>(),
                Enumerable.Repeat(game.getDefine<DefaultServant>() as CardDefine, 20));
            THHPlayer you = game.createPlayer(1, "玩家1", game.getDefine<Aya>(),
                Enumerable.Repeat(game.getDefine<Crow>() as CardDefine, 20));

            game.skipUntilCanUse<Crow>(you);
            you.cmdUse(game, you.hand.getCard<Crow>());

            you.field[0].setDead(game, true);
            game.updateDeath(game);
            foreach (Card c in you.hand)
            {
                Debug.Log("法力：" + c.getCost(game));
            }

            game.Dispose();
        }

        [Test]
        public void NewspaperDistributionTest()
        {
            THHGame game = TestGameflow.initGameWithoutPlayers(null, new GameOption()
            {
                shuffle = false
            });
            THHPlayer oppo = game.createPlayer(0, "玩家0", game.getDefine<TestMaster2>(),
                Enumerable.Repeat(game.getDefine<DefaultServant>() as CardDefine, 9));
            THHPlayer you = game.createPlayer(1, "玩家1", game.getDefine<Aya>(),
                Enumerable.Repeat(game.getDefine<NewspaperDistribution>() as CardDefine, 1).Concat(Enumerable.Repeat(game.getDefine<DefaultServant>() as CardDefine, 9)));

            for (int i = 0; i < 5; i++)
            {
                game.skipUntilCanUse<DefaultServant>(oppo);
                oppo.cmdUse(game, oppo.hand.getCard<DefaultServant>());
            }

            game.skipUntilHasGem(you, 8);
            Card ND = you.hand.getCard<NewspaperDistribution>();
            you.cmdUse(game, ND, targets: oppo.field[0]);

            Assert.True(you.hand.Any(u => u == ND));
            Assert.False(ND.isValidTarget(game, oppo.field[0]));
            Assert.True(ND.isValidTarget(game, oppo.field[1]));

            you.cmdUse(game, ND, targets: oppo.field[1]);

            you.cmdTurnEnd(game);
            Assert.True(you.grave.Any(u => u == ND));

            game.Dispose();
        }

        [Test]
        public void ScoopTest()
        {
            THHGame game = TestGameflow.initGameWithoutPlayers(null, new GameOption()
            {
                shuffle = false
            });
            THHPlayer oppo = game.createPlayer(0, "玩家0", game.getDefine<TestMaster2>(),
                Enumerable.Repeat(game.getDefine<DefaultServant>() as CardDefine, 20));
            THHPlayer you = game.createPlayer(1, "玩家1", game.getDefine<Aya>(),
                Enumerable.Repeat(game.getDefine<Scoop>() as CardDefine, 5).Concat(Enumerable.Repeat(game.getDefine<FireBall>() as CardDefine, 5)));

            game.skipUntilHasGem(you, 10);
            you.cmdUse(game, you.hand.getCard<Scoop>());
            Debug.Log("法力：" + you.hand[0].getCost(game));
            Debug.Log("法力：" + you.hand[1].getCost(game));
            Debug.Log("法力：" + you.hand[2].getCost(game));

            you.cmdUse(game, you.hand.getCard<FireBall>(), targets: oppo.master);
            Debug.Log("法力：" + you.hand[0].getCost(game));
            Debug.Log("法力：" + you.hand[1].getCost(game));
            Debug.Log("法力：" + you.hand[2].getCost(game));

            game.Dispose();
        }

        [Test]
        public void LittleAyaTest()
        {
            THHGame game = TestGameflow.initGameWithoutPlayers(null, new GameOption()
            {
                shuffle = false
            });
            THHPlayer oppo = game.createPlayer(0, "玩家0", game.getDefine<TestMaster2>(),
                Enumerable.Repeat(game.getDefine<DefaultServant>() as CardDefine, 20));
            THHPlayer you = game.createPlayer(1, "玩家1", game.getDefine<Aya>(),
                Enumerable.Repeat(game.getDefine<LittleAya>() as CardDefine, 5).Concat(Enumerable.Repeat(game.getDefine<FireBall>() as CardDefine, 5)));

            game.skipUntilHasGem(you, 5);
            you.cmdUse(game, you.skill);
            you.cmdUse(game, you.hand.getCard<LittleAya>());
            you.cmdTurnEnd(game);
            oppo.cmdTurnEnd(game);
            you.cmdUse(game, you.skill);
            you.field[0].setDead(game, true);
            you.cmdTurnEnd(game);
            oppo.cmdTurnEnd(game);
            you.cmdUse(game, you.skill);

            game.Dispose();
        }

        [Test]
        public void BunbunmaruNewsTest()
        {
            THHGame game = TestGameflow.initGameWithoutPlayers(null, new GameOption()
            {
                shuffle = false
            });
            THHPlayer oppo = game.createPlayer(0, "玩家0", game.getDefine<TestMaster2>(),
                Enumerable.Repeat(game.getDefine<DefaultServant>() as CardDefine, 30));
            THHPlayer you = game.createPlayer(1, "玩家1", game.getDefine<Aya>(),
                Enumerable.Repeat(game.getDefine<BunbunmaruNews>() as CardDefine, 30));

            game.skipUntilHasGem(you, 4);
            you.cmdUse(game, you.hand.getCard<BunbunmaruNews>());
            Card c = game.triggers.getRecordedEvents().OfType<THHPlayer.DrawEventArg>().Last().card;
            you.cmdUse(game, c);

            int x = game.triggers.getRecordedEvents().
                        skipUntilLast(e => e is THHGame.TurnStartEventArg TurnStart).
                        OfType<THHPlayer.DrawEventArg>().
                        Where(e2 => e2.player == c.getOwner()).Count();
            Debug.Log("抽牌：" + x);
            game.Dispose();
        }

        [Test]
        public void MapleFansWindTest()
        {
            THHGame game = TestGameflow.initGameWithoutPlayers(null, new GameOption()
            {
                shuffle = false
            });
            THHPlayer oppo = game.createPlayer(0, "玩家0", game.getDefine<TestMaster2>(),
                Enumerable.Repeat(game.getDefine<DefaultServant>() as CardDefine, 9));
            THHPlayer you = game.createPlayer(1, "玩家1", game.getDefine<Aya>(),
                Enumerable.Repeat(game.getDefine<MapleFansWind>() as CardDefine, 1).Concat(Enumerable.Repeat(game.getDefine<DefaultServant>() as CardDefine, 9)));

            for (int i = 0; i < 5; i++)
            {
                game.skipUntilCanUse<DefaultServant>(oppo);
                oppo.cmdUse(game, oppo.hand.getCard<DefaultServant>());
            }

            game.skipUntilHasGem(you, 10);
            you.cmdUse(game, you.hand.getCard<DefaultServant>());
            you.cmdUse(game, you.hand.getCard<DefaultServant>());
            you.cmdUse(game, you.hand.getCard<MapleFansWind>());

            game.Dispose();

        }

        [Test]
        public void WindGodFanTest()
        {
            THHGame game = TestGameflow.initGameWithoutPlayers(null, new GameOption()
            {
                shuffle = false
            });
            THHPlayer oppo = game.createPlayer(0, "玩家0", game.getDefine<TestMaster2>(),
                Enumerable.Repeat(game.getDefine<DefaultServant>() as CardDefine, 10));
            THHPlayer you = game.createPlayer(1, "玩家1", game.getDefine<Aya>(),
                Enumerable.Repeat(game.getDefine<WindGodFan>() as CardDefine, 10));

            game.skipUntilHasGem(you,10);

            you.cmdUse(game, you.skill);
            you.cmdUse(game, you.hand.getCard<WindGodFan>(),targets: oppo.master);
            you.cmdUse(game, you.hand.getCard<WindGodFan>(), targets: oppo.master);

            game.Dispose();
        }

        [Test]
        public void CameraTest()
        {
            THHGame game = TestGameflow.initGameWithoutPlayers(null, new GameOption()
            {
                shuffle = false
            });
            THHPlayer oppo = game.createPlayer(0, "玩家0", game.getDefine<TestMaster2>(),
                Enumerable.Repeat(game.getDefine<DefaultServant>() as CardDefine, 10));
            THHPlayer you = game.createPlayer(1, "玩家1", game.getDefine<Aya>(),
                Enumerable.Repeat(game.getDefine<ReporterCamera>() as CardDefine, 10));

            game.skipUntilCanUse<ReporterCamera>(you);
            you.cmdUse(game, you.hand.getCard<ReporterCamera>());

            oppo.draw(game);
            oppo.draw(game);

            Assert.True(you.hand.Any(e => e.define == game.getDefine<DefaultServant>()));
            game.Dispose();
        }
    }
}

    