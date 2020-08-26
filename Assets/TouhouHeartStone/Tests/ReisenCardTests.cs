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
        public void DefaultMysteryTest()
        {
            TestGameflow.createGame(out var game, out var you, out var oppo,
                new KeyValuePair<int, int>(DefaultMystery.ID, 3),
                new KeyValuePair<int, int>(DefaultMystery_zf.ID, 3)
            );

            for(int i=0; i<2; i++)
            {
                game.skipUntilCanUse<DefaultServant>(oppo);
                oppo.cmdUse(game, oppo.hand.getCard<DefaultServant>());
            }

            //单独测试：冰冻、蒸发
            game.skipUntilCanUse<DefaultMystery>(you);
            you.cmdUse(game, you.hand.getCard<DefaultMystery>());
            you.cmdTurnEnd(game);
            oppo.cmdAttack(game, oppo.field[0], you.master);
            oppo.cmdTurnEnd(game);

            game.skipUntilCanUse<DefaultMystery_zf>(you);
            you.cmdUse(game, you.hand.getCard<DefaultMystery_zf>());
            you.cmdTurnEnd(game);
            oppo.cmdAttack(game, oppo.field[0], you.master);
            oppo.cmdTurnEnd(game);

            //组合测试：蒸发、冰冻
            for (int i = 0; i < 2; i++)
            {
                game.skipUntilCanUse<DefaultServant>(oppo);
                oppo.cmdUse(game, oppo.hand.getCard<DefaultServant>());
            }
            game.skipUntilCanUse<DefaultMystery_zf>(you);
            you.cmdUse(game, you.hand.getCard<DefaultMystery_zf>());
            you.cmdUse(game, you.hand.getCard<DefaultMystery>());
            you.cmdTurnEnd(game);
            oppo.cmdAttack(game, oppo.field[0], you.master);
            //断点观察：you头上是否还有冰冻
            oppo.cmdAttack(game, oppo.field[0], you.master);


            //组合测试：冰冻、蒸发
            for (int i = 0; i < 2; i++)
            {
                game.skipUntilCanUse<DefaultServant>(oppo);
                oppo.cmdUse(game, oppo.hand.getCard<DefaultServant>());
            }
            game.skipUntilCanUse<DefaultMystery_zf>(you);
            you.cmdUse(game, you.hand.getCard<DefaultMystery>());
            you.cmdUse(game, you.hand.getCard<DefaultMystery_zf>());
            you.cmdTurnEnd(game);
            oppo.cmdAttack(game, oppo.field[0], you.master);
            //断点观察：you头上是否还有蒸发
            oppo.cmdAttack(game, oppo.field[0], you.master);

            game.Dispose();
        }
    }
}