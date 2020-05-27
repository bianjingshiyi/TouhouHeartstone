using System.Collections;
using System.Collections.Generic;
using System.Threading;
using NUnit.Framework;
using System.IO;
using UnityEngine;
using UnityEngine.TestTools;
using System.Linq;
using TouhouCardEngine;
using TouhouCardEngine.Interfaces;
using TouhouHeartstone;
using System.Threading.Tasks;
using System.Net;
using System.Net.Sockets;
using System.Xml.Serialization;
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
                triggers = new GameObject("TriggerManager").AddComponent<TriggerManager>(),
                time = new GameObject(nameof(TimeManager)).AddComponent<TimeManager>(),
                logger = new ULogger()
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
        public IEnumerator useTest()
        {
            THHGame game = TestGameflow.initStandardGame();

            _ = game.run();
            yield return new WaitForSeconds(.1f);
            game.players[0].cmdInitReplace(game);
            game.players[1].cmdInitReplace(game);
            yield return new WaitForSeconds(.1f);
            game.sortedPlayers[0].cmdUse(game, game.sortedPlayers[0].hand[0], 0);
            yield return new WaitForSeconds(.1f);

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
    class TestMaster : MasterCardDefine
    {
        public const int ID = 0x00100000;
        public override int id { get; set; } = ID;
        public override int life { get; set; } = 30;
        public override int skillID { get; set; } = TestSkill.ID;
        public override IEffect[] effects { get; set; } = new Effect[0];
    }
    class TestSkill : SkillCardDefine
    {
        public const int ID = 0x00110000;
        public override int id { get; set; } = ID;
        public override int cost { get; set; } = 2;
        public override IEffect[] effects { get; set; } = new Effect[0];
    }
    class TestServant : ServantCardDefine
    {
        public const int ID = 0x00110001;
        public override int id { get; set; } = ID;
        public override int cost { get; set; } = 1;
        public override int attack { get; set; } = 2;
        public override int life { get; set; } = 2;
        public override IEffect[] effects { get; set; } = new Effect[0];
    }
    class TestServant_TurnEndEffect : ServantCardDefine
    {
        public const int ID = 0x00110002;
        public override int id { get; set; } = ID;
        public override int cost { get; set; } = 1;
        public override int attack { get; set; } = 1;
        public override int life { get; set; } = 2;
        public override IEffect[] effects { get; set; } = new IEffect[]
        {
            new THHEffectBefore<THHGame.TurnEndEventArg>(PileName.FIELD,(game,card,arg)=>
            {
                return true;
            },(game,card,targets)=>
            {
                return true;
            },(game,card,arg)=>
            {
                card.setProp("TestResult",true);
                return Task.CompletedTask;
            })
        };
    }
    class TestServant_ZeroAttack : ServantCardDefine
    {
        public const int ID = 0x00110003;
        public override int id { get; set; } = ID;
        public override int cost { get; set; } = 1;
        public override int attack { get; set; } = 0;
        public override int life { get; set; } = 4;
        public override IEffect[] effects { get; set; } = new IEffect[]
        {
        };
    }
    class TestServant_Reverse : ServantCardDefine
    {
        public const int ID = 0x00110004;
        public override int id { get; set; } = ID;
        public override int cost { get; set; } = 2;
        public override int attack { get; set; } = 2;
        public override int life { get; set; } = 2;
        public override IEffect[] effects { get; set; } = new IEffect[]
        {
            new THHEffect<THHPlayer.ActiveEventArg>(PileName.NONE, (game,card,arg)=>
            {
                return true;
            },(game,card,targets)=>
            {
                return true;
            },(game,card,arg,targets)=>
            {
                return Task.CompletedTask;
            })
        };
    }
    class TestServant_Buff : ServantCardDefine
    {
        public const int ID = 0x00110005;
        public override int id { get; set; } = ID;
        public override int cost { get; set; } = 1;
        public override int attack { get; set; } = 1;
        public override int life { get; set; } = 1;
        public override IEffect[] effects { get; set; } = new IEffect[]
        {
            new THHEffect<THHPlayer.ActiveEventArg>(PileName.NONE, (game,card,arg)=>
            {
                return true;
            },(game,card,targets)=>
            {
                return true;
            },(game,card,arg,targets)=>
            {
                card.addBuff(game,new TestBuff());
                return Task.CompletedTask;
            })
        };
        class TestBuff : Buff
        {
            public const int ID = 0x001;
            public override int id { get; } = ID;
            public override PropModifier[] modifiers { get; } = new PropModifier[]
            {
                new AttackModifier(1),
                new LifeModifier(1)
            };
        }
    }
    /// <summary>
    /// 一个会突袭的随从
    /// </summary>
    public class RushServant : ServantCardDefine
    {
        public const int ID = 0x00110006;
        public override int id { get; set; } = ID;
        public override int cost { get; set; } = 1;
        public override int attack { get; set; } = 1;
        public override int life { get; set; } = 3;
        public override string[] tags { get; set; } = new string[0];
        public override string[] keywords { get; set; } = new string[] { Keyword.RUSH };
        public override IEffect[] effects { get; set; } = new IEffect[0];
    }
    /// <summary>
    /// 一个会圣盾的随从
    /// </summary>
    public class ShieldServant : ServantCardDefine
    {
        public const int ID = 0x00110007;
        public override int id { get; set; } = ID;
        public override int cost { get; set; } = 1;
        public override int attack { get; set; } = 1;
        public override int life { get; set; } = 3;
        public override string[] tags { get; set; } = new string[0];
        public override string[] keywords { get; set; } = new string[] { Keyword.SHIELD };
        public override IEffect[] effects { get; set; } = new IEffect[0];
    }
    /// <summary>
    /// 会潜行的随从
    /// </summary>
    public class StealthServant : ServantCardDefine
    {
        public const int ID = 0x00110008;
        public override int id { get; set; } = ID;
        public override int cost { get; set; } = 1;
        public override int attack { get; set; } = 1;
        public override int life { get; set; } = 3;
        public override string[] tags { get; set; } = new string[0];
        public override string[] keywords { get; set; } = new string[] { Keyword.STEALTH };
        public override IEffect[] effects { get; set; } = new IEffect[]
        {
            new THHEffectBefore<THHGame.TurnEndEventArg>(PileName.FIELD,(game,card,arg)=>
            {
                return true;
            },(game,card,targets)=>
            {
                return true;
            },(game,card,arg)=>
            {
                game.getPlayerForNextTurn(arg.player).master.damage(game,card,1);
                return Task.CompletedTask;
            })
        };

    }

    /// <summary>
    /// 会吸血的随从
    /// </summary>
    public class DrainServant : ServantCardDefine
    {
        public const int ID = 0x00110009;
        public override int id { get; set; } = ID;
        public override int cost { get; set; } = 1;
        public override int attack { get; set; } = 1;
        public override int life { get; set; } = 3;
        public override string[] tags { get; set; } = new string[] { };
        public override string[] keywords { get; set; } = new string[] { Keyword.DRAIN };
        public override IEffect[] effects { get; set; } = new IEffect[0];
    }

    /// <summary>
    /// 剧毒随从
    /// </summary>
    public class PoisonousServant : ServantCardDefine
    {
        public const int ID = 0x0011000A;
        public override int id { get; set; } = ID;
        public override int cost { get; set; } = 1;
        public override int attack { get; set; } = 1;
        public override int life { get; set; } = 3;
        public override string[] tags { get; set; } = new string[] { };
        public override string[] keywords { get; set; } = new string[] { Keyword.POISONOUS };
        public override IEffect[] effects { get; set; } = new IEffect[0];
    }

    /// <summary>
    /// 魔免随从
    /// </summary>
    public class MaginImmuneServant : ServantCardDefine
    {
        public const int ID = 0x0011000B;
        public override int id { get; set; } = ID;
        public override int cost { get; set; } = 1;
        public override int attack { get; set; } = 1;
        public override int life { get; set; } = 3;
        public override string[] tags { get; set; } = new string[] { };
        public override string[] keywords { get; set; } = new string[] { Keyword.MAGICIMMUNE };
        public override IEffect[] effects { get; set; } = new IEffect[0];
    }

    /// <summary>
    /// 拥有指定单个敌人攻击的技能的Master
    /// </summary>
    public class TestMaster2 : MasterCardDefine
    {
        public const int ID = 0x0011000C;
        public override int id { get; set; } = ID;
        public override int life { get; set; } = 30;
        public override int skillID { get; set; } = TestDamageSkill.ID;
        public override IEffect[] effects { get; set; } = new Effect[0];
    }
    class TestDamageSkill : SkillCardDefine
    {
        public const int ID = 0x0011000D;
        public override int id { get; set; } = ID;
        public override int cost { get; set; } = 1;
        public override IEffect[] effects { get; set; } = new IEffect[]
        {
            new THHEffect<THHPlayer.ActiveEventArg>("Skill",(game,card,arg)=>
            {
                return true;
            },(game,card,targets)=>
            {
                if(targets[0] is Card target)
                    return true;
                return false;
            },async (game,card,arg,targets)=>
            {
                if(targets[0] is Card target)
                    await target.damage(game,card,1);
            })
        };

    }

    /// <summary>
    /// 单体指向型攻击的spellcard
    /// </summary>
    public class TestSpellCard : SpellCardDefine
    {
        public const int ID = 0x0011000E;
        public override int id { get; set; } = ID;
        public override int cost { get; set; } = 1;
        public override IEffect[] effects { get; set; } = new IEffect[]
        {
            new THHEffect<THHPlayer.ActiveEventArg>(PileName.NONE,(game,card,arg)=>
            {
                return true;
            },(game,card,targets)=>
            {
                if(targets[0] is Card target)
                    return true;
                return false;
            },async (game,card,arg,targets)=>
            {
                if(targets[0] is Card target)
                    await target.damage(game,card,1);
            })
        };
    }

    /// <summary>
    /// 一只白板的挨打用随从
    /// </summary>
    public class DefaultServant : ServantCardDefine
    {
        public const int ID = 0x0011FFFF;
        public override int id { get; set; } = ID;
        public override int cost { get; set; } = 1;
        public override int attack { get; set; } = 1;
        public override int life { get; set; } = 7;
        public override string[] tags { get; set; } = new string[0];
        public override string[] keywords { get; set; } = new string[0];
        public override IEffect[] effects { get; set; } = new IEffect[0];
    }
    public class MountainGaint : ServantCardDefine
    {
        public const int ID = 0x0011000B;
        public override int id { get; set; } = ID;
        public override int cost { get; set; } = 12;
        public override int attack { get; set; } = 8;
        public override int life { get; set; } = 8;
        public override IEffect[] effects { get; set; } = new IEffect[]
        {
            new CostFixer()
        };
        class CostFixer : IEffect
        {
            public string[] events => throw new System.NotImplementedException();
            public string[] piles { get; } = new string[] { PileName.HAND };
            public bool checkCondition(IGame game, ICard card, object[] vars)
            {
                throw new System.NotImplementedException();
            }
            public bool checkTarget(IGame game, ICard card, object[] vars, object[] targets)
            {
                throw new System.NotImplementedException();
            }
            public Task execute(IGame game, ICard card, object[] vars, object[] targets)
            {
                throw new System.NotImplementedException();
            }
            public string[] getEvents(ITriggerManager manager)
            {
                return new string[0];
            }
            CostModifier _modifier = new CostModifier();
            public void register(IGame game, ICard card)
            {
                card.addModifier(game, _modifier);
            }
            public void unregister(IGame game, ICard card)
            {
                card.removeModifier(game, _modifier);
            }
            class CostModifier : PropModifier<int>
            {
                public override string propName { get; } = nameof(ServantCardDefine.cost);
                public override int calc(Card card, int value)
                {
                    return value - card.getOwner().hand.count + 1;
                }
            }
        }
    }
}