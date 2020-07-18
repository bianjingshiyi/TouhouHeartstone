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
using BJSYGameCore;
namespace Tests
{
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
            Assert.AreEqual(30, game.players[0].master.getCurrentLife(game));
            Assert.AreEqual(TestMaster.ID, game.players[1].master.define.id);
            Assert.AreEqual(30, game.players[1].master.getCurrentLife(game));
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
            yield return game.players[0].cmdInitReplace(game, game.players[0].init[0]).wait();
            yield return game.players[1].cmdInitReplace(game, game.players[1].init[0, 1]).wait();
            //替换手牌
            IEventArg[] events = game.triggers.getRecordedEvents();
            THHPlayer.InitReplaceEventArg initReplace = events.OfType<THHPlayer.InitReplaceEventArg>().First();
            Assert.NotNull(initReplace);
            Assert.AreEqual(game.players[0], initReplace.player);
            Assert.AreEqual(1, initReplace.replacedCards.Length);
            initReplace = events.OfType<THHPlayer.InitReplaceEventArg>().Skip(1).First();
            Assert.NotNull(initReplace);
            Assert.AreEqual(game.players[1], initReplace.player);
            Assert.AreEqual(2, initReplace.replacedCards.Length);
            //游戏开始
            THHGame.StartEventArg start = events.OfType<THHGame.StartEventArg>().First();
            Assert.NotNull(start);
            //玩家回合开始
            THHGame.TurnStartEventArg turnStart = events.OfType<THHGame.TurnStartEventArg>().First();
            Assert.NotNull(turnStart);
            Assert.AreEqual(game.sortedPlayers[0], turnStart.player);
            //增加法力水晶并充满
            THHPlayer.SetMaxGemEventArg setMaxGem = events.skipUntil(e => e is THHGame.TurnStartEventArg).OfType<THHPlayer.SetMaxGemEventArg>().First();
            Assert.NotNull(setMaxGem);
            Assert.AreEqual(1, setMaxGem.value);
            THHPlayer.SetGemEventArg setGem = events.OfType<THHPlayer.SetGemEventArg>().First();
            Assert.NotNull(setGem);
            Assert.AreEqual(1, setGem.value);
            //抽一张卡
            THHPlayer.DrawEventArg draw = events.OfType<THHPlayer.DrawEventArg>().First();
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
            Card card = game.sortedPlayers[0].hand[0];
            var task = game.sortedPlayers[0].cmdUse(game, card, 0);
            yield return TestHelper.waitTask(task);
            Assert.True(game.sortedPlayers[0].field.Contains(card));

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

            game.run();
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
            yield break;
        }
        [Test]
        public void turnEndTest_Timeout()
        {
            TestGameflow.createGameWithoutDeck(out var game, out var you, out _);
            game.skipTurnUntil(() => !game.isRunning);
            var fatigue = game.triggers.getRecordedEvents().OfType<THHPlayer.FatigueEventArg>().Last();
            Assert.NotNull(fatigue);
            var damage = game.triggers.getRecordedEvents().skipUntil(e => e == fatigue).OfType<THHCard.DamageEventArg>().First();
            Assert.NotNull(damage);
            var death = game.triggers.getRecordedEvents().skipUntil(e => e == damage).OfType<THHCard.DeathEventArg>().First();
            Assert.NotNull(death);
            var gameEnd = game.triggers.getRecordedEvents().skipUntil(e => e == death).OfType<THHGame.GameEndEventArg>().First();
            Assert.NotNull(gameEnd);
        }
        [UnityTest]
        public IEnumerator turnEndTest_TimeoutWhenUsing()
        {
            TestGameflow.createGame(out var game, out var you, out _, new GameOption()
            {
                shuffle = false,
                timeoutForTurn = 1
            }, new KeyValuePair<int, int>(DiscoverCopy.ID, 1));
            game.skipTurnUntil(() => game.currentPlayer == you);
            you.cmdUse(game, you.hand.getCard<DiscoverCopy>());
            Assert.AreEqual(1, game.answers.getRequests(you.id).Length);
            Assert.NotNull(game.answers.getRequest<DiscoverRequest>(you.id));
            //挂机的阿凯
            yield return new WaitForSeconds(1.1f);
            Assert.AreNotEqual(game.currentPlayer, you);
            //整理一下这里的逻辑
            //超时导致取消所有询问
            //此时的询问中应该只包括发现
            //发现取消，使用继续，TurnLoop继续，导致回合不能结束，继续FreeAct

            //但是正确的情况应该是如何？
            //正确的情况应该是Discover被取消，继续FreeAct
            //然后强制回合结束，这个时候再取消FreeAct，TurnLoop退出
            //到了原本的TurnEnd，但是因为这个时候Turn已经End了，就不会再次触发TurnEnd了
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
        [Test]
        public void fatigueTest_UntilDeath()
        {
            TestGameflow.createGame(out var game, out var you, out var oppo);
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
            roomInfo.setOption(new GameOption());
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
                return local.send(playerInfo);
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
                    return local.send(newRoomInfo);
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
                return Task.CompletedTask;
            };
            host.start();
            local.start();
            yield return local.join(host.ip, host.port).wait();
            Assert.AreEqual(1, roomInfo.playerList.Count);

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
                return remote.send(playerInfo);
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
                return Task.CompletedTask;
            };
            //加入房间
            remote.start();
            yield return remote.join(host.ip, host.port).wait();
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
                    yield return new WaitUntil(() => localPlayer.field[0].getAttackTimes(localGame) > 0);
                    localPlayer.cmdTurnEnd(localGame);
                    yield return new WaitUntil(() => localGame.currentPlayer != localPlayer);
                }
                else if (remoteGame.currentPlayer == remotePlayer)
                {
                    remotePlayer.cmdAttack(remoteGame, remotePlayer.field[0], remoteGame.getOpponent(remotePlayer).master);
                    yield return new WaitUntil(() => remotePlayer.field[0].getAttackTimes(remoteGame) > 0);
                    remotePlayer.cmdTurnEnd(remoteGame);
                    yield return new WaitUntil(() => remoteGame.currentPlayer != remotePlayer);
                }
            }
            while (localGame.isRunning && remoteGame.isRunning);

            local.disconnect();
            remote.disconnect();
            yield break;
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

            Assert.True(game.sortedPlayers[0].field[0].getProp<bool>(game, "TestResult"));
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
            Assert.AreEqual(30, game.sortedPlayers[0].master.getCurrentLife(game));
            game.sortedPlayers[0].cmdAttack(game, game.sortedPlayers[0].field[0], game.sortedPlayers[0].field[1]);
            Assert.AreEqual(7, game.sortedPlayers[0].field[1].getCurrentLife(game));
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
            Assert.True(game.sortedPlayers[0].skill.isUsed(game));  //技能已使用
            Assert.AreEqual(6, game.sortedPlayers[1].field[0].getCurrentLife(game));    //敌方随从受到伤害
            Assert.AreEqual(1, gemNum - game.sortedPlayers[0].gem);     //水晶减1
            gemNum = game.sortedPlayers[0].gem;
            game.sortedPlayers[0].cmdUse(game, game.sortedPlayers[0].hand[0], 0, game.sortedPlayers[1].field[0]);
            Assert.AreEqual(5, game.sortedPlayers[1].field[0].getCurrentLife(game));
            Assert.AreEqual(1, gemNum - game.sortedPlayers[0].gem);     //水晶减1
        }
        [UnityTest]
        public IEnumerator surrenderTest()
        {
            THHGame game = TestGameflow.initStandardGame();
            game.run();
            //yield return new WaitUntil(() =>
            //{
            //    return game.answers.getLastRequest(game.players[0].id) is InitReplaceRequest;
            //});
            yield return game.players[0].cmdSurrender(game).wait();
            THHGame.GameEndEventArg gameEnd = game.triggers.getRecordedEvents().LastOrDefault(e => e is THHGame.GameEndEventArg) as THHGame.GameEndEventArg;
            Assert.AreEqual(game.players[1], gameEnd.winners[0]);
            game.Dispose();
            yield break;
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