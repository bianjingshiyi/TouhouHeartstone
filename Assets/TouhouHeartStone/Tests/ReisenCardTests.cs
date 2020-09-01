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
            you.cmdUse(game, you.hand.getCard<PowrOverwhelming>(),targets: you.field[0]);
            you.cmdTurnEnd(game);
            game.Dispose();
        }

        [Test]
        public void ReisenXWavegTest()
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
        public void HareMonsterTest()
        {
            THHGame game = TestGameflow.initGameWithoutPlayers(null, new GameOption()
            {
                shuffle = false
            });
            THHPlayer oppo = game.createPlayer(0, "玩家0", game.getDefine<TestMaster2>(),
                Enumerable.Repeat(game.getDefine<DefaultServant>() as CardDefine, 1));
            THHPlayer you = game.createPlayer(1, "玩家1", game.getDefine<Reisen>(),
                Enumerable.Repeat(game.getDefine<HareMonster>() as CardDefine, 1));

            game.skipTurnUntil(() => game.currentPlayer == you && you.gem >= 4);
            you.cmdUse(game, you.hand.getCard<HareMonster>());
            Assert.AreEqual(1, you.field[0].getAttack(game));
            you.cmdUse(game, you.skill,targets:oppo.master);
            Assert.AreEqual(3, you.field[0].getAttack(game));
        }

        [Test]
        public void ReisenMineShakerTest()
        {
            THHGame game = TestGameflow.initGameWithoutPlayers(null, new GameOption()
            {
                shuffle = false
            });
            THHPlayer oppo = game.createPlayer(0, "玩家0", game.getDefine<TestMaster2>(),
                Enumerable.Repeat(game.getDefine<DefaultServant>() as CardDefine, 1));
            THHPlayer you = game.createPlayer(1, "玩家1", game.getDefine<Reisen>(),
                Enumerable.Repeat(game.getDefine<DefaultServant>() as CardDefine, 1).Concat(Enumerable.Repeat(game.getDefine<MineShaker>(), 1)));

            game.skipUntilCanUse<DefaultServant>(oppo);
            oppo.cmdUse(game, oppo.hand.getCard<DefaultServant>());

            game.skipUntilCanUse<MineShaker>(you);
            you.cmdUse(game, you.hand.getCard<MineShaker>());
            you.cmdTurnEnd(game);

            oppo.cmdAttack(game, oppo.field[0], you.master);

            game.Dispose();
        }

        [Test]
        public void ReisenDisorderEyeTest()
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
        public void ReisenVisionaryTuningTest()
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
        public void ReisenMindStarMineTest()
        {
            THHGame game = TestGameflow.initGameWithoutPlayers(null, new GameOption()
            {
                shuffle = false
            });
            THHPlayer oppo = game.createPlayer(0, "玩家0", game.getDefine<TestMaster2>(),
                Enumerable.Repeat(game.getDefine<DefaultServant>() as CardDefine, 1));
            THHPlayer you = game.createPlayer(1, "玩家1", game.getDefine<Reisen>(),
                Enumerable.Repeat(game.getDefine<DefaultServant>() as CardDefine, 1).Concat(Enumerable.Repeat(game.getDefine<MindStarMine>(), 1)));

            game.skipUntilHasGem(you, 3);
            Assert.False(you.skill.isUsed(game));
            you.cmdUse(game, you.skill);
            Assert.True(you.skill.isUsed(game));
            you.cmdUse(game, you.hand.getCard<MindStarMine>());
            Assert.False(you.skill.isUsed(game));
            you.cmdUse(game, you.skill);
            Assert.True(you.skill.isUsed(game));

            game.Dispose();
        }
    }
}