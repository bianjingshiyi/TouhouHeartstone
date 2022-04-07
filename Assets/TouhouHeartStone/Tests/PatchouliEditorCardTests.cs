using NUnit.Framework;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TouhouCardEngine;
using TouhouHeartstone;
using UnityEngine.TestTools;
using BJSYGameCore;

namespace Tests
{
    public class PatchouliEditorCardTests
    {
        [UnityTest]
        public IEnumerator LibraryProtectorTest()
        {
            var task = TestGameflow.createGameWithEditorCards(new HearthstoneOption() { shuffle = false },
                TestMaster.ID, new KeyValuePair<int, int>[] { new KeyValuePair<int, int>(-2147418090, 1), new KeyValuePair<int, int>(FireBall.ID, 20) },
                TestMaster.ID, new KeyValuePair<int, int>[0]
            );
            yield return task.wait();
            CardEngine game = task.Result.game;
            Player you = task.Result.you;
            Player oppo = task.Result.oppo;
            game.skipTurnUntil(() => game.getCurrentPlayer() == you && you.getGem() >= 3);
            you.cmdUse(game, you.getHand().getCardByDefine(-2147418090));
            Assert.AreEqual(1 + you.getHand().Where(c => c.isSpell()).Count(), you.getField()[0].getCurrentLife(game));
            game.Dispose();
        }
    }
}

