using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using System.Linq;
using TouhouHeartstone;

using TouhouHeartstone.Builtin;

namespace Tests
{
    public class SanaeCardTests
    {
        [UnityTest]
        public IEnumerator sanaeSkillTest()
        {
            THHGame game = TestGameflow.initStandardGame(null, new int[] { 0, 1 },
                Enumerable.Repeat(new Sanae(), 2).ToArray(),
                Enumerable.Repeat(Enumerable.Repeat(new RashFairy(), 30).ToArray(), 2).ToArray(),
                new GameOption() { });
            game.run();
            yield return new WaitForSeconds(.1f);
            game.sortedPlayers[0].cmdInitReplace(game);
            game.sortedPlayers[1].cmdInitReplace(game);
            yield return new WaitForSeconds(.1f);

            game.sortedPlayers[0].cmdUse(game, game.sortedPlayers[0].hand[0], 0);
            yield return new WaitForSeconds(.1f);
            game.sortedPlayers[0].cmdAttack(game, game.sortedPlayers[0].field[0], game.sortedPlayers[1].master);
            yield return new WaitForSeconds(.1f);
            game.sortedPlayers[0].cmdTurnEnd(game);
            yield return new WaitForSeconds(.1f);
            game.sortedPlayers[1].cmdTurnEnd(game);
            yield return new WaitForSeconds(.1f);
            game.sortedPlayers[0].cmdTurnEnd(game);
            yield return new WaitForSeconds(.1f);
            game.sortedPlayers[1].cmdUse(game, game.sortedPlayers[1].skill, 0, game.sortedPlayers[1].master);
            yield return new WaitForSeconds(.1f);

            THHCard.HealEventArg heal = game.triggers.getRecordedEvents().LastOrDefault(e => e is THHCard.HealEventArg) as THHCard.HealEventArg;
            Assert.AreEqual(1, heal.cards.Length);
            Assert.AreEqual(2, heal.value);
            Assert.AreEqual(1, heal.infoDic[game.sortedPlayers[1].master].healedValue);
        }
    }
}
