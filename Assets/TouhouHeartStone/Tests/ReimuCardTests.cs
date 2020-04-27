using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using System.Linq;
using TouhouHeartstone;

using TouhouHeartstone.Builtin;

namespace Tests
{
    public class HelloWorldTests
    {
        [Test]
        public void newTest()
        {
            Debug.Log("HelloWorld");
        }
    }
    public class ReimuCardTests
    {
        [UnityTest]
        public IEnumerator reimuSkillTest()
        {
            THHGame game = TestGameflow.initStandardGame(null, new int[] { 0, 1 },
                Enumerable.Repeat(new Reimu(), 2).ToArray(),
                Enumerable.Repeat(Enumerable.Repeat(new RashFairy(), 30).ToArray(), 2).ToArray(),
                new GameOption() { });
            game.run();
            yield return new WaitForSeconds(.1f);
            game.sortedPlayers[0].cmdInitReplace(game);
            game.sortedPlayers[1].cmdInitReplace(game);
            yield return new WaitForSeconds(.1f);

            game.sortedPlayers[0].cmdTurnEnd(game);
            yield return new WaitForSeconds(.1f);
            game.sortedPlayers[1].cmdTurnEnd(game);
            yield return new WaitForSeconds(.1f);
            game.sortedPlayers[0].cmdUse(game, game.sortedPlayers[0].skill, 0);
            yield return new WaitForSeconds(.1f);

            Assert.AreEqual(1, game.sortedPlayers[0].field.count);

            game.sortedPlayers[0].cmdTurnEnd(game);
            yield return new WaitForSeconds(.1f);
            game.sortedPlayers[1].cmdTurnEnd(game);
            yield return new WaitForSeconds(.1f);
            game.sortedPlayers[0].cmdUse(game, game.sortedPlayers[0].skill, 0);
            yield return new WaitForSeconds(.1f);

            Assert.AreEqual(2, game.sortedPlayers[0].field.count);

            game.sortedPlayers[0].cmdTurnEnd(game);
            yield return new WaitForSeconds(.1f);
            game.sortedPlayers[1].cmdTurnEnd(game);
            yield return new WaitForSeconds(.1f);
            game.sortedPlayers[0].cmdUse(game, game.sortedPlayers[0].skill, 0);
            yield return new WaitForSeconds(.1f);

            Assert.AreEqual(3, game.sortedPlayers[0].field.count);

            game.sortedPlayers[0].cmdTurnEnd(game);
            yield return new WaitForSeconds(.1f);
            game.sortedPlayers[1].cmdTurnEnd(game);
            yield return new WaitForSeconds(.1f);
            game.sortedPlayers[0].cmdUse(game, game.sortedPlayers[0].skill, 0);
            yield return new WaitForSeconds(.1f);

            Assert.AreEqual(4, game.sortedPlayers[0].field.count);

            Assert.AreEqual(4, game.sortedPlayers[0].field.Select(c => c.define).Distinct().Count());//有4种图腾
        }
    }
}
