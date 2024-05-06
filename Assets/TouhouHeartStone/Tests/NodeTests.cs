using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Game;
using NUnit.Framework;
using TouhouCardEngine;
using TouhouHeartstone;
using UnityEngine;
using UnityEngine.TestTools;

namespace Tests
{
    public class NodeTests
    {
        /// <summary>
        /// 空输入值测试
        /// </summary>
        /// <returns></returns>
        [UnityTest]
        public IEnumerator nullInputTest()
        {
            var assemblies = new Assembly[] { typeof(EffectEditorLibrary).Assembly };
            var nodes = MethodActionDefine.loadMethodsFromAssemblies(assemblies).Where(d => d.obsoleteMsg == null).ToArray();
            var player1 = createPlayer(1);
            var player2 = createPlayer(2);
            var eventManager = new GameObject().AddComponent<EventManager>();
            var initTask = createGame(eventManager, new Player[] { player1, player2 });
            while (!initTask.IsCompleted)
                yield return null;
            var game = initTask.Result;
            var logger = game.logger as TestLogger;
            List<ActionDefine> success = new List<ActionDefine>();
            foreach (var nodeDefine in nodes)
            {
                game.addActionDefine(nodeDefine);
                var flow = new Flow(game, player1.getMaster(), null, null, null);
                var node = new ActionNode(1, new ActionReference(nodeDefine.cardPoolId, nodeDefine.defineId));
                node.define = nodeDefine;
                node.Define();

                foreach (var input in node.getInputPorts<ValueInput>())
                {
                    var targetType = input.define.type;
                    node.setInputDefaultValue(input.name, targetType.IsValueType ? Activator.CreateInstance(targetType) : null);
                }
                logger.exception = null;
                var task = Task.Run(async () =>
                {
                    try
                    {
                        var inputPort = node.getEnterPort();
                        if (inputPort != null)
                        {
                            await game.runActions(flow, inputPort);
                        }
                        else
                        {
                            await game.getValue(flow, node.getOutputPort<ValueOutput>(MethodActionDefine.returnValueName));
                        }
                        if (logger.exception != null)
                        {
                            Debug.Log($"节点{nodeDefine.editorName}存在异常：{logger.exception}");
                        }
                        else
                        {
                            Debug.Log($"节点{nodeDefine.editorName}正常。");
                            success.Add(nodeDefine);
                        }
                    }
                    catch (Exception e)
                    {
                        Debug.Log($"节点{nodeDefine.editorName}存在异常：{e}");
                    }
                });
                while (!task.IsCompleted)
                    yield return null;
            }
            Assert.AreEqual(nodes.Length, success.Count);
        }

        private static async Task<CardEngine> createGame(EventManager eventManager, Player[] players)
        {
            //加载外部卡牌
            var patchouli = new CardDefine(ClassicCardID.Patchouli, DEFINE.MASTER) { cardPoolId = 1 };
            var summerFire = new CardDefine(ClassicCardID.SummerFire, DEFINE.SKILL) { cardPoolId = 1 };
            var luckyCoin = new CardDefine(ClassicCardID.LuckyCoin, DEFINE.SPELL) { cardPoolId = 1 };
            patchouli.setSkillRef(summerFire.getDefineRef());
            List<CardDefine> cards = new List<CardDefine>()
            {
                patchouli,
                summerFire,
                luckyCoin
            };
            List<BuffDefine> buffs = new List<BuffDefine>();
            List<ActionDefine> actionDefines = new List<ActionDefine>();
            List<Rule> rules = new List<Rule>()
            {
                new HearthStoneRule()
            };
            var eventDefines = eventManager.getOrLoadBuiltinEventDefines();
            var answers = new GameObject().AddComponent<AnswerManager>();
            var triggerManager = new GameObject().AddComponent<TriggerManager>();
            CardEngine game = new CardEngine()
            {
                answers = answers,
                triggers = triggerManager,
                time = new GameObject(nameof(TimeManager)).AddComponent<TimeManager>(),
                snapshoter = new THHSnapshoter(),
                logger = new TestLogger()
            };
            answers.game = game;
            triggerManager.game = game;
            foreach (var actionDefine in actionDefines)
            {
                game.addActionDefine(actionDefine);
            }

            foreach (var eventDefine in eventDefines)
            {
                game.addEventDefine(eventDefine);
            }
            var luckCoinRef = luckyCoin.getDefineRef();
            for (int i = 0; i < players.Length; i++)
            {
                var player = players[i];
                var deck = new Deck(i, "testDeck", patchouli.getDefineRef(), new DefineReference[] { luckCoinRef, luckCoinRef });
                var startDeck = deck.toDefRefArray();
                var defineArray = startDeck.Select(d => cards.FirstOrDefault(c => c.cardPoolId == d.cardPoolId && c.id == d.defineId)).ToArray();
                player.setStartDeck(game, defineArray);
                players[i] = player;
            }
            await game.init(cards.ToArray(), buffs.ToArray(), rules.ToArray(), new HearthstoneOption(), players);
            return game;
        }
        private static Player createPlayer(int id)
        {
            return new Player(id, id.ToString());
        }
        private class TestLogger : TouhouCardEngine.Shared.ILogger
        {
            public bool enable { get => false; set { } }
            private List<string> _blacklist;
            public List<string> blackList { get => _blacklist; set => _blacklist = value; }
            public Exception exception;

            public void log(string channel, string msg)
            {
            }

            public void logWarn(string channel, string msg)
            {
            }

            public void logError(string channel, string msg)
            {
                exception = new Exception($"[{channel}]{msg}");
                throw exception;
            }

            public void logTrace(string channel, string msg)
            {
            }

            public void log(string msg)
            {
            }

            public void logWarn(string msg)
            {
            }

            public void logError(string msg)
            {
                exception = new Exception(msg);
                throw exception;
            }

            public void logTrace(string msg)
            {
            }
        }
    }
}
