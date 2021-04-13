using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using System.Linq;
using TouhouCardEngine;
using TouhouHeartstone;

using TouhouHeartstone.Builtin;

namespace Tests
{
    public class ReimuCardTests
    {
        [Test]
        public void reimuSkillTest()
        {
            CardEngine game = TestGameflow.initStandardGame(null, new int[] { 0, 1 },
                Enumerable.Repeat(new Reimu(), 2).ToArray(),
                Enumerable.Repeat(Enumerable.Repeat(new RashFairy(), 30).ToArray(), 2).ToArray(),
                new HearthstoneOption() { });
            game.run();
            game.getSortedPlayers()[0].cmdInitReplace(game);
            game.getSortedPlayers()[1].cmdInitReplace(game);

            game.getSortedPlayers()[0].cmdTurnEnd(game);
            game.getSortedPlayers()[1].cmdTurnEnd(game);
            game.getSortedPlayers()[0].cmdUse(game, game.getSortedPlayers()[0].getSkill(), 0);

            Assert.AreEqual(1, game.getSortedPlayers()[0].getField().count);

            game.getSortedPlayers()[0].cmdTurnEnd(game);
            game.getSortedPlayers()[1].cmdTurnEnd(game);
            game.getSortedPlayers()[0].cmdUse(game, game.getSortedPlayers()[0].getSkill(), 0);

            Assert.AreEqual(2, game.getSortedPlayers()[0].getField().count);

            game.getSortedPlayers()[0].cmdTurnEnd(game);
            game.getSortedPlayers()[1].cmdTurnEnd(game);
            game.getSortedPlayers()[0].cmdUse(game, game.getSortedPlayers()[0].getSkill(), 0);

            Assert.AreEqual(3, game.getSortedPlayers()[0].getField().count);

            game.getSortedPlayers()[0].cmdTurnEnd(game);
            game.getSortedPlayers()[1].cmdTurnEnd(game);
            game.getSortedPlayers()[0].cmdUse(game, game.getSortedPlayers()[0].getSkill(), 0);

            Assert.AreEqual(4, game.getSortedPlayers()[0].getField().count);

            Assert.AreEqual(4, game.getSortedPlayers()[0].getField().Select(c => c.define).Distinct().Count());//有4种图腾
        }
    }
}
