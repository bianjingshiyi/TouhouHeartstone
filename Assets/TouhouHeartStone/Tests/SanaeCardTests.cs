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
                Enumerable.Repeat(game.getDefine<TestFreezeBrain>() as CardDefine, 29).Concat(Enumerable.Repeat(game.getDefine<StarRitual>(), 1)));

            game.skipUntilCanUse<StarRitual>(you);
            you.cmdUse(game, you.hand.getCard<StarRitual>());

            game.Dispose();
        }

        [Test]
        public void GrayThaumaturgyTest()
        {
            THHGame game = TestGameflow.initGameWithoutPlayers(null, new GameOption()
            {
                shuffle = false
            });
            THHPlayer oppo = game.createPlayer(0, "玩家0", game.getDefine<Sanae>(),
                Enumerable.Repeat(game.getDefine<DefaultServant>() as CardDefine, 30));
            THHPlayer you = game.createPlayer(1, "玩家1", game.getDefine<Sanae>(),
                Enumerable.Repeat(game.getDefine<TestFreezeBrain>() as CardDefine, 28).Concat(Enumerable.Repeat(game.getDefine<GrayThaumaturgy>(), 2)));

            game.skipUntilHasGem(you, 2);
            you.master.damage(game, null, 10);

            you.cmdUse(game, you.hand.getCard<GrayThaumaturgy>(), targets: you.master);
            Sanae.revealDuel(you,game);
            Sanae.revealDuel(you, game);
            Sanae.revealDuel(you, game);

            
            you.cmdUse(game, you.hand.getCard<GrayThaumaturgy>(),targets: you.master);

            game.Dispose();
        }

        [Test]
        public void BumperCropRiceShowerTest()
        {
            THHGame game = TestGameflow.initGameWithoutPlayers(null, new GameOption()
            {
                shuffle = false
            });
            THHPlayer oppo = game.createPlayer(0, "玩家0", game.getDefine<Sanae>(),
                Enumerable.Repeat(game.getDefine<DefaultServant>() as CardDefine, 30));
            THHPlayer you = game.createPlayer(1, "玩家1", game.getDefine<Sanae>(),
                Enumerable.Repeat(game.getDefine<DefaultServant>() as CardDefine, 29).Concat(Enumerable.Repeat(game.getDefine<BumperCrop>(), 1)));

            for(int i = 0; i < 3; i = i + 1)
            {
                game.skipUntilCanUse<DefaultServant>(you);
                you.cmdUse(game, you.hand.getCard<DefaultServant>());
                you.field[0].damage(game, null, 3);
            }
            you.master.damage(game,null,3);
            game.skipUntilCanUse<BumperCrop>(you);
            you.cmdUse(game, you.hand.getCard<BumperCrop>());

            game.Dispose();
        }

        [Test]
        public void CharmOfGoodCommerceTest()
        {
            THHGame game = TestGameflow.initGameWithoutPlayers(null, new GameOption()
            {
                shuffle = false
            });
            THHPlayer oppo = game.createPlayer(0, "玩家0", game.getDefine<Sanae>(),
                Enumerable.Repeat(game.getDefine<DefaultServant>() as CardDefine, 30));
            THHPlayer you = game.createPlayer(1, "玩家1", game.getDefine<Sanae>(),
                Enumerable.Repeat(game.getDefine<DefectedAgent>() as CardDefine, 29).Concat(Enumerable.Repeat(game.getDefine<CharmOfGoodCommerce>(), 1)));

            game.skipUntilCanUse<DefaultServant>(oppo);
            oppo.cmdUse(game, oppo.hand.getCard<DefaultServant>());

            game.skipUntilCanUse<CharmOfGoodCommerce>(you);
            you.master.damage(game, null,4);
            you.cmdUse(game, you.hand.getCard<CharmOfGoodCommerce>(), targets: oppo.field[0]);

            game.Dispose();
        }

        [Test]
        public void GuestStarsMiracleTest()
        {
            THHGame game = TestGameflow.initGameWithoutPlayers(null, new GameOption()
            {
                shuffle = false
            });
            THHPlayer oppo = game.createPlayer(0, "玩家0", game.getDefine<Sanae>(),
                Enumerable.Repeat(game.getDefine<DefaultServant>() as CardDefine, 30));
            THHPlayer you = game.createPlayer(1, "玩家1", game.getDefine<Sanae>(),
                Enumerable.Repeat(game.getDefine<DefectedAgent>() as CardDefine, 29).Concat(Enumerable.Repeat(game.getDefine<GuestStarsMiracle>(), 1)));

            for (int i = 0; i < 3; i = i + 1)
            {
                game.skipUntilCanUse<DefaultServant>(oppo);
                oppo.cmdUse(game, oppo.hand.getCard<DefaultServant>());
                oppo.field[0].damage(game, null, 3);
            }
            game.skipUntilCanUse<GuestStarsMiracle>(you);
            you.cmdUse(game, you.hand.getCard<GuestStarsMiracle>());

            game.Dispose();
            
        }

        [Test]
        public void OpenSeaMiracleTest()
        {
            THHGame game = TestGameflow.initGameWithoutPlayers(null, new GameOption()
            {
                shuffle = false
            });
            THHPlayer oppo = game.createPlayer(0, "玩家0", game.getDefine<Sanae>(),
                Enumerable.Repeat(game.getDefine<DefaultServant>() as CardDefine, 30));
            THHPlayer you = game.createPlayer(1, "玩家1", game.getDefine<Sanae>(),
                Enumerable.Repeat(game.getDefine<DefectedAgent>() as CardDefine, 29).Concat(Enumerable.Repeat(game.getDefine<OpenSeaMiracle>(), 1)));

            for (int i = 0; i < 3; i = i + 1)
            {
                game.skipUntilCanUse<DefaultServant>(oppo);
                oppo.cmdUse(game, oppo.hand.getCard<DefaultServant>());
            }
            game.skipUntilCanUse<OpenSeaMiracle>(you);
            you.cmdUse(game, you.hand.getCard<OpenSeaMiracle>(),targets: oppo.field[1]);

            game.Dispose();

        }

        [Test]
        public void GodSummon_ThrustTest()
        {
            TestGameflow.createGame(out var game, out var you, out var oppo,
                new KeyValuePair<int, int>(GodSummon_Thrust.ID, 2),
                new KeyValuePair<int, int>(YasakaKanako.ID, 2),
                new KeyValuePair<int, int>(DefaultServant.ID, 2)
                
            );
         
            game.skipUntilCanUse<GodSummon_Thrust>(you);
            you.cmdUse(game, you.hand.getCard<GodSummon_Thrust>());

            game.Dispose();

        }

        [Test]
        public void ManipulatedGaintFrogTest()
        {
            THHGame game = TestGameflow.initGameWithoutPlayers(null, new GameOption()
            {
                shuffle = false
            });
            THHPlayer oppo = game.createPlayer(0, "玩家0", game.getDefine<Sanae>(),
                Enumerable.Repeat(game.getDefine<DefaultServant>() as CardDefine, 30));
            THHPlayer you = game.createPlayer(1, "玩家1", game.getDefine<Sanae>(),
                Enumerable.Repeat(game.getDefine<DefaultServant>() as CardDefine, 28).Concat(Enumerable.Repeat(game.getDefine<ManipulatedGaintFrog>(), 2)));

            for (int i = 0; i < 3; i = i + 1)
            {
                game.skipUntilCanUse<DefaultServant>(oppo);
                oppo.cmdUse(game, oppo.hand.getCard<DefaultServant>());
            }
            for (int i = 0; i < 3; i = i + 1)
            {
                game.skipUntilCanUse<DefaultServant>(you);
                you.cmdUse(game, you.hand.getCard<DefaultServant>());
            }

            game.skipUntilCanUse<ManipulatedGaintFrog>(you);
            Sanae.revealDuel(you, game);
            Sanae.revealDuel(you, game);
            Sanae.revealDuel(you, game);


            you.cmdUse(game, you.hand.getCard<ManipulatedGaintFrog>());
            you.field[0].setDead(game, true);
            game.updateDeath(game);

            game.Dispose();
        }

        [Test]
        public void OrochiOfAncientTimesTest()
        {
            THHGame game = TestGameflow.initGameWithoutPlayers(null, new GameOption()
            {
                shuffle = false
            });
            THHPlayer oppo = game.createPlayer(0, "玩家0", game.getDefine<Sanae>(),
                Enumerable.Repeat(game.getDefine<DefaultServant>() as CardDefine, 30));
            THHPlayer you = game.createPlayer(1, "玩家1", game.getDefine<Sanae>(),
                Enumerable.Repeat(game.getDefine<DefaultServant>() as CardDefine, 28).Concat(Enumerable.Repeat(game.getDefine<OrochiOfAncientTimes>(), 2)));
            for (int i = 0; i < 3; i = i + 1)
            {
                game.skipUntilCanUse<DefaultServant>(oppo);
                oppo.cmdUse(game, oppo.hand.getCard<DefaultServant>());
            }
            for (int i = 0; i < 3; i = i + 1)
            {
                game.skipUntilCanUse<DefaultServant>(you);
                you.cmdUse(game, you.hand.getCard<DefaultServant>());
            }
            game.skipUntilCanUse<OrochiOfAncientTimes>(you);
            you.cmdUse(game, you.hand.getCard<OrochiOfAncientTimes>());
            game.skipUntilCanUse<OrochiOfAncientTimes>(you);
            Sanae.revealDuel(you, game);
            you.cmdUse(game, you.hand.getCard<OrochiOfAncientTimes>());
            game.Dispose();
        }

        [Test]
        public void YasakaKanakoTest()
        {
            THHGame game = TestGameflow.initGameWithoutPlayers(null, new GameOption()
            {
                shuffle = false
            });
            THHPlayer oppo = game.createPlayer(0, "玩家0", game.getDefine<Sanae>(),
                Enumerable.Repeat(game.getDefine<DefaultServant>() as CardDefine, 30));
            THHPlayer you = game.createPlayer(1, "玩家1", game.getDefine<Sanae>(),
                Enumerable.Repeat(game.getDefine<DefaultServant>() as CardDefine, 26).Concat(Enumerable.Repeat(game.getDefine<YasakaKanako>(), 4)));

            for (int i = 0; i < 3; i = i + 1)
            {
                game.skipUntilCanUse<DefaultServant>(oppo);
                oppo.cmdUse(game, oppo.hand.getCard<DefaultServant>());
            }
            
            game.skipUntilCanUse<OrochiOfAncientTimes>(you);
            you.cmdUse(game, you.hand.getCard<OrochiOfAncientTimes>());
            game.skipUntilCanUse<OrochiOfAncientTimes>(you);
            Sanae.revealDuel(you, game);
            you.cmdUse(game, you.hand.getCard<OrochiOfAncientTimes>());
            game.Dispose();
        }
        
        [Test]
        public void SuwakoBackToFullHandTest()
        {
            TestGameflow.createGame(out var game, out var you, out var oppo, new KeyValuePair<int, int>(MoriyaSuwako.ID,1));
            game.skipUntilCanUse<MoriyaSuwako>(you);
            you.cmdUse(game, you.hand.getCard<MoriyaSuwako>());
            game.skipTurnUntil(() => { return you.hand.count == 10;});
            you.field[0].backToHand(game);
            Assert.AreEqual(10,you.hand.count);
        }
    }
}
