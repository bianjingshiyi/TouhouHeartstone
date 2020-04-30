using NUnit.Framework;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TouhouHeartstone;
using TouhouHeartstone.Builtin;
using UnityEngine;
using UnityEngine.TestTools;

namespace Tests
{
    public class PatchouliCardTests
    {
        [UnityTest]
        public IEnumerator PatchouliSkillTest()
        {
            THHGame game = TestGameflow.initStandardGame(null, new int[] { 0, 1 },
                Enumerable.Repeat(new Patchouli(), 2).ToArray(),
                Enumerable.Repeat(Enumerable.Repeat(new RashFairy(), 30).ToArray(), 2).ToArray(),
                new GameOption() { });
            game.run();
            yield return new WaitForSeconds(.1f);
            game.sortedPlayers[0].cmdInitReplace(game);
            game.sortedPlayers[1].cmdInitReplace(game);
            yield return new WaitForSeconds(.1f);
            game.sortedPlayers[0].cmdUse(game, game.sortedPlayers[0].hand[0], 0);
            yield return new WaitForSeconds(.1f);
            game.sortedPlayers[0].cmdTurnEnd(game);
            yield return new WaitForSeconds(.1f);
            game.sortedPlayers[1].cmdTurnEnd(game);
            yield return new WaitForSeconds(.1f);
            game.sortedPlayers[0].cmdTurnEnd(game);
            yield return new WaitForSeconds(.1f);
            game.sortedPlayers[1].cmdTurnEnd(game);
            yield return new WaitForSeconds(.1f);
            game.sortedPlayers[0].cmdTurnEnd(game);
            yield return new WaitForSeconds(.1f);

            int preHand = game.sortedPlayers[1].hand.count;
            int preDeck = game.sortedPlayers[1].deck.count;
            int preLife = game.sortedPlayers[1].master.getLife();
            game.sortedPlayers[1].cmdUse(game, game.sortedPlayers[1].skill, 0, game.sortedPlayers[1].master);
            yield return new WaitForSeconds(.1f);
            Assert.AreEqual(1, game.sortedPlayers[1].hand.count - preHand);
            Assert.AreEqual(-1, game.sortedPlayers[1].deck.count - preDeck);
            Assert.AreEqual(-2, game.sortedPlayers[1].master.getLife() - preLife);
            
            

            
        }
    }
}

