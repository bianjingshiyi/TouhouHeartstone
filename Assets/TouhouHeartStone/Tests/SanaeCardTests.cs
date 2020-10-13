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
using System.Threading.Tasks;

namespace Tests
{
    public class SanaeCardTests
    {
        [Test]
        public void sanaeSkillTest()
        {
            THHGame game = TestGameflow.initStandardGame(null, new int[] { 0, 1 },
                Enumerable.Repeat(new Sanae(), 2).ToArray(),
                Enumerable.Repeat(Enumerable.Repeat(new RashFairy(), 30).ToArray(), 2).ToArray(),
                new GameOption() { });
            game.run();
            game.sortedPlayers[0].cmdInitReplace(game);
            game.sortedPlayers[1].cmdInitReplace(game);

            game.sortedPlayers[0].cmdUse(game, game.sortedPlayers[0].hand[0], 0);
            game.sortedPlayers[0].cmdAttack(game, game.sortedPlayers[0].field[0], game.sortedPlayers[1].master);
            game.sortedPlayers[0].cmdTurnEnd(game);
            game.sortedPlayers[1].cmdTurnEnd(game);
            game.sortedPlayers[0].cmdTurnEnd(game);
            game.sortedPlayers[1].cmdUse(game, game.sortedPlayers[1].skill, 0, game.sortedPlayers[1].master);

            THHCard.HealEventArg heal = game.triggers.getRecordedEvents().LastOrDefault(e => e is THHCard.HealEventArg) as THHCard.HealEventArg;
            Assert.AreEqual(1, heal.cards.Length);
            Assert.AreEqual(2, heal.value);
            Assert.AreEqual(1, heal.infoDic[game.sortedPlayers[1].master].healedValue);
        }

        //拼点测试
        [Test]
        public void RevealDuelTest()
        {
            THHGame game = TestGameflow.initGameWithoutPlayers(null, new GameOption()
            {
                shuffle = false
            });
            THHPlayer oppo = game.createPlayer(0, "玩家0", game.getDefine<Sanae>(),
                Enumerable.Repeat(game.getDefine<TestFreezeBrain>() as CardDefine, 30));
            THHPlayer you = game.createPlayer(1, "玩家1", game.getDefine<Sanae>(),
                Enumerable.Repeat(game.getDefine<DefaultServant>() as CardDefine, 30));
            var reveal = Sanae.revealDuel(you,game);
            Assert.True(reveal.IsCompleted);

            game.Dispose();
        }

        /// <summary>
        /// 星之仪式测试
        /// 召请神明 如果神明回应了你，抽一张法力值消耗最高的牌；否则将一张全新的“星之仪式”置入手牌
        /// </summary>
        [Test]
        public void StarCeremonyTest()
        {
            THHGame game = TestGameflow.initGameWithoutPlayers(null, new GameOption()
            {
                shuffle = false
            });
            THHPlayer oppo = game.createPlayer(0, "玩家0", game.getDefine<Sanae>(),
                Enumerable.Repeat(game.getDefine<DefaultServant>() as CardDefine, 30));
            THHPlayer you = game.createPlayer(1, "玩家1", game.getDefine<Sanae>(),
                Enumerable.Repeat(game.getDefine<TestFreezeBrain>() as CardDefine, 29).Concat(Enumerable.Repeat(game.getDefine<StarCeremony>(), 1)));

            game.skipUntilCanUse<StarCeremony>(you);
            you.cmdUse(game, you.hand.getCard<StarCeremony>());

            game.Dispose();
        }
    }
}
