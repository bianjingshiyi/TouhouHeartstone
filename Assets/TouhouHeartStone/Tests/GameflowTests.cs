using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;
using Game;
using NUnit.Framework;
using TouhouCardEngine;
using TouhouCardEngine.Interfaces;
using TouhouHeartstone;
using TouhouHeartstone.Builtin;
using UnityEngine;
using UnityEngine.TestTools;
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
            THHGame game = initGameWithoutPlayers(name, option);
            if (playersId == null)
                playersId = new int[] { 1, 2 };
            if (masters == null)
                masters = Enumerable.Repeat(game.getCardDefine<TestMaster>(), playersId.Length).ToArray();
            if (decks == null)
                decks = Enumerable.Repeat(Enumerable.Repeat(game.getCardDefine<TestServant>(), 30).ToArray(), playersId.Length).ToArray();
            if (option == null)
                option = GameOption.Default;
            for (int i = 0; i < playersId.Length; i++)
            {
                game.createPlayer(playersId[i], "玩家" + playersId[i], masters[i], decks[i]);
            }
            return game;
        }
        public static THHGame initGameWithoutPlayers(string name, GameOption option)
        {
            TaskExceptionHandler.register();
            THHGame game = new THHGame(option != null ? option : GameOption.Default, CardHelper.getCardDefines())
            {
                answers = new GameObject(nameof(AnswerManager)).AddComponent<AnswerManager>(),
                triggers = new GameObject(nameof(TriggerManager)).AddComponent<TriggerManager>(),
                time = new GameObject(nameof(TimeManager)).AddComponent<TimeManager>(),
                logger = new ULogger(name)
            };
            (game.triggers as TriggerManager).logger = game.logger;
            return game;
        }
    }
    public class GameflowTests
    {
        [UnityTest]
        public IEnumerator initTest()
        {
            THHGame game = TestGameflow.initStandardGame();

            _ = game.run();
            yield return new WaitForSeconds(.1f);

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
        [UnityTest]
        public IEnumerator initReplaceTest()
        {
            THHGame game = TestGameflow.initStandardGame();

            _ = game.run();
            yield return new WaitForSeconds(.1f);
            game.players[0].cmdInitReplace(game, game.players[0].init[0]);
            game.players[1].cmdInitReplace(game, game.players[1].init[0, 1]);
            yield return new WaitForSeconds(.1f);
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
        [UnityTest]
        public IEnumerator initReplaceRefuseTest()
        {
            THHGame game = TestGameflow.initStandardGame();

            _ = game.run();
            yield return new WaitForSeconds(.1f);
            game.players[0].cmdInitReplace(game);
            game.players[1].cmdInitReplace(game);
            yield return new WaitForSeconds(.1f);
            game.sortedPlayers[0].cmdUse(game, game.sortedPlayers[0].hand[0], 0);
            yield return new WaitForSeconds(.1f);

            game.players[0].cmdInitReplace(game);
            game.players[1].cmdInitReplace(game);

            var args = game.triggers.getRecordedEvents().Where(e => e is THHPlayer.InitReplaceEventArg);
            Assert.AreEqual(args.Count(), 2);
            Assert.AreEqual(((THHPlayer.InitReplaceEventArg)args.ElementAt(0)).player, game.players[0]);
            Assert.AreEqual(((THHPlayer.InitReplaceEventArg)args.ElementAt(1)).player, game.players[1]);

            game.Dispose();
        }

        [UnityTest]
        public IEnumerator useTest()
        {
            THHGame game = TestGameflow.initStandardGame();

            _ = game.run();
            yield return new WaitForSeconds(.1f);
            game.players[0].cmdInitReplace(game);
            game.players[1].cmdInitReplace(game);
            yield return new WaitForSeconds(.1f);
            var task = game.sortedPlayers[0].cmdUse(game, game.sortedPlayers[0].hand[0], 0);
            yield return 

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
        [UnityTest]
        public IEnumerator turnEndTest()
        {
            THHGame game = TestGameflow.initStandardGame();

            _ = game.run();
            yield return new WaitForSeconds(.1f);
            game.players[0].cmdInitReplace(game);
            game.players[1].cmdInitReplace(game);
            yield return new WaitForSeconds(.1f);
            game.sortedPlayers[0].cmdTurnEnd(game);
            yield return new WaitForSeconds(.1f);

            THHGame.TurnEndEventArg turnEnd = game.triggers.getRecordedEvents().LastOrDefault(e => e is THHGame.TurnEndEventArg) as THHGame.TurnEndEventArg;
            Assert.NotNull(turnEnd);
            Assert.AreEqual(game.sortedPlayers[0], turnEnd.player);
            THHGame.TurnStartEventArg turnStart = game.triggers.getRecordedEvents().LastOrDefault(e => e is THHGame.TurnStartEventArg) as THHGame.TurnStartEventArg;
            Assert.NotNull(turnStart);
            Assert.AreEqual(game.sortedPlayers[1], turnStart.player);
            game.Dispose();
        }
        [UnityTest]
        public IEnumerator burnTest()
        {
            THHGame game = TestGameflow.initStandardGame();

            _ = game.run();
            yield return new WaitForSeconds(.1f);
            game.players[0].cmdInitReplace(game);
            game.players[1].cmdInitReplace(game);
            yield return new WaitForSeconds(.1f);
            for (int i = 0; i < 7; i++)
            {
                game.sortedPlayers[0].cmdTurnEnd(game);
                yield return new WaitForSeconds(.1f);
                game.sortedPlayers[1].cmdTurnEnd(game);
                yield return new WaitForSeconds(.1f);
            }

            THHPlayer.BurnEventArg burn = game.triggers.getRecordedEvents().LastOrDefault(e => e is THHPlayer.BurnEventArg) as THHPlayer.BurnEventArg;
            Assert.NotNull(burn);
            Assert.AreEqual(game.sortedPlayers[0], burn.player);
            Assert.AreEqual(game.sortedPlayers[0].grave[0], burn.card);
            game.Dispose();
        }
        [UnityTest]
        public IEnumerator fatigueTest()
        {
            THHGame game = TestGameflow.initStandardGame(deckCount: 10);

            _ = game.run();
            yield return new WaitForSeconds(.1f);
            game.players[0].cmdInitReplace(game);
            game.players[1].cmdInitReplace(game);
            yield return new WaitForSeconds(.1f);
            for (int i = 0; i < 7; i++)
            {
                game.sortedPlayers[0].cmdTurnEnd(game);
                yield return new WaitForSeconds(.1f);
                game.sortedPlayers[1].cmdTurnEnd(game);
                yield return new WaitForSeconds(.1f);
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
        [UnityTest]
        public IEnumerator attackTest()
        {
            THHGame game = TestGameflow.initStandardGame();

            _ = game.run();
            yield return new WaitForSeconds(.2f);
            game.players[0].cmdInitReplace(game);
            game.players[1].cmdInitReplace(game);
            yield return new WaitForSeconds(.1f);
            game.sortedPlayers[0].cmdUse(game, game.sortedPlayers[0].hand[0], 0);
            yield return new WaitForSeconds(.1f);
            game.sortedPlayers[0].cmdTurnEnd(game);
            yield return new WaitForSeconds(.1f);
            game.sortedPlayers[1].cmdUse(game, game.sortedPlayers[1].hand[0], 0);
            yield return new WaitForSeconds(.1f);
            game.sortedPlayers[1].cmdTurnEnd(game);
            yield return new WaitForSeconds(.1f);
            game.sortedPlayers[0].cmdAttack(game, game.sortedPlayers[0].field[0], game.sortedPlayers[1].field[0]);
            yield return new WaitForSeconds(.1f);

            THHCard.AttackEventArg attack = game.triggers.getRecordedEvents().LastOrDefault(e => e is THHCard.AttackEventArg) as THHCard.AttackEventArg;
            Assert.NotNull(attack);
            THHCard.DamageEventArg d1 = attack.children[0] as THHCard.DamageEventArg;
            Assert.NotNull(d1);
            Assert.AreEqual(2, d1.value);
            THHCard.DamageEventArg d2 = attack.children[1] as THHCard.DamageEventArg;
            Assert.NotNull(d2);
            Assert.AreEqual(2, d2.value);
            THHCard.DeathEventArg d3 = game.triggers.getRecordedEvents().LastOrDefault(e => e is THHCard.DeathEventArg) as THHCard.DeathEventArg;
            Assert.NotNull(d3);
            Assert.AreEqual(2, d3.infoDic.Count);

            game.Dispose();
        }
        [UnityTest]
        public IEnumerator winTest()
        {
            THHGame game = TestGameflow.initStandardGame();
            _ = game.run();
            yield return new WaitForSeconds(.1f);
            game.players[0].cmdInitReplace(game);
            game.players[1].cmdInitReplace(game);
            yield return new WaitForSeconds(.1f);
            game.sortedPlayers[0].cmdUse(game, game.sortedPlayers[0].hand[0], 0);
            yield return new WaitForSeconds(.1f);
            for (int i = 0; i < 15; i++)
            {
                game.sortedPlayers[0].cmdTurnEnd(game);
                yield return new WaitForSeconds(.1f);
                game.sortedPlayers[1].cmdTurnEnd(game);
                yield return new WaitForSeconds(.1f);
                game.sortedPlayers[0].cmdAttack(game, game.sortedPlayers[0].field[0], game.sortedPlayers[1].master);
                yield return new WaitForSeconds(.1f);
            }
            THHCard.AttackEventArg attack = game.triggers.getRecordedEvents().LastOrDefault(e => e is THHCard.AttackEventArg) as THHCard.AttackEventArg;
            Assert.NotNull(attack);
            THHGame.GameEndEventArg gameEnd = game.triggers.getRecordedEvents().LastOrDefault(e => e is THHGame.GameEndEventArg) as THHGame.GameEndEventArg;
            Assert.AreEqual(game.sortedPlayers[0], gameEnd.winners[0]);
            game.Dispose();
        }
        [UnityTest]
        public IEnumerator closeTest()
        {
            THHGame game = TestGameflow.initStandardGame(option: new GameOption()
            {
                timeoutForInitReplace = 5
            });
            Task task = game.run();
            game.close();
            yield return new WaitForSeconds(5.5f);

            Assert.False(game.isRunning);
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
            Task task = c1.join(Dns.GetHostEntry(Dns.GetHostName()).AddressList.FirstOrDefault(ip => ip.AddressFamily == AddressFamily.InterNetwork)?.ToString(), host.port);
            (g1.answers as AnswerManager).client = c1;

            THHGame g2 = TestGameflow.initStandardGame(name: "客户端1", playersId: new int[] { 0, 1 }, option: new GameOption()
            {
                sortedPlayers = new int[] { 0, 1 }
            });
            ClientManager c2 = new GameObject(nameof(ClientManager)).AddComponent<ClientManager>();
            c2.logger = g2.logger;
            c2.start();
            task = c2.join(Dns.GetHostEntry(Dns.GetHostName()).AddressList.FirstOrDefault(ip => ip.AddressFamily == AddressFamily.InterNetwork)?.ToString(), host.port);
            (g2.answers as AnswerManager).client = c2;
            yield return new WaitUntil(() => task.IsCompleted);

            _ = g1.run();
            _ = g2.run();
            yield return new WaitForSeconds(.5f);
            g1.players[0].cmdInitReplace(g1, g1.players[0].init[0, 1]);
            yield return new WaitForSeconds(.5f);
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
        [UnityTest]
        public IEnumerator remotePVPSimulTest()
        {
            UnityLogger logger = new UnityLogger();
            HostManager host = new GameObject(nameof(HostManager)).AddComponent<HostManager>();
            host.logger = logger;
            ClientManager local = new GameObject(nameof(ClientManager)).AddComponent<ClientManager>();
            local.logger = logger;
            //开房，打开Host，自己加入自己，房间应该有Option
            RoomInfo roomInfo = new RoomInfo();
            THHGame localGame = null;
            local.onConnected += () =>
            {
                //发送玩家信息
                RoomPlayerInfo playerInfo = new RoomPlayerInfo()
                {
                    id = local.id,
                    name = "玩家" + local.id,
                };
                playerInfo.setDeck(new int[] { Reimu.ID }.Concat(Enumerable.Repeat(DrizzleFairy.ID, 30)).ToArray());
                _ = local.send(playerInfo);
            };
            local.onReceive += (id, obj) =>
            {
                if (obj is RoomPlayerInfo newPlayerInfo)
                {
                    //收到玩家信息
                    RoomInfo newRoomInfo = new RoomInfo()
                    {
                        playerList = new List<RoomPlayerInfo>(roomInfo.playerList)
                    };
                    newRoomInfo.setOption(roomInfo.getOption());
                    newRoomInfo.playerList.Add(newPlayerInfo);
                    //发送房间信息
                    _ = local.send(newRoomInfo);
                }
                else if (obj is RoomInfo newRoomInfo)
                {
                    roomInfo = newRoomInfo;
                    //收到房间信息
                    if (newRoomInfo.playerList.Count > 1)
                    {
                        localGame = TestGameflow.initGameWithoutPlayers("本地游戏", newRoomInfo.getOption());
                        (localGame.answers as AnswerManager).client = local;
                        foreach (var playerInfo in newRoomInfo.playerList)
                        {
                            localGame.createPlayer(playerInfo.id, "玩家" + playerInfo.id, localGame.getCardDefine<MasterCardDefine>(playerInfo.getDeck()[0]), playerInfo.getDeck().Skip(1).Select(did => localGame.getCardDefine(did)));
                        }
                        localGame.run();
                    }
                }
            };
            host.start();
            local.start();
            string address = Dns.GetHostEntry(Dns.GetHostName()).AddressList.FirstOrDefault(ip => ip.AddressFamily == AddressFamily.InterNetwork)?.ToString();
            Task task = local.join(address, host.port);
            yield return new WaitUntil(() => task.IsCompleted && roomInfo.playerList.Count > 0);

            ClientManager remote = new GameObject(nameof(ClientManager)).AddComponent<ClientManager>();
            remote.logger = logger;
            THHGame remoteGame = null;
            remote.onConnected += () =>
            {
                //发送玩家信息
                RoomPlayerInfo playerInfo = new RoomPlayerInfo()
                {
                    id = remote.id,
                    name = "玩家" + remote.id
                };
                playerInfo.setDeck(new int[] { Reimu.ID }.Concat(Enumerable.Repeat(DrizzleFairy.ID, 30)).ToArray());
                _ = remote.send(playerInfo);
            };
            remote.onReceive += (id, obj) =>
            {
                if (obj is RoomInfo newRoomInfo)
                {
                    //收到房间信息
                    if (newRoomInfo.playerList.Count > 1)
                    {
                        remoteGame = TestGameflow.initGameWithoutPlayers("远端游戏", newRoomInfo.getOption());
                        (remoteGame.answers as AnswerManager).client = remote;
                        foreach (var playerInfo in newRoomInfo.playerList)
                        {
                            remoteGame.createPlayer(playerInfo.id, "玩家" + playerInfo.id, remoteGame.getCardDefine<MasterCardDefine>(playerInfo.getDeck()[0]), playerInfo.getDeck().Skip(1).Select(did => remoteGame.getCardDefine(did)));
                        }
                        remoteGame.run();
                    }
                }
            };
            //加入房间
            remote.start();
            task = remote.join(address, host.port);
            yield return new WaitUntil(() => task.IsCompleted && roomInfo.playerList.Count > 1);
            //连接了，远程玩家把玩家信息发给本地，本地更新房间信息发给远端和开始游戏。
            yield return new WaitUntil(() => localGame != null && remoteGame != null);

            Assert.True(localGame.isRunning);
            Assert.AreEqual(local.id, localGame.players[0].id);
            Assert.AreEqual(remote.id, localGame.players[1].id);
            Assert.True(remoteGame.isRunning);
            Assert.AreEqual(local.id, remoteGame.players[0].id);
            Assert.AreEqual(remote.id, remoteGame.players[1].id);

            THHPlayer localPlayer = localGame.getPlayer(local.id);
            Assert.AreEqual(0, localPlayer.id);
            yield return new WaitUntil(() => localGame.answers.getRequests(localPlayer.id).FirstOrDefault() is InitReplaceRequest);
            Assert.Greater(localPlayer.init.count, 0);
            localPlayer.cmdInitReplace(localGame);
            yield return new WaitUntil(() => localGame.answers.getResponse(localPlayer.id, localGame.answers.getRequests(localPlayer.id).FirstOrDefault()) is InitReplaceResponse);

            THHPlayer remotePlayer = remoteGame.getPlayer(remote.id);
            Assert.AreEqual(1, remotePlayer.id);
            yield return new WaitUntil(() => remoteGame.answers.getRequests(remotePlayer.id).FirstOrDefault() is InitReplaceRequest);
            Assert.Greater(remotePlayer.init.count, 0);
            remotePlayer.cmdInitReplace(remoteGame);
            yield return new WaitUntil(() => remoteGame.triggers.getRecordedEvents().Any(e => e is THHGame.StartEventArg));
            //拍怪
            if (localGame.sortedPlayers[0] == localPlayer)
            {
                yield return new WaitUntil(() => localGame.answers.getRequests(localPlayer.id).FirstOrDefault() is FreeActRequest);
                localPlayer.cmdUse(localGame, localPlayer.hand[0], 0);
                yield return new WaitUntil(() => localPlayer.field.count > 0);
                localPlayer.cmdTurnEnd(localGame);
                yield return new WaitUntil(() => localGame.currentPlayer != localPlayer);
            }
            yield return new WaitUntil(() => remoteGame.answers.getRequests(remotePlayer.id).FirstOrDefault() is FreeActRequest);
            remotePlayer.cmdUse(remoteGame, remotePlayer.hand[0], 0);
            yield return new WaitUntil(() => remotePlayer.field.count > 0);
            remotePlayer.cmdTurnEnd(remoteGame);
            yield return new WaitUntil(() => remoteGame.currentPlayer != remotePlayer);
            if (localGame.sortedPlayers[0] != localPlayer)
            {
                yield return new WaitUntil(() => localGame.answers.getRequests(localPlayer.id).FirstOrDefault() is FreeActRequest);
                localPlayer.cmdUse(localGame, localPlayer.hand[0], 0);
                yield return new WaitUntil(() => localPlayer.field.count > 0);
                localPlayer.cmdTurnEnd(localGame);
                yield return new WaitUntil(() => localGame.currentPlayer != localPlayer);
            }
            do
            {
                yield return new WaitUntil(() => localGame.currentPlayer == localPlayer || remoteGame.currentPlayer == remotePlayer);
                if (localGame.currentPlayer == localPlayer)
                {
                    localPlayer.cmdAttack(localGame, localPlayer.field[0], localGame.getOpponent(localPlayer).master);
                    yield return new WaitUntil(() => localPlayer.field[0].getAttackTimes() > 0);
                    localPlayer.cmdTurnEnd(localGame);
                    yield return new WaitUntil(() => localGame.currentPlayer != localPlayer);
                }
                else if (remoteGame.currentPlayer == remotePlayer)
                {
                    remotePlayer.cmdAttack(remoteGame, remotePlayer.field[0], remoteGame.getOpponent(remotePlayer).master);
                    yield return new WaitUntil(() => remotePlayer.field[0].getAttackTimes() > 0);
                    remotePlayer.cmdTurnEnd(remoteGame);
                    yield return new WaitUntil(() => remoteGame.currentPlayer != remotePlayer);
                }
            }
            while (localGame.isRunning && remoteGame.isRunning);

            local.disconnect();
            remote.disconnect();
        }
        [UnityTest]
        public IEnumerator effectRegisterTest()
        {
            THHGame game = TestGameflow.initGameWithoutPlayers(null, GameOption.Default);
            game.createPlayer(1, "玩家1", game.getCardDefine<TestMaster>(), Enumerable.Repeat(game.getCardDefine<TestServant_TurnEndEffect>(), 30));
            game.createPlayer(2, "玩家2", game.getCardDefine<TestMaster>(), Enumerable.Repeat(game.getCardDefine<TestServant_TurnEndEffect>(), 30));
            _ = game.run();
            yield return new WaitForSeconds(.1f);
            game.players[0].cmdInitReplace(game);
            game.players[1].cmdInitReplace(game);
            yield return new WaitForSeconds(.1f);
            game.sortedPlayers[0].cmdUse(game, game.sortedPlayers[0].hand[0], 0);
            yield return new WaitForSeconds(.1f);
            game.sortedPlayers[0].cmdTurnEnd(game);
            yield return new WaitForSeconds(.1f);

            Assert.True(game.sortedPlayers[0].field[0].getProp<bool>("TestResult"));
        }
        [Test]
        public void attackSelfTest()
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
            Assert.AreEqual(30, game.sortedPlayers[0].master.getCurrentLife());
            game.sortedPlayers[0].cmdAttack(game, game.sortedPlayers[0].field[0], game.sortedPlayers[0].field[1]);
            Assert.AreEqual(7, game.sortedPlayers[0].field[1].getCurrentLife());
        }


        /// <summary>
        /// 使用法术以及符卡时能正常消耗水晶的测试
        /// </summary>
        [Test]
        public void SkillAndSpellCardTest()
        {
            THHGame game = TestGameflow.initGameWithoutPlayers(null, new GameOption()
            {
                shuffle = false
            });
            game.createPlayer(0, "玩家0", game.getCardDefine<TestMaster2>(), Enumerable.Repeat(game.getCardDefine<DefaultServant>() as CardDefine, 28)
            .Concat(Enumerable.Repeat(game.getCardDefine<DefaultServant>(), 2)));
            game.createPlayer(1, "玩家1", game.getCardDefine<TestMaster2>(), Enumerable.Repeat(game.getCardDefine<DefaultServant>() as CardDefine, 29)
            .Concat(Enumerable.Repeat(game.getCardDefine<TestSpellCard>(), 1)));
            game.run();
            game.sortedPlayers[0].cmdInitReplace(game);
            game.sortedPlayers[1].cmdInitReplace(game);

            game.sortedPlayers[0].cmdTurnEnd(game);
            game.sortedPlayers[1].cmdUse(game, game.sortedPlayers[1].hand[0], 0);
            game.sortedPlayers[1].cmdTurnEnd(game);
            int gemNum = game.sortedPlayers[0].gem;
            game.sortedPlayers[0].cmdUse(game, game.sortedPlayers[0].skill, 0, game.sortedPlayers[1].field[0]);
            Assert.True(game.sortedPlayers[0].skill.isUsed());  //技能已使用
            Assert.AreEqual(6, game.sortedPlayers[1].field[0].getCurrentLife());    //敌方随从受到伤害
            Assert.AreEqual(1, gemNum - game.sortedPlayers[0].gem);     //水晶减1
            gemNum = game.sortedPlayers[0].gem;
            game.sortedPlayers[0].cmdUse(game, game.sortedPlayers[0].hand[0], 0, game.sortedPlayers[1].field[0]);
            Assert.AreEqual(5, game.sortedPlayers[1].field[0].getCurrentLife());
            Assert.AreEqual(1, gemNum - game.sortedPlayers[0].gem);     //水晶减1
        }
        [UnityTest]
        public IEnumerator surrenderTest()
        {
            THHGame game = TestGameflow.initStandardGame();
            Task task = game.run();
            yield return new WaitUntil(() => game.answers.getLastRequest(game.players[0].id) is InitReplaceRequest);
            game.players[0].cmdSurrender(game);
            yield return new WaitUntil(() => game.triggers.getRecordedEvents().LastOrDefault() is THHGame.GameEndEventArg);
            THHGame.GameEndEventArg gameEnd = game.triggers.getRecordedEvents().LastOrDefault(e => e is THHGame.GameEndEventArg) as THHGame.GameEndEventArg;
            Assert.AreEqual(game.players[1], gameEnd.winners[0]);
            game.Dispose();
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
                    if (obj == null)
                        return;
                    if (obj.Exception.InnerExceptions != null && obj.Exception.InnerExceptions.Count > 1)
                    {
                        foreach (var exception in obj.Exception.InnerExceptions)
                        {
                            Debug.LogError(exception);
                        }
                    }
                    else if (obj.Exception != null && obj.Exception.InnerException != null)
                        Debug.LogError(obj.Exception.InnerException);
                    else
                        Debug.LogError(obj.Exception);
                    obj.SetObserved();
                };
                registered = true;
            }
        }
    }
}