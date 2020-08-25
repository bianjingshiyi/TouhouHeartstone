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
        public void ResisenAnalepticTest()
        {
            THHGame game = TestGameflow.initGameWithoutPlayers(null, new GameOption()
            {
                shuffle = false
            });
            THHPlayer oppo = game.createPlayer(0, "玩家0", game.getDefine<TestMaster2>(),
                Enumerable.Repeat(game.getDefine<DefaultServant>() as CardDefine, 30));
            THHPlayer you = game.createPlayer(1, "玩家1", game.getDefine<Reisen>(),
                Enumerable.Repeat(game.getDefine<DefaultServant>() as CardDefine, 27).Concat(Enumerable.Repeat(game.getDefine<Analeptic>(), 3)));

            game.skipUntilHasGem(you, 2);
            you.cmdUse(game, you.skill);

            game.skipUntilHasGem(you, 8);
            you.cmdUse(game, you.skill);
            you.cmdUse(game, you.hand.getCard<Analeptic>());

            you.cmdUse(game, you.skill);
            you.cmdUse(game, you.hand.getCard<Analeptic>());

            you.cmdUse(game, you.skill);
            you.cmdUse(game, you.hand.getCard<Analeptic>());

            you.cmdUse(game, you.skill);
            game.Dispose();
        }

        [Test]
        public void ResisenMarinTest()
        {
            THHGame game = TestGameflow.initGameWithoutPlayers(null, new GameOption()
            {
                shuffle = false
            });
            THHPlayer oppo = game.createPlayer(0, "玩家0", game.getDefine<TestMaster2>(),
                Enumerable.Repeat(game.getDefine<DefaultServant>() as CardDefine, 30));
            THHPlayer you = game.createPlayer(1, "玩家1", game.getDefine<Reisen>(),
                Enumerable.Repeat(game.getDefine<DefaultServant>() as CardDefine, 27).Concat(Enumerable.Repeat(game.getDefine<Marin>(), 3)));

           
            game.skipUntilHasGem(you, 4);
            you.cmdUse(game, you.hand.getCard<Marin>());

            you.cmdUse(game, you.skill);
            
            game.Dispose();
        }
    }
}