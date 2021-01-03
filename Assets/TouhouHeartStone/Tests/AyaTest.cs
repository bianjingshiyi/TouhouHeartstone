using NUnit.Framework;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TouhouCardEngine;
using TouhouHeartstone;
using TouhouHeartstone.Builtin.Proble;
using UnityEngine;
using UnityEngine.TestTools;
using System;

namespace Tests
{
    public class AyaCardTest
    {
        [Test]
        public void SpareFilm()
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
    }
}