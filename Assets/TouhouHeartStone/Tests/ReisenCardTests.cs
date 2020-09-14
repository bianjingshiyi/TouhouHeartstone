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
using static TouhouHeartstone.Builtin.Proble.Probe;

namespace Tests
{
    public class ResisenCardTests
    {
        [Test]
        public void XWavegTest()
        {
            THHGame game = TestGameflow.initGameWithoutPlayers(null, new GameOption()
            {
                shuffle = false
            });
            THHPlayer oppo = game.createPlayer(0, "玩家0", game.getDefine<TestMaster2>(),
                Enumerable.Repeat(game.getDefine<DefaultServant>() as CardDefine, 1));
            THHPlayer you = game.createPlayer(1, "玩家1", game.getDefine<Reisen>(),
                Enumerable.Repeat(game.getDefine<DefaultServant>() as CardDefine, 1).Concat(Enumerable.Repeat(game.getDefine<XWave>(), 1)));

            game.skipUntilCanUse<DefaultServant>(oppo);
            oppo.cmdUse(game, oppo.hand.getCard<DefaultServant>());

            game.skipUntilCanUse<XWave>(you);
            you.cmdUse(game, you.hand.getCard<XWave>());
            you.cmdTurnEnd(game);

            oppo.cmdAttack(game, oppo.field[0], you.master);

            game.Dispose();
        }

        [Test]
        public void MindStopperTest()
        {
            TestGameflow.createGame(out var game, out var you, out var oppo,
                new KeyValuePair<int, int>(MindStopper.ID, 3)
            );
            game.skipUntilCanUse<MindStopper>(you);
            you.cmdUse(game, you.hand.getCard<MindStopper>());
            you.cmdTurnEnd(game);
            Assert.AreEqual(3, oppo.gem);
            oppo.cmdUse(game, oppo.hand.getCard<DefaultServant>());
            Assert.AreEqual(0, oppo.field.count);
            Assert.AreEqual(2, oppo.gem);

        }

        [Test]
        public void WildYoukaiRabbitTest()
        {
            THHGame game = TestGameflow.initGameWithoutPlayers(null, new GameOption()
            {
                shuffle = false
            });
            THHPlayer oppo = game.createPlayer(0, "玩家0", game.getDefine<TestMaster2>(),
                Enumerable.Repeat(game.getDefine<DefaultServant>() as CardDefine, 1));
            THHPlayer you = game.createPlayer(1, "玩家1", game.getDefine<Reisen>(),
                Enumerable.Repeat(game.getDefine<WildYoukaiRabbit>() as CardDefine, 1));

            game.skipTurnUntil(() => game.currentPlayer == you && you.gem >= 4);
            you.cmdUse(game, you.hand.getCard<WildYoukaiRabbit>());
            Assert.AreEqual(1, you.field[0].getAttack(game));
            you.cmdUse(game, you.skill,targets:oppo.master);
            Assert.AreEqual(3, you.field[0].getAttack(game));
        }

        [Test]
        public void MineShakerTest()
        {
            THHGame game = TestGameflow.initGameWithoutPlayers(null, new GameOption()
            {
                shuffle = false
            });
            THHPlayer oppo = game.createPlayer(0, "玩家0", game.getDefine<TestMaster2>(),
                Enumerable.Repeat(game.getDefine<DefaultServant>() as CardDefine, 1));
            THHPlayer you = game.createPlayer(1, "玩家1", game.getDefine<Reisen>(),
                Enumerable.Repeat(game.getDefine<DefaultServant>() as CardDefine, 1).Concat(Enumerable.Repeat(game.getDefine<MindShaker>(), 1)));

            game.skipUntilCanUse<DefaultServant>(oppo);
            oppo.cmdUse(game, oppo.hand.getCard<DefaultServant>());

            game.skipUntilCanUse<MindShaker>(you);
            you.cmdUse(game, you.hand.getCard<MindShaker>());
            you.cmdTurnEnd(game);

            oppo.cmdAttack(game, oppo.field[0], you.master);

            game.Dispose();
        }

        [Test]
        public void DisorderEyeTest()
        {
            THHGame game = TestGameflow.initGameWithoutPlayers(null, new GameOption()
            {
                shuffle = false
            });
            THHPlayer oppo = game.createPlayer(0, "玩家0", game.getDefine<TestMaster2>(),
                Enumerable.Repeat(game.getDefine<DefaultServant>() as CardDefine, 1));
            THHPlayer you = game.createPlayer(1, "玩家1", game.getDefine<Reisen>(),
                Enumerable.Repeat(game.getDefine<DefaultServant>() as CardDefine, 1).Concat(Enumerable.Repeat(game.getDefine<DisorderEye>(), 1)));

            game.skipUntilCanUse<DefaultServant>(oppo);
            oppo.cmdUse(game, oppo.hand.getCard<DefaultServant>());

            game.skipUntilCanUse<DisorderEye>(you);
            you.cmdUse(game, you.hand.getCard<DisorderEye>());
            you.cmdTurnEnd(game);

            oppo.cmdAttack(game, oppo.field[0], you.master);

            game.Dispose();
        }

        [Test]
        public void VisionaryTuningTest()
        {
            THHGame game = TestGameflow.initGameWithoutPlayers(null, new GameOption()
            {
                shuffle = false
            });
            THHPlayer oppo = game.createPlayer(0, "玩家0", game.getDefine<TestMaster2>(),
                Enumerable.Repeat(game.getDefine<DefaultServant>() as CardDefine, 2).Concat(Enumerable.Repeat(game.getDefine<TestFreezeBrain>(), 1)));
            THHPlayer you = game.createPlayer(1, "玩家1", game.getDefine<Reisen>(),
                Enumerable.Repeat(game.getDefine<DefaultServant>() as CardDefine, 2).Concat(Enumerable.Repeat(game.getDefine<VisionaryTuning>(), 1)));
            

            for (int i = 0; i < 2; i++)
            {
                game.skipUntilCanUse<DefaultServant>(you);
                you.cmdUse(game, you.hand.getCard<DefaultServant>());
            }

            game.skipUntilCanUse<VisionaryTuning>(you);
            you.cmdUse(game, you.hand.getCard<VisionaryTuning>());
            you.cmdTurnEnd(game);

            oppo.cmdUse(game, oppo.hand.getCard<TestFreezeBrain>(), targets: you.field[0]);
            Assert.False(you.field[0].isFreeze(game));
            game.Dispose();
        }

        [Test]
        public void FieldUltraVioletTest()
        {
            THHGame game = TestGameflow.initGameWithoutPlayers(null, new GameOption()
            {
                shuffle = false
            });
            THHPlayer oppo = game.createPlayer(0, "玩家0", game.getDefine<TestMaster2>(),
                Enumerable.Repeat(game.getDefine<DefaultServant>() as CardDefine, 1));
            THHPlayer you = game.createPlayer(1, "玩家1", game.getDefine<Reisen>(),
                Enumerable.Repeat(game.getDefine<DefaultServant>() as CardDefine, 1).Concat(Enumerable.Repeat(game.getDefine<FieldUltraViolet>(), 1)));

            game.skipUntilHasGem(you, 3);
            Assert.False(you.skill.isUsed(game));
            you.cmdUse(game, you.skill);
            Assert.True(you.skill.isUsed(game));
            you.cmdUse(game, you.hand.getCard<FieldUltraViolet>());
            Assert.False(you.skill.isUsed(game));
            you.cmdUse(game, you.skill);
            Assert.True(you.skill.isUsed(game));

            game.Dispose();
        }

        [Test]
        public void MindDroppingTest()
        {
            THHGame game = TestGameflow.initGameWithoutPlayers(null, new GameOption()
            {
                shuffle = false
            });
            THHPlayer oppo = game.createPlayer(0, "玩家0", game.getDefine<TestMaster2>(),
                Enumerable.Repeat(game.getDefine<DefaultServant>() as CardDefine, 1));
            THHPlayer you = game.createPlayer(1, "玩家1", game.getDefine<Reisen>(),
                Enumerable.Repeat(game.getDefine<DefaultServant>() as CardDefine, 1).Concat(Enumerable.Repeat(game.getDefine<MindDropping>(), 1)));
            game.skipUntilCanUse<DefaultServant>(oppo);
            Card servant = oppo.hand.getCard<DefaultServant>();
            oppo.cmdUse(game, servant);
            oppo.cmdTurnEnd(game);
            game.skipUntilCanUse<MindDropping>(you);
            you.setGem(game,10);
            you.cmdUse(game, you.skill);
            you.cmdUse(game, you.hand.getCard<MindDropping>());
            Assert.True(you.skill.define is LunaticShoot_MindDropping);
            you.cmdUse(game, you.skill,targets:oppo.field[0]);
            Assert.True(you.skill.define is LunaticShoot);
            Assert.AreEqual(5, oppo.field[0].getCurrentLife(game));

        }

        [Test]
        public void TewiInabaTest()
        {
            THHGame game = TestGameflow.initGameWithoutPlayers(null, new GameOption()
            {
                shuffle = false
            });
            THHPlayer oppo = game.createPlayer(0, "玩家0", game.getDefine<TestMaster2>(),
                Enumerable.Repeat(game.getDefine<DefaultServant>() as CardDefine, 1));
            THHPlayer you = game.createPlayer(1, "玩家1", game.getDefine<Reisen>(),
                Enumerable.Repeat(game.getDefine<FreezingTrap>() as CardDefine, 9).Concat(Enumerable.Repeat(game.getDefine<TewiInaba>(), 1)));

            game.skipUntilCanUse<TewiInaba>(you);
            Assert.AreEqual(0, you.head.count);
            you.cmdUse(game, you.hand.getCard<TewiInaba>());
            Assert.AreEqual(1, you.head.count);
            game.Dispose();
        }

        [Test]
        public void PeerlessPatriotsElixirTest()
        {
            THHGame game = TestGameflow.initGameWithoutPlayers(null, new GameOption()
            {
                shuffle = false
            });
            THHPlayer oppo = game.createPlayer(0, "玩家0", game.getDefine<TestMaster2>(),
                Enumerable.Repeat(game.getDefine<DefaultServant>() as CardDefine, 1));
            THHPlayer you = game.createPlayer(1, "玩家1", game.getDefine<Reisen>(),
                Enumerable.Repeat(game.getDefine<DefaultServant>() as CardDefine, 3).Concat(Enumerable.Repeat(game.getDefine<PeerlessPatriotsElixir>(), 4)));

            game.skipTurnUntil(() => game.currentPlayer == you && you.gem >= 4);
            you.cmdUse(game, you.hand.getCard<DefaultServant>());
            you.cmdUse(game, you.hand.getCard<PeerlessPatriotsElixir>(),targets:you.field[0]);
            game.skipTurnUntil(() => game.currentPlayer == you && you.gem >= 5);
            you.cmdUse(game, you.hand.getCard<DefaultServant>());
            you.cmdUse(game, you.hand.getCard<PeerlessPatriotsElixir>(), targets: you.field[0]);
            you.cmdUse(game, you.hand.getCard<PeerlessPatriotsElixir>(), targets: you.field[0]);
            game.Dispose();
        }

        [Test]
        public void LunaticGunTest()
        {
            THHGame game = TestGameflow.initGameWithoutPlayers(null, new GameOption()
            {
                shuffle = false
            });
            THHPlayer oppo = game.createPlayer(0, "玩家0", game.getDefine<TestMaster2>(),
                Enumerable.Repeat(game.getDefine<DefaultServant>() as CardDefine, 30));
            THHPlayer you = game.createPlayer(1, "玩家1", game.getDefine<Reisen>(),
                Enumerable.Repeat(game.getDefine<FreezingTrap>() as CardDefine, 9).Concat(Enumerable.Repeat(game.getDefine<LunaticGun>(), 1)));

            game.skipUntilCanUse<LunaticGun>(you);          
            you.cmdUse(game, you.hand.getCard<LunaticGun>());
            game.skipUntilCanUse<FreezingTrap>(you);
            you.cmdUse(game, you.hand.getCard<FreezingTrap>());
            game.Dispose();
        }
    }
}