﻿using System.Collections;
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
using System.Net;
using System.Net.Sockets;

using TouhouHeartstone.Builtin;

namespace Tests
{
    public static class TestGameflow
    {
        public static THHGame initStandardGame(string name = null, int deckCount = 30, int[] playersId = null, GameOption option = null)
        {
            return initStandardGame(name, playersId,
                Enumerable.Repeat(new TestMaster(), 2).ToArray(),
                Enumerable.Repeat(Enumerable.Repeat(new TestServant(), deckCount).ToArray(), 2).ToArray(),
                option);
        }
        public static THHGame initStandardGame(string name, int[] playersId, MasterCardDefine[] masters, CardDefine[][] decks, GameOption option)
        {
            TaskExceptionHandler.register();
            THHGame game = new THHGame(option != null ? option : GameOption.Default, BuiltinCards.getCardDefines())
            {
                answers = new GameObject(nameof(AnswerManager)).AddComponent<AnswerManager>(),
                triggers = new GameObject("TriggerManager").AddComponent<TriggerManager>(),
                logger = new UnityLogger(name)
            };
            if (playersId == null)
                playersId = new int[] { 1, 2 };
            for (int i = 0; i < playersId.Length; i++)
            {
                game.createPlayer(playersId[i], "玩家" + playersId[i], masters[i], decks[i]);
            }
            return game;
        }
    }
    public class GameflowTests
    {
        [Test]
        public void initTest()
        {
            THHGame game = TestGameflow.initStandardGame();

            _ = game.run();

            THHGame.InitEventArg init = game.triggers.getRecordedEvents().LastOrDefault(e => e is THHGame.InitEventArg) as THHGame.InitEventArg;
            Assert.NotNull(init);
            Assert.AreEqual(TestMaster.ID, game.players[0].master.define.id);
            Assert.AreEqual(30, game.players[0].master.getCurrentLife());
            Assert.AreEqual(TestMaster.ID, game.players[1].master.define.id);
            Assert.AreEqual(30, game.players[1].master.getCurrentLife());
            Assert.AreEqual(2, game.sortedPlayers.Length);
            bool isFirstPlayer = game.getPlayerIndex(game.sortedPlayers[0]) == 0;
            Assert.AreEqual(isFirstPlayer ? 3 : 4, game.players[0].init.count);
            Assert.AreEqual(isFirstPlayer ? 4 : 3, game.players[1].init.count);
            game.Dispose();
        }
        [Test]
        public void initReplaceTest()
        {
            THHGame game = TestGameflow.initStandardGame();

            _ = game.run();
            game.players[0].cmdInitReplace(game, game.players[0].init[0]);
            game.players[1].cmdInitReplace(game, game.players[1].init[0, 1]);
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
            game.Dispose();
        }
        [Test]
        public void useTest()
        {
            THHGame game = TestGameflow.initStandardGame();

            _ = game.run();
            game.players[0].cmdInitReplace(game);
            game.players[1].cmdInitReplace(game);
            game.sortedPlayers[0].cmdUse(game, game.sortedPlayers[0].hand[0], 0);

            THHPlayer.UseEventArg use = game.triggers.getRecordedEvents().FirstOrDefault(e => e is THHPlayer.UseEventArg) as THHPlayer.UseEventArg;
            Assert.NotNull(use);
            Assert.AreEqual(game.sortedPlayers[0], use.player);
            Assert.AreEqual(game.sortedPlayers[0].field[0], use.card);
            Assert.AreEqual(TestServant.ID, use.card.define.id);
            Assert.AreEqual(0, use.position);
            Assert.AreEqual(0, use.targets.Length);
            THHPlayer.SetGemEventArg setGem = game.triggers.getRecordedEvents().LastOrDefault(e => e is THHPlayer.SetGemEventArg) as THHPlayer.SetGemEventArg;
            Assert.AreEqual(0, setGem.value);
            THHPlayer.MoveEventArg summon = game.triggers.getRecordedEvents().LastOrDefault(e => e is THHPlayer.MoveEventArg) as THHPlayer.MoveEventArg;
            Assert.NotNull(summon);
            Assert.AreEqual(game.sortedPlayers[0], summon.player);
            Assert.AreEqual(TestServant.ID, summon.card.define.id);
            Assert.AreEqual(0, summon.position);
            game.Dispose();
        }
        [Test]
        public void turnEndTest()
        {
            THHGame game = TestGameflow.initStandardGame();

            _ = game.run();
            game.players[0].cmdInitReplace(game);
            game.players[1].cmdInitReplace(game);
            game.sortedPlayers[0].cmdTurnEnd(game);

            THHGame.TurnEndEventArg turnEnd = game.triggers.getRecordedEvents().LastOrDefault(e => e is THHGame.TurnEndEventArg) as THHGame.TurnEndEventArg;
            Assert.NotNull(turnEnd);
            Assert.AreEqual(game.sortedPlayers[0], turnEnd.player);
            THHGame.TurnStartEventArg turnStart = game.triggers.getRecordedEvents().LastOrDefault(e => e is THHGame.TurnStartEventArg) as THHGame.TurnStartEventArg;
            Assert.NotNull(turnStart);
            Assert.AreEqual(game.sortedPlayers[1], turnStart.player);
            game.Dispose();
        }
        [Test]
        public void burnTest()
        {
            THHGame game = TestGameflow.initStandardGame();

            _ = game.run();
            game.players[0].cmdInitReplace(game);
            game.players[1].cmdInitReplace(game);
            for (int i = 0; i < 7; i++)
            {
                game.sortedPlayers[0].cmdTurnEnd(game);
                game.sortedPlayers[1].cmdTurnEnd(game);
            }

            THHPlayer.BurnEventArg burn = game.triggers.getRecordedEvents().LastOrDefault(e => e is THHPlayer.BurnEventArg) as THHPlayer.BurnEventArg;
            Assert.NotNull(burn);
            Assert.AreEqual(game.sortedPlayers[0], burn.player);
            Assert.AreEqual(game.sortedPlayers[0].grave[0], burn.card);
            game.Dispose();
        }
        [Test]
        public void fatigueTest()
        {
            THHGame game = TestGameflow.initStandardGame(deckCount: 10);

            _ = game.run();
            game.players[0].cmdInitReplace(game);
            game.players[1].cmdInitReplace(game);
            for (int i = 0; i < 7; i++)
            {
                game.sortedPlayers[0].cmdTurnEnd(game);
                game.sortedPlayers[1].cmdTurnEnd(game);
            }
            THHPlayer.FatigueEventArg fatigue = game.triggers.getRecordedEvents().LastOrDefault(e => e is THHPlayer.FatigueEventArg) as THHPlayer.FatigueEventArg;
            Assert.NotNull(fatigue);
            Assert.AreEqual(game.sortedPlayers[0], fatigue.player);
            THHCard.DamageEventArg damage = game.triggers.getRecordedEvents().LastOrDefault(e => e is THHCard.DamageEventArg) as THHCard.DamageEventArg;
            Assert.NotNull(damage);
            Assert.AreEqual(game.sortedPlayers[0].master, damage.cards[0]);
            Assert.AreEqual(1, damage.value);

            game.Dispose();
        }
        [Test]
        public void attackTest()
        {
            THHGame game = TestGameflow.initStandardGame();

            _ = game.run();
            game.players[0].cmdInitReplace(game);
            game.players[1].cmdInitReplace(game);

            game.sortedPlayers[0].cmdUse(game, game.sortedPlayers[0].hand[0], 0);
            game.sortedPlayers[0].cmdTurnEnd(game);
            game.sortedPlayers[1].cmdUse(game, game.sortedPlayers[1].hand[0], 0);
            game.sortedPlayers[1].cmdTurnEnd(game);
            game.sortedPlayers[0].cmdAttack(game, game.sortedPlayers[0].field[0], game.sortedPlayers[1].field[0]);

            THHCard.AttackEventArg attack = game.triggers.getRecordedEvents().LastOrDefault(e => e is THHCard.AttackEventArg) as THHCard.AttackEventArg;
            Assert.NotNull(attack);
            THHCard.DamageEventArg d1 = attack.children[0] as THHCard.DamageEventArg;
            Assert.NotNull(d1);
            Assert.AreEqual(2, d1.value);
            THHCard.DamageEventArg d2 = attack.children[1] as THHCard.DamageEventArg;
            Assert.NotNull(d2);
            Assert.AreEqual(2, d2.value);
            THHCard.DeathEventArg d3 = attack.children[2] as THHCard.DeathEventArg;
            Assert.NotNull(d3);
            Assert.AreEqual(2, d3.cards.Length);

            game.Dispose();
        }
        [Test]
        public void winTest()
        {
            THHGame game = TestGameflow.initStandardGame();
            _ = game.run();
            game.players[0].cmdInitReplace(game);
            game.players[1].cmdInitReplace(game);
            game.sortedPlayers[0].cmdUse(game, game.sortedPlayers[0].hand[0], 0);
            for (int i = 0; i < 15; i++)
            {
                game.sortedPlayers[0].cmdTurnEnd(game);
                game.sortedPlayers[1].cmdTurnEnd(game);
                game.sortedPlayers[0].cmdAttack(game, game.sortedPlayers[0].field[0], game.sortedPlayers[1].master);
            }
            THHCard.AttackEventArg attack = game.triggers.getRecordedEvents().LastOrDefault(e => e is THHCard.AttackEventArg) as THHCard.AttackEventArg;
            Assert.NotNull(attack);
            THHCard.DamageEventArg damage = attack.children[0] as THHCard.DamageEventArg;
            Assert.NotNull(damage);
            THHCard.DeathEventArg death = attack.children[1] as THHCard.DeathEventArg;
            Assert.NotNull(death);
            THHGame.GameEndEventArg gameEnd = game.triggers.getRecordedEvents().LastOrDefault(e => e is THHGame.GameEndEventArg) as THHGame.GameEndEventArg;
            Assert.AreEqual(game.sortedPlayers[0], gameEnd.winners[0]);
            game.Dispose();
        }
        [UnityTest]
        public IEnumerator closeTest()
        {
            THHGame game = TestGameflow.initStandardGame(option: new GameOption()
            {
                timeout = 5
            });
            Task task = game.run();
            game.close();
            yield return new WaitForSeconds(5.5f);

            Assert.True(task.IsCanceled);
        }
        [UnityTest]
        public IEnumerator remoteGameflowTest()
        {
            THHGame g1 = TestGameflow.initStandardGame(name: "客户端0", playersId: new int[] { 0, 1 }, option: new GameOption()
            {
                sortedPlayers = new int[] { 0, 1 }
            });
            HostManager host = new GameObject(nameof(HostManager)).AddComponent<HostManager>();
            host.logger = g1.logger;
            host.start();
            ClientManager c1 = new GameObject(nameof(ClientManager)).AddComponent<ClientManager>();
            c1.logger = g1.logger;
            c1.start();
            _ = c1.join(Dns.GetHostEntry(Dns.GetHostName()).AddressList.FirstOrDefault(ip => ip.AddressFamily == AddressFamily.InterNetwork)?.ToString(), host.port);
            (g1.answers as AnswerManager).client = c1;

            THHGame g2 = TestGameflow.initStandardGame(name: "客户端1", playersId: new int[] { 0, 1 }, option: new GameOption()
            {
                sortedPlayers = new int[] { 0, 1 }
            });
            ClientManager c2 = new GameObject(nameof(ClientManager)).AddComponent<ClientManager>();
            c2.logger = g2.logger;
            c2.start();
            _ = c2.join(Dns.GetHostEntry(Dns.GetHostName()).AddressList.FirstOrDefault(ip => ip.AddressFamily == AddressFamily.InterNetwork)?.ToString(), host.port);
            (g2.answers as AnswerManager).client = c2;
            yield return new WaitForSeconds(.5f);

            _ = g1.run();
            _ = g2.run();
            g1.players[0].cmdInitReplace(g1, g1.players[0].init[0, 1]);
            g2.players[1].cmdInitReplace(g2);
            yield return new WaitForSeconds(.5f);
            g1.players[0].cmdUse(g1, g1.players[0].hand[0], 0);
            yield return new WaitForSeconds(.5f);
            g1.players[0].cmdTurnEnd(g1);
            yield return new WaitForSeconds(.5f);
            g2.players[1].cmdTurnEnd(g2);
            yield return new WaitForSeconds(.5f);
            g1.players[0].cmdAttack(g1, g1.players[0].field[0], g1.players[1].master);
            yield return new WaitForSeconds(.5f);

            g1.Dispose();
            g2.Dispose();
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
        public override int life { get; } = 30;
        public override int skillID { get; } = TestSkill.ID;
        public override IEffect[] effects { get; } = new Effect[0];
    }
    class TestSkill : SkillCardDefine
    {
        public const int ID = 0x00110000;
        public override int id { get; set; } = ID;
        public override int cost { get; } = 2;
        public override IEffect[] effects { get; } = new Effect[0];
    }
    class TestServant : ServantCardDefine
    {
        public const int ID = 0x00110001;
        public override int id { get; set; } = ID;
        public override int cost { get; } = 1;
        public override int attack { get; } = 2;
        public override int life { get; } = 2;
        public override IEffect[] effects { get; } = new Effect[0];
    }
}