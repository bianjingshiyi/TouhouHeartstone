using System.Collections;
using System.Collections.Generic;
using System.Threading;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using System.Linq;
using TouhouCardEngine;
using TouhouCardEngine.Interfaces;
using TouhouHeartstone;
using System.Threading.Tasks;

namespace Tests
{
    public class GameflowTests
    {
        [Test]
        public void initTest()
        {
            TaskExceptionHandler.register();
            THHGame game = new THHGame(new TestMaster(), new TestSkill(), new TestServant())
            {
                triggers = new GameObject("TriggerManager").AddComponent<TriggerManager>()
            };
            game.createPlayer("玩家1", game.getCardDefine<TestMaster>(), Enumerable.Repeat(game.getCardDefine<TestServant>(), 30));
            game.createPlayer("玩家2", game.getCardDefine<TestMaster>(), Enumerable.Repeat(game.getCardDefine<TestServant>(), 30));
            List<IEventArg> eventList = new List<IEventArg>();
            game.triggers.onEventAfter += arg =>
            {
                eventList.Add(arg);
            };

            game.init();

            Assert.AreEqual(1, eventList.Count);
            THHGame.InitEventArg initEvent = eventList[0] as THHGame.InitEventArg;
            Assert.IsInstanceOf<THHGame.InitEventArg>(eventList[0]);
            Assert.AreEqual(TestMaster.ID, game.players[0].master.define.id);
            Assert.AreEqual(30, game.players[0].master.getLife());
            Assert.AreEqual(TestMaster.ID, game.players[1].master.define.id);
            Assert.AreEqual(30, game.players[1].master.getLife());
            Assert.AreEqual(2, game.sortedPlayers.Length);
            bool isFirstPlayer = game.getPlayerIndex(game.sortedPlayers[0]) == 0;
            Assert.AreEqual(isFirstPlayer ? 3 : 4, game.players[0].init.count);
            Assert.AreEqual(isFirstPlayer ? 4 : 3, game.players[1].init.count);
        }
        [Test]
        public void initReplaceTest()
        {
            TaskExceptionHandler.register();
            THHGame game = new THHGame(new TestMaster(), new TestSkill(), new TestServant())
            {
                triggers = new GameObject("TriggerManager").AddComponent<TriggerManager>()
            };
            game.createPlayer("玩家1", game.getCardDefine<TestMaster>(), Enumerable.Repeat(game.getCardDefine<TestServant>(), 30));
            game.createPlayer("玩家2", game.getCardDefine<TestMaster>(), Enumerable.Repeat(game.getCardDefine<TestServant>(), 30));
            List<IEventArg> eventList = new List<IEventArg>();
            game.triggers.onEventAfter += arg =>
            {
                eventList.Add(arg);
            };

            game.init();
            _ = game.players[0].initReplace(game, game.players[0].init[0]);
            _ = game.players[1].initReplace(game, game.players[1].init[0, 1]);
            //替换手牌
            Assert.AreEqual(8, game.triggers.getRecordedEvents().Length);
            THHPlayer.InitReplaceEventArg initReplace = game.triggers.getRecordedEvents()[1] as THHPlayer.InitReplaceEventArg;
            Assert.NotNull(initReplace);
            Assert.AreEqual(game.players[0], initReplace.player);
            Assert.AreEqual(1, initReplace.replacedCards.Length);
            initReplace = game.triggers.getRecordedEvents()[2] as THHPlayer.InitReplaceEventArg;
            Assert.NotNull(initReplace);
            Assert.AreEqual(game.players[1], initReplace.player);
            Assert.AreEqual(2, initReplace.replacedCards.Length);
            //游戏开始
            THHGame.StartEventArg start = game.triggers.getRecordedEvents()[3] as THHGame.StartEventArg;
            Assert.NotNull(start);
            //玩家回合开始
            THHGame.TurnStartEventArg turnStart = game.triggers.getRecordedEvents()[4] as THHGame.TurnStartEventArg;
            Assert.NotNull(turnStart);
            Assert.AreEqual(game.sortedPlayers[0], turnStart.player);
            //增加法力水晶并充满
            THHPlayer.SetMaxGemEventArg setMaxGem = game.triggers.getRecordedEvents()[5] as THHPlayer.SetMaxGemEventArg;
            Assert.NotNull(setMaxGem);
            Assert.AreEqual(1, setMaxGem.value);
            THHPlayer.SetGemEventArg setGem = game.triggers.getRecordedEvents()[6] as THHPlayer.SetGemEventArg;
            Assert.NotNull(setGem);
            Assert.AreEqual(1, setGem.value);
            //抽一张卡
            THHPlayer.DrawEventArg draw = game.triggers.getRecordedEvents()[7] as THHPlayer.DrawEventArg;
            Assert.NotNull(draw);
            Assert.AreEqual(game.sortedPlayers[0], draw.player);
        }
        [Test]
        public void useTest()
        {
            TaskExceptionHandler.register();
            THHGame game = new THHGame(new TestMaster(), new TestSkill(), new TestServant())
            {
                triggers = new GameObject("TriggerManager").AddComponent<TriggerManager>()
            };
            game.createPlayer("玩家1", game.getCardDefine<TestMaster>(), Enumerable.Repeat(game.getCardDefine<TestServant>(), 30));
            game.createPlayer("玩家2", game.getCardDefine<TestMaster>(), Enumerable.Repeat(game.getCardDefine<TestServant>(), 30));
            List<IEventArg> eventList = new List<IEventArg>();
            game.triggers.onEventAfter += arg =>
            {
                eventList.Add(arg);
            };

            game.init();
            _ = game.players[0].initReplace(game);
            _ = game.players[1].initReplace(game);
            _ = game.sortedPlayers[0].tryUse(game, game.sortedPlayers[0].hand[0], 0);

            THHPlayer.UseEventArg use = eventList.FirstOrDefault(e => e is THHPlayer.UseEventArg) as THHPlayer.UseEventArg;
            Assert.NotNull(use);
            Assert.AreEqual(game.sortedPlayers[0], use.player);
            Assert.AreEqual(game.sortedPlayers[0].field[0], use.card);
            Assert.AreEqual(TestServant.ID, use.card.define.id);
            Assert.AreEqual(0, use.position);
            Assert.AreEqual(0, use.targets.Length);
            THHPlayer.SetGemEventArg setGem = eventList.LastOrDefault(e => e is THHPlayer.SetGemEventArg) as THHPlayer.SetGemEventArg;
            Assert.AreEqual(0, setGem.value);
            THHPlayer.SummonEventArg summon = eventList.LastOrDefault(e => e is THHPlayer.SummonEventArg) as THHPlayer.SummonEventArg;
            Assert.NotNull(summon);
            Assert.AreEqual(game.sortedPlayers[0], summon.player);
            Assert.AreEqual(TestServant.ID, summon.card.define.id);
            Assert.AreEqual(0, summon.position);
        }
        [Test]
        public void turnEndTest()
        {
            TaskExceptionHandler.register();
            THHGame game = new THHGame(new TestMaster(), new TestSkill(), new TestServant())
            {
                triggers = new GameObject("TriggerManager").AddComponent<TriggerManager>()
            };
            game.createPlayer("玩家1", game.getCardDefine<TestMaster>(), Enumerable.Repeat(game.getCardDefine<TestServant>(), 30));
            game.createPlayer("玩家2", game.getCardDefine<TestMaster>(), Enumerable.Repeat(game.getCardDefine<TestServant>(), 30));
            List<IEventArg> eventList = new List<IEventArg>();
            game.triggers.onEventAfter += arg =>
            {
                eventList.Add(arg);
            };

            game.init();
            _ = game.players[0].initReplace(game);
            _ = game.players[1].initReplace(game);
            game.turnEnd(game.sortedPlayers[0]);

            THHGame.TurnEndEventArg turnEnd = eventList.LastOrDefault(e => e is THHGame.TurnEndEventArg) as THHGame.TurnEndEventArg;
            Assert.NotNull(turnEnd);
            Assert.AreEqual(game.sortedPlayers[0], turnEnd.player);
            THHGame.TurnStartEventArg turnStart = eventList.LastOrDefault(e => e is THHGame.TurnStartEventArg) as THHGame.TurnStartEventArg;
            Assert.NotNull(turnStart);
            Assert.AreEqual(game.sortedPlayers[1], turnStart.player);
        }
        [Test]
        public void burnTest()
        {
            TaskExceptionHandler.register();
            THHGame game = new THHGame(new TestMaster(), new TestSkill(), new TestServant())
            {
                triggers = new GameObject("TriggerManager").AddComponent<TriggerManager>()
            };
            game.createPlayer("玩家1", game.getCardDefine<TestMaster>(), Enumerable.Repeat(game.getCardDefine<TestServant>(), 30));
            game.createPlayer("玩家2", game.getCardDefine<TestMaster>(), Enumerable.Repeat(game.getCardDefine<TestServant>(), 30));
            List<IEventArg> eventList = new List<IEventArg>();
            game.triggers.onEventAfter += arg =>
            {
                eventList.Add(arg);
            };

            game.init();
            _ = game.players[0].initReplace(game);
            _ = game.players[1].initReplace(game);
            for (int i = 0; i < 7; i++)
            {
                game.turnEnd(game.sortedPlayers[0]);
                game.turnEnd(game.sortedPlayers[1]);
            }

            EventWitness witness = frontends[0].witnessList[frontends[0].witnessList.Count - 1].child[2];
            Assert.AreEqual("onBurn", witness.eventName);
            Assert.AreEqual(firstPlayerIndex, witness.getVar<int>("playerIndex"));
            Assert.AreEqual(1, witness.getVar<int>("cardDID"));
            Assert.IsTrue(witness.getVar<int>("cardRID") > 0);
        }
        [Test]
        public void tiredTest()
        {
            THHGame game = new THHGame(new UnitTestGameEnv());
            TestFrontend[] frontends = new TestFrontend[2];
            frontends[0] = new TestFrontend();
            frontends[1] = new TestFrontend();
            game.addPlayer(frontends[0], new int[] { 1000, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1 });
            game.addPlayer(frontends[1], new int[] { 2000, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1 });
            game.init();
            int firstPlayerIndex = frontends[0].witnessList[0].getVar<int[]>("sortedPlayersIndex")[0];
            int secondPlayerIndex = frontends[0].witnessList[0].getVar<int[]>("sortedPlayersIndex")[1];
            game.initReplace(0, new int[0]);
            game.initReplace(1, new int[0]);
            for (int i = 0; i < 8; i++)
            {
                game.turnEnd(firstPlayerIndex);
                game.turnEnd(secondPlayerIndex);
            }

            EventWitness witness = frontends[0].witnessList[frontends[0].witnessList.Count - 1].child[2];
            Assert.AreEqual("onTired", witness.eventName);
            Assert.AreEqual(firstPlayerIndex, witness.getVar<int>("playerIndex"));
            witness = witness.child[0];
            Assert.AreEqual("onDamage", witness.eventName);
            Assert.AreEqual(1, witness.getVar<int[]>("amounts")[0]);
        }
        [Test]
        public void attackTest()
        {
            THHGame game = new THHGame(new UnitTestGameEnv());
            TestFrontend[] frontends = new TestFrontend[2];
            frontends[0] = new TestFrontend();
            frontends[1] = new TestFrontend();
            game.addPlayer(frontends[0], new int[] { 1000, 1, 1, 1, 1, 1 });
            game.addPlayer(frontends[1], new int[] { 2000, 1, 1, 1, 1, 1 });
            game.init();
            int firstPlayerIndex = frontends[0].witnessList[0].getVar<int[]>("sortedPlayersIndex")[0];
            int secondPlayerIndex = frontends[0].witnessList[0].getVar<int[]>("sortedPlayersIndex")[1];
            int p0c0 = frontends[0].witnessList[0].getVar<int[][]>("initCardsRID")[firstPlayerIndex][0];
            int p1c0 = frontends[1].witnessList[0].getVar<int[][]>("initCardsRID")[secondPlayerIndex][0];
            game.initReplace(0, new int[0]);
            game.initReplace(1, new int[0]);
            game.use(firstPlayerIndex, p0c0, 0, new int[0]);
            game.turnEnd(firstPlayerIndex);
            game.use(secondPlayerIndex, p1c0, 0, new int[0]);
            game.turnEnd(secondPlayerIndex);
            game.attack(firstPlayerIndex, p0c0, p1c0);

            EventWitness witness = frontends[0].witnessList.Find(e => { return e.eventName == "onAttack"; });
            Assert.AreEqual(firstPlayerIndex, witness.getVar<int>("playerIndex"));
            Assert.AreEqual(p0c0, witness.getVar<int>("cardRID"));
            Assert.AreEqual(p1c0, witness.getVar<int>("targetCardRID"));
            witness = witness.child[0];
            Assert.AreEqual("onDamage", witness.eventName);
            Assert.AreEqual(p0c0, witness.getVar<int[]>("cardsRID")[0]);
            Assert.AreEqual(p1c0, witness.getVar<int[]>("cardsRID")[1]);
            Assert.AreEqual(1, witness.getVar<int[]>("amounts")[0]);
            Assert.AreEqual(1, witness.getVar<int[]>("amounts")[1]);
            witness = witness.child[0];
            Assert.AreEqual("onDeath", witness.eventName);
            Assert.AreEqual(p0c0, witness.getVar<int[]>("cardsRID")[0]);
            Assert.AreEqual(p1c0, witness.getVar<int[]>("cardsRID")[1]);
        }
        [Test]
        public void winTest()
        {
            THHGame game = new THHGame(new UnitTestGameEnv());
            TestFrontend[] frontends = new TestFrontend[2];
            frontends[0] = new TestFrontend();
            frontends[1] = new TestFrontend();
            game.addPlayer(frontends[0], new int[] { 1000, 1, 1, 1, 1, 1 });
            game.addPlayer(frontends[1], new int[] { 2000, 1, 1, 1, 1, 1 });
            game.init();
            int firstPlayerIndex = frontends[0].witnessList[0].getVar<int[]>("sortedPlayersIndex")[0];
            int secondPlayerIndex = frontends[0].witnessList[0].getVar<int[]>("sortedPlayersIndex")[1];
            int cardRID = frontends[0].witnessList[0].getVar<int[][]>("initCardsRID")[firstPlayerIndex][0];
            int targetCardRID = frontends[1].witnessList[0].getVar<int[]>("masterCardsRID")[secondPlayerIndex];
            game.initReplace(0, new int[0]);
            game.initReplace(1, new int[0]);
            game.use(firstPlayerIndex, cardRID, 0, new int[0]);
            for (int i = 0; i < 30; i++)
            {
                game.turnEnd(firstPlayerIndex);
                game.turnEnd(secondPlayerIndex);
                game.attack(firstPlayerIndex, cardRID, targetCardRID);
            }

            EventWitness witness = frontends[0].witnessList[frontends[0].witnessList.Count - 1];
            Assert.AreEqual("onAttack", witness.eventName);
            witness = witness.child[0];
            Assert.AreEqual("onDamage", witness.eventName);
            witness = witness.child[0];
            Assert.AreEqual("onDeath", witness.eventName);
            witness = witness.child[0];
            Assert.AreEqual("onGameEnd", witness.eventName);
            Assert.AreEqual(1, witness.getVar<int[]>("winnerPlayersIndex").Length);
            Assert.AreEqual(firstPlayerIndex, witness.getVar<int[]>("winnerPlayersIndex")[0]);
        }
    }
    static class TaskExceptionHandler
    {
        static bool registered { get; set; } = false;
        public static void register()
        {
            if (!registered)
            {
                TaskScheduler.UnobservedTaskException += (sender, obj) =>
                {
                    if (obj.Exception.InnerExceptions != null && obj.Exception.InnerExceptions.Count > 1)
                    {
                        foreach (var exception in obj.Exception.InnerExceptions)
                        {
                            Debug.LogError(exception);
                        }
                    }
                    else if (obj.Exception.InnerException != null)
                        Debug.LogError(obj.Exception.InnerException);
                    else
                        Debug.LogError(obj.Exception);
                    obj.SetObserved();
                };
                registered = true;
            }
        }
    }
    class TestMaster : MasterCardDefine
    {
        public const int ID = 0x00100000;
        public override int id { get; set; } = ID;
        public override int skillID { get; } = TestSkill.ID;
        public override Effect[] effects { get; } = new Effect[0];
    }
    class TestSkill : SkillCardDefine
    {
        public const int ID = 0x00110000;
        public override int id { get; set; } = ID;
        public override int cost { get; } = 2;
        public override Effect[] effects { get; } = new Effect[0];
    }
    class TestServant : ServantCardDefine
    {
        public const int ID = 0x00110001;
        public override int id { get; set; } = ID;
        public override int cost { get; } = 1;
        public override int attack { get; } = 2;
        public override int life { get; } = 2;
        public override Effect[] effects { get; } = new Effect[0];
    }
}
