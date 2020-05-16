using System.Collections;
using System;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using System.Linq;
using TouhouHeartstone;
using TouhouCardEngine;
using TouhouHeartstone.Builtin;

namespace Tests
{
    public class NeutralCardTests
    {
        [Test]
        public void rashFairyTest()
        {
            THHGame game = TestGameflow.initStandardGame(null, new int[] { 0, 1 },
            Enumerable.Repeat(new Reimu(), 2).ToArray(),
            Enumerable.Repeat(Enumerable.Repeat(new RashFairy(), 30).ToArray(), 2).ToArray(),
            new GameOption() { });
            game.run();
            game.sortedPlayers[0].cmdInitReplace(game);
            game.sortedPlayers[1].cmdInitReplace(game);

            game.sortedPlayers[0].cmdUse(game, game.sortedPlayers[0].hand[0], 0);
            Assert.True(game.sortedPlayers[0].field[0].isCharge());
            Assert.True(game.sortedPlayers[0].field[0].isReady());
        }
        [Test]
        public void humanVillageGuardTest()
        {
            THHGame game = TestGameflow.initStandardGame(null, new int[] { 0, 1 },
            Enumerable.Repeat(new Reimu(), 2).ToArray(),
            Enumerable.Repeat(Enumerable.Repeat(new HumanVillageGuard(), 30).ToArray(), 2).ToArray(),
            new GameOption() { });
            game.run();
            game.sortedPlayers[0].cmdInitReplace(game);
            game.sortedPlayers[1].cmdInitReplace(game);

            game.sortedPlayers[0].cmdTurnEnd(game);
            game.sortedPlayers[1].cmdTurnEnd(game);
            game.sortedPlayers[0].cmdTurnEnd(game);
            game.sortedPlayers[1].cmdTurnEnd(game);

            game.sortedPlayers[0].cmdUse(game, game.sortedPlayers[0].hand[0], 0);
            Assert.True(game.sortedPlayers[0].field[0].isTaunt());
        }
        [Test]
        public void rifleHunterTest()
        {
            THHGame game = TestGameflow.initStandardGame(null, new int[] { 0, 1 },
            Enumerable.Repeat(new Reimu(), 2).ToArray(),
            Enumerable.Repeat(Enumerable.Repeat(new RifleHunter(), 30).ToArray(), 2).ToArray(),
            new GameOption() { });
            game.run();
            game.sortedPlayers[0].cmdInitReplace(game);
            game.sortedPlayers[1].cmdInitReplace(game);

            game.sortedPlayers[0].cmdTurnEnd(game);
            game.sortedPlayers[1].cmdTurnEnd(game);
            game.sortedPlayers[0].cmdTurnEnd(game);
            game.sortedPlayers[1].cmdTurnEnd(game);

            game.sortedPlayers[0].cmdUse(game, game.sortedPlayers[0].hand[0], 0, game.sortedPlayers[1].master);
            Assert.AreEqual(28, game.sortedPlayers[1].master.getCurrentLife());
        }
        [Test]
        public void missingSpecterTest()
        {
            THHGame game = TestGameflow.initStandardGame(null, new int[] { 0, 1 },
            Enumerable.Repeat(new Reimu(), 2).ToArray(),
            Enumerable.Repeat(Enumerable.Repeat(new MissingSpecter(), 30).ToArray(), 2).ToArray(),
            new GameOption() { });
            game.run();
            game.sortedPlayers[0].cmdInitReplace(game);
            game.sortedPlayers[1].cmdInitReplace(game);

            game.sortedPlayers[0].cmdTurnEnd(game);
            game.sortedPlayers[1].cmdTurnEnd(game);
            game.sortedPlayers[0].cmdTurnEnd(game);
            game.sortedPlayers[1].cmdTurnEnd(game);
            game.sortedPlayers[0].cmdTurnEnd(game);
            game.sortedPlayers[1].cmdTurnEnd(game);
            game.sortedPlayers[0].cmdTurnEnd(game);
            game.sortedPlayers[1].cmdTurnEnd(game);

            game.sortedPlayers[0].cmdUse(game, game.sortedPlayers[0].hand[0], 0);
            game.sortedPlayers[0].cmdTurnEnd(game);
            game.sortedPlayers[1].cmdUse(game, game.sortedPlayers[1].hand[0], 0);
            game.sortedPlayers[1].cmdTurnEnd(game);
            game.sortedPlayers[0].cmdAttack(game, game.sortedPlayers[0].field[0], game.sortedPlayers[1].field[0]);

            Assert.AreEqual(1, game.sortedPlayers[0].field.count);
            Assert.AreEqual(1, game.sortedPlayers[1].field.count);
        }
        [Test]
        public void drizzleFairyTest()
        {
            THHGame game = TestGameflow.initGameWithoutPlayers(null, new GameOption()
            {
                shuffle = false
            });
            game.createPlayer(0, "玩家0", game.getCardDefine<Reimu>(), Enumerable.Repeat(game.getCardDefine<DrizzleFairy>() as CardDefine, 29)
            .Concat(Enumerable.Repeat(game.getCardDefine<FantasySeal>(), 1)));
            game.createPlayer(1, "玩家1", game.getCardDefine<Reimu>(), Enumerable.Repeat(game.getCardDefine<DrizzleFairy>() as CardDefine, 29)
            .Concat(Enumerable.Repeat(game.getCardDefine<FantasySeal>(), 1)));
            game.run();
            game.sortedPlayers[0].cmdInitReplace(game);
            game.sortedPlayers[1].cmdInitReplace(game);

            //第一回合
            game.sortedPlayers[0].cmdUse(game, game.sortedPlayers[0].hand[1], 0);

            game.sortedPlayers[0].cmdTurnEnd(game);

            game.sortedPlayers[1].cmdUse(game, game.sortedPlayers[1].hand[1], 0);

            game.sortedPlayers[1].cmdTurnEnd(game);
            //第二回合
            game.sortedPlayers[0].cmdUse(game, game.sortedPlayers[0].hand[1], 0);
            game.sortedPlayers[0].cmdUse(game, game.sortedPlayers[0].hand[1], 0);

            game.sortedPlayers[0].cmdTurnEnd(game);
            game.sortedPlayers[1].cmdTurnEnd(game);
            //第三回合，共计4个妖精
            game.sortedPlayers[0].cmdUse(game, game.sortedPlayers[0].hand[1], 0);

            game.sortedPlayers[0].cmdTurnEnd(game);
            game.sortedPlayers[1].cmdTurnEnd(game);
            //第四回合
            game.sortedPlayers[0].cmdTurnEnd(game);

            game.sortedPlayers[1].cmdUse(game, game.sortedPlayers[1].hand[0], 0);

            //对3个随从造成3点伤害
            THHCard.DamageEventArg damage = game.triggers.getRecordedEvents().LastOrDefault(e => e is THHCard.DamageEventArg) as THHCard.DamageEventArg;
            Assert.AreEqual(3, damage.cards.Length);
            Assert.AreEqual(3, damage.value);
            Assert.AreEqual(1, game.sortedPlayers[0].field.count);
        }

        [Test]
        public void RushTest()
        {
            //THHGame game = TestGameflow.initGameWithoutPlayers(null, new GameOption()
            //{
            //    shuffle = false
            //});
            //game.createPlayer(0, "玩家0", game.getCardDefine<Reimu>(), Enumerable.Repeat(game.getCardDefine<DefaultServant>() as CardDefine, 29)
            //.Concat(Enumerable.Repeat(game.getCardDefine<RushServant>(), 1)));
            //game.createPlayer(1, "玩家1", game.getCardDefine<Reimu>(), Enumerable.Repeat(game.getCardDefine<DefaultServant>() as CardDefine, 29)
            //.Concat(Enumerable.Repeat(game.getCardDefine<RushServant>(), 1)));
            //game.run();
            //game.sortedPlayers[0].cmdInitReplace(game);
            //game.sortedPlayers[1].cmdInitReplace(game);
            THHGame game = GameInitWithoutPlayer<DefaultServant, RushServant>(29, 1);

            game.sortedPlayers[0].cmdTurnEnd(game);
            game.sortedPlayers[1].cmdUse(game, game.sortedPlayers[1].hand[1], 0);
            game.sortedPlayers[1].cmdTurnEnd(game);
            game.sortedPlayers[0].cmdUse(game, game.sortedPlayers[0].hand[0], 0);
            game.sortedPlayers[0].cmdUse(game, game.sortedPlayers[0].hand[1], 1);

            Assert.True(game.sortedPlayers[0].field[0].isRush());
            game.sortedPlayers[0].cmdAttack(game, game.sortedPlayers[0].field[1], game.sortedPlayers[1].master);
            Assert.AreEqual(30, game.sortedPlayers[1].master.getCurrentLife());
            game.sortedPlayers[0].cmdAttack(game, game.sortedPlayers[0].field[0], game.sortedPlayers[1].master);
            Assert.AreEqual(30, game.sortedPlayers[1].master.getCurrentLife());
            game.sortedPlayers[0].cmdAttack(game, game.sortedPlayers[0].field[0], game.sortedPlayers[1].field[0]);
            Debug.Log(game.sortedPlayers[1].field[0].getCurrentLife());
            Assert.AreEqual(6, game.sortedPlayers[1].field[0].getCurrentLife());
        }

        [Test]
        public void ShieldTest()
        {
            //THHGame game = TestGameflow.initGameWithoutPlayers(null, new GameOption()
            //{
            //    shuffle = false
            //});
            //game.createPlayer(0, "玩家0", game.getCardDefine<Reimu>(), Enumerable.Repeat(game.getCardDefine<DefaultServant>() as CardDefine, 29)
            //.Concat(Enumerable.Repeat(game.getCardDefine<ShieldServant>(), 1)));
            //game.createPlayer(1, "玩家1", game.getCardDefine<Reimu>(), Enumerable.Repeat(game.getCardDefine<DefaultServant>() as CardDefine, 29)
            //.Concat(Enumerable.Repeat(game.getCardDefine<ShieldServant>(), 1)));
            //game.run();
            //game.sortedPlayers[0].cmdInitReplace(game);
            //game.sortedPlayers[1].cmdInitReplace(game);
            THHGame game = GameInitWithoutPlayer<DefaultServant, ShieldServant>(29, 1);
            game.sortedPlayers[0].cmdTurnEnd(game);
            game.sortedPlayers[1].cmdUse(game, game.sortedPlayers[1].hand[1], 0);
            game.sortedPlayers[1].cmdTurnEnd(game);
            game.sortedPlayers[0].cmdUse(game, game.sortedPlayers[0].hand[1], 0);
            game.sortedPlayers[0].cmdUse(game, game.sortedPlayers[0].hand[0], 1);
            game.sortedPlayers[0].cmdTurnEnd(game);
            game.sortedPlayers[1].cmdTurnEnd(game);

            game.sortedPlayers[0].cmdAttack(game, game.sortedPlayers[0].field[0], game.sortedPlayers[1].field[0]);
            Assert.AreEqual(6, game.sortedPlayers[0].field[0].getCurrentLife());
            game.sortedPlayers[0].cmdAttack(game, game.sortedPlayers[0].field[1], game.sortedPlayers[1].field[0]);
            Assert.AreEqual(1, game.sortedPlayers[0].field[1].getCurrentLife());
        }

        [Test]
        public void StealthTest()
        {
            //THHGame game = TestGameflow.initGameWithoutPlayers(null, new GameOption()
            //{
            //    shuffle = false
            //});
            //game.createPlayer(0, "玩家0", game.getCardDefine<Reimu>(), Enumerable.Repeat(game.getCardDefine<DefaultServant>() as CardDefine, 29)
            //.Concat(Enumerable.Repeat(game.getCardDefine<StealthServant>(), 1)));
            //game.createPlayer(1, "玩家1", game.getCardDefine<Reimu>(), Enumerable.Repeat(game.getCardDefine<DefaultServant>() as CardDefine, 29)
            //.Concat(Enumerable.Repeat(game.getCardDefine<StealthServant>(), 1)));
            //game.run();
            //game.sortedPlayers[0].cmdInitReplace(game);
            //game.sortedPlayers[1].cmdInitReplace(game);
            THHGame game = GameInitWithoutPlayer<DefaultServant, StealthServant>(29, 1);

            game.sortedPlayers[0].cmdTurnEnd(game);
            game.sortedPlayers[1].cmdUse(game, game.sortedPlayers[1].hand[1], 0);
            game.sortedPlayers[1].cmdTurnEnd(game);
            game.sortedPlayers[0].cmdUse(game, game.sortedPlayers[0].hand[1], 0);
            game.sortedPlayers[0].cmdUse(game, game.sortedPlayers[0].hand[0], 1);
            Assert.True(game.sortedPlayers[0].field[1].isStealth());
            game.sortedPlayers[0].cmdTurnEnd(game);
            

            game.sortedPlayers[1].cmdAttack(game, game.sortedPlayers[1].field[0], game.sortedPlayers[0].field[1]);
            //game.sortedPlayers[1].cmdAttack(game, game.sortedPlayers[1].field[0], game.sortedPlayers[1].master);
            
            Assert.AreEqual(3, game.sortedPlayers[0].field[1].getCurrentLife());  //潜行的随从不会被攻击，因此血量不变
            game.sortedPlayers[1].cmdAttack(game, game.sortedPlayers[1].field[0], game.sortedPlayers[0].field[0]);
            Assert.AreEqual(6, game.sortedPlayers[1].field[0].getCurrentLife());  //非潜行随从可以被攻击
            game.sortedPlayers[1].cmdTurnEnd(game);
            game.sortedPlayers[0].cmdAttack(game, game.sortedPlayers[0].field[1], game.sortedPlayers[1].field[0]);
            Assert.False(game.sortedPlayers[0].field[1].isStealth());   //潜行随从攻击后变为非潜行状态
            game.sortedPlayers[0].cmdTurnEnd(game);
            game.sortedPlayers[1].cmdAttack(game, game.sortedPlayers[1].field[0], game.sortedPlayers[0].field[1]);
            Assert.AreEqual(2, game.sortedPlayers[0].field[1].getCurrentLife());    //变为非潜行状态后可以被攻击
        }

        public void SuckingServantTest()
        {
            THHGame game = GameInitWithoutPlayer<DefaultServant, SuckingServant>(29, 1);

        }

        /// <summary>
        /// 攻击自己的测试
        /// </summary>
        [Test]
        public void AttackSelf()
        {
            THHGame game = TestGameflow.initStandardGame(null, new int[] { 0, 1 },
            Enumerable.Repeat(new Reimu(), 2).ToArray(),
            Enumerable.Repeat(Enumerable.Repeat(new DefaultServant(), 30).ToArray(), 2).ToArray(),
            new GameOption() { });
            game.run();
            game.sortedPlayers[0].cmdInitReplace(game);
            game.sortedPlayers[1].cmdInitReplace(game);

            game.sortedPlayers[0].cmdUse(game, game.sortedPlayers[0].hand[0], 0);
            game.sortedPlayers[0].cmdTurnEnd(game);
            game.sortedPlayers[1].cmdTurnEnd(game);
            game.sortedPlayers[0].cmdUse(game, game.sortedPlayers[0].hand[0], 1);
            game.sortedPlayers[0].cmdAttack(game, game.sortedPlayers[0].field[0], game.sortedPlayers[0].master);
            Assert.AreEqual(30, game.sortedPlayers[0].master.getCurrentLife()); //攻击己方master
            game.sortedPlayers[0].cmdAttack(game, game.sortedPlayers[0].field[0], game.sortedPlayers[0].field[1]);
            Assert.AreEqual(7, game.sortedPlayers[0].field[1].getCurrentLife());  //攻击己方servant

        }

        /// <summary>
        /// 从player[0]开始经过x回合
        /// </summary>
        /// <param name="game"></param>
        /// <param name="x"></param>
        public void AfterXRound(THHGame game, int x)
        {
            for (int i = 0; i < x; i++)
            {
                game.sortedPlayers[0].cmdTurnEnd(game);
                game.sortedPlayers[1].cmdTurnEnd(game);
            }
        }

        public THHGame GameInitWithoutPlayer<T1, T2>(int card1Count, int card2Count) where T1 : CardDefine where T2 : CardDefine
        {
            
            THHGame game = TestGameflow.initGameWithoutPlayers(null, new GameOption()
            {
                shuffle = false
            });
            game.createPlayer(0, "玩家0", game.getCardDefine<Reimu>(), Enumerable.Repeat(game.getCardDefine<T1>() as CardDefine, 29)
            .Concat(Enumerable.Repeat(game.getCardDefine<T2>(), 1)));
            game.createPlayer(1, "玩家1", game.getCardDefine<Reimu>(), Enumerable.Repeat(game.getCardDefine<T1>() as CardDefine, 29)
            .Concat(Enumerable.Repeat(game.getCardDefine<T2>(), 1)));
            game.run();
            game.sortedPlayers[0].cmdInitReplace(game);
            game.sortedPlayers[1].cmdInitReplace(game);
            return game;
        }
        
    }
}
