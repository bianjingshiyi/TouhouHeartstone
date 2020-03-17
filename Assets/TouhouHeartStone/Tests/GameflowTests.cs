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
            List<IEventArg> eventList = new List<IEventArg>();
            initStandardGame(out var game, eventList);

            _ = game.init();

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
        }

        private static void initStandardGame(out THHGame game, List<IEventArg> eventList, int deckCount = 30)
        {
            TaskExceptionHandler.register();
            game = new THHGame(new TestMaster(), new TestSkill(), new TestServant())
            {
                triggers = new GameObject("TriggerManager").AddComponent<TriggerManager>(),
                logger = new UnityLogger()
            };
            game.createPlayer("玩家1", game.getCardDefine<TestMaster>(), Enumerable.Repeat(game.getCardDefine<TestServant>(), deckCount));
            game.createPlayer("玩家2", game.getCardDefine<TestMaster>(), Enumerable.Repeat(game.getCardDefine<TestServant>(), deckCount));
            game.triggers.onEventAfter += arg =>
            {
                eventList.Add(arg);
            };
        }

        [Test]
        public void initReplaceTest()
        {
            List<IEventArg> eventList = new List<IEventArg>();
            initStandardGame(out var game, eventList);

            _ = game.init();
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
            List<IEventArg> eventList = new List<IEventArg>();
            initStandardGame(out var game, eventList);

            _ = game.init();
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
            List<IEventArg> eventList = new List<IEventArg>();
            initStandardGame(out var game, eventList);

            _ = game.init();
            _ = game.players[0].initReplace(game);
            _ = game.players[1].initReplace(game);
            _ = game.turnEnd(game.sortedPlayers[0]);

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
            List<IEventArg> eventList = new List<IEventArg>();
            initStandardGame(out var game, eventList);

            _ = game.init();
            _ = game.players[0].initReplace(game);
            _ = game.players[1].initReplace(game);
            for (int i = 0; i < 7; i++)
            {
                _ = game.turnEnd(game.sortedPlayers[0]);
                _ = game.turnEnd(game.sortedPlayers[1]);
            }

            THHPlayer.BurnEventArg burn = eventList.LastOrDefault(e => e is THHPlayer.BurnEventArg) as THHPlayer.BurnEventArg;
            Assert.NotNull(burn);
            Assert.AreEqual(game.sortedPlayers[0], burn.player);
            Assert.AreEqual(game.sortedPlayers[0].grave[0], burn.card);
        }
        [Test]
        public void fatigueTest()
        {
            List<IEventArg> eventList = new List<IEventArg>();
            initStandardGame(out var game, eventList, 10);

            _ = game.init();
            _ = game.players[0].initReplace(game);
            _ = game.players[1].initReplace(game);
            for (int i = 0; i < 7; i++)
            {
                _ = game.turnEnd(game.sortedPlayers[0]);
                _ = game.turnEnd(game.sortedPlayers[1]);
            }
            THHPlayer.FatigueEventArg fatigue = eventList.LastOrDefault(e => e is THHPlayer.FatigueEventArg) as THHPlayer.FatigueEventArg;
            Assert.NotNull(fatigue);
            Assert.AreEqual(game.sortedPlayers[0], fatigue.player);
            THHCard.DamageEventArg damage = eventList.LastOrDefault(e => e is THHCard.DamageEventArg) as THHCard.DamageEventArg;
            Assert.NotNull(damage);
            Assert.AreEqual(game.sortedPlayers[0].master, damage.cards[0]);
            Assert.AreEqual(1, damage.value);
        }
        [Test]
        public void attackTest()
        {
            List<IEventArg> eventList = new List<IEventArg>();
            initStandardGame(out var game, eventList, 30);

            _ = game.init();
            _ = game.players[0].initReplace(game);
            _ = game.players[1].initReplace(game);

            _ = game.sortedPlayers[0].tryUse(game, game.sortedPlayers[0].hand[0], 0);
            _ = game.turnEnd(game.sortedPlayers[0]);
            _ = game.sortedPlayers[1].tryUse(game, game.sortedPlayers[1].hand[0], 0);
            _ = game.turnEnd(game.sortedPlayers[1]);
            _ = game.sortedPlayers[0].field[0].tryAttack(game, game.sortedPlayers[1].field[0]);

            THHCard.AttackEventArg attack = eventList.LastOrDefault(e => e is THHCard.AttackEventArg) as THHCard.AttackEventArg;
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
        }
        [Test]
        public void winTest()
        {
            List<IEventArg> eventList = new List<IEventArg>();
            initStandardGame(out var game, eventList, 30);
            _ = game.init();
            _ = game.players[0].initReplace(game);
            _ = game.players[1].initReplace(game);
            _ = game.sortedPlayers[0].tryUse(game, game.sortedPlayers[0].hand[0], 0);
            for (int i = 0; i < 15; i++)
            {
                _ = game.turnEnd(game.sortedPlayers[0]);
                _ = game.turnEnd(game.sortedPlayers[1]);
                _ = game.sortedPlayers[0].field[0].tryAttack(game, game.sortedPlayers[1].master);
            }
            THHCard.AttackEventArg attack = game.triggers.getRecordedEvents().LastOrDefault(e => e is THHCard.AttackEventArg) as THHCard.AttackEventArg;
            Assert.NotNull(attack);
            THHCard.DamageEventArg damage = attack.children[0] as THHCard.DamageEventArg;
            Assert.NotNull(damage);
            THHCard.DeathEventArg death = attack.children[1] as THHCard.DeathEventArg;
            Assert.NotNull(death);
            THHGame.GameEndEventArg gameEnd = game.triggers.getRecordedEvents().LastOrDefault(e => e is THHGame.GameEndEventArg) as THHGame.GameEndEventArg;
            Assert.AreEqual(game.sortedPlayers[0], gameEnd.winners[0]);
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
