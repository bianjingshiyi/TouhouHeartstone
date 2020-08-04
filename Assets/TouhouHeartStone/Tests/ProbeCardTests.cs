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
    public class ProbeCardTests
    {
        //粒子光束测试
        [Test]
        public void ProbeParticulateLaserTest()
        {
            TestGameflow.createGame(out var game, out var you, out var oppo,
                new KeyValuePair<int, int>(ParticulateLaser.ID, 3)
            );
            game.skipTurnUntil(() => game.currentPlayer == you && you.gem >= game.getCardDefine<ParticulateLaser>().cost);
            you.cmdUse(game, you.hand.getCard<ParticulateLaser>(), targets: oppo.master);
            game.Dispose();
        }

        // 召唤水晶塔测试
        [Test]
        public void ProbeBulidPylonTest(){
            TestGameflow.createGame(out var game, out var you, out var oppo,
                new KeyValuePair<int, int>(BulidPylon.ID, 1)
            );
            // 快进到可以使用此卡
            game.skipUntilCanUse<BulidPylon>(you);
            // 我方使用召唤水晶塔
            you.cmdUse(game, you.hand.getCard<BulidPylon>());
            // 回合结束
            you.cmdTurnEnd(game);
            // 快进到你的回合
            //game.skipTurnUntil(() => game.currentPlayer == you);
            game.Dispose();
        }

        // 水晶塔过载测试
        [Test]
        public void ProbePylonOverloadTest(){
            TestGameflow.createGame(out var game, out var you, out var oppo,
                new KeyValuePair<int, int>(BulidPylon.ID, 1),
                new KeyValuePair<int, int>(PylonOverload.ID, 1)
            );
            game.skipUntilCanUse<BulidPylon>(you);
            // 我方使用召唤水晶塔
            you.cmdUse(game, you.hand.getCard<BulidPylon>());
            // 回合结束
            you.cmdTurnEnd(game);    
            game.skipUntilCanUse<PylonOverload>(you);
            you.cmdUse(game, you.hand.getCard<PylonOverload>());
            Assert.True(you.field[0].canAttack(game));
            Assert.True(you.field[1].canAttack(game));
            game.Dispose();
        }

    }
    
}