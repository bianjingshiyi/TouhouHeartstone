using System;
using System.Collections.Generic;
using System.Linq;
using Controllers;
using TouhouCardEngine;
using TouhouCardEngine.Interfaces;
using TouhouHeartstone;
using UnityEngine;

namespace Game
{
    public class CardPoolConvertor
    {
        private SerializableCardPool _cardPool;
        private long poolId;
        private SerializableActionNodeGraphInfo[] _graphInfos;
        private TypeFinder typeFinder;
        private ActionDefineFinder defineFinder;
        private EventTypeInfoFinder eventInfoFinder;
        private List<GeneratedActionDefine> convertedActionDefines;
        private INodeDefiner definer;
        public CardPoolConvertor(long poolId, SerializableCardPool pool, ActionDefineFinder defineFinder, EventTypeInfoFinder eventFinder, TypeFinder typeFinder)
        {
            _cardPool = pool;
            this.poolId = poolId;
            _graphInfos = _cardPool.actionNodeGraphInfoList.ToArray();
            this.typeFinder = typeFinder;
            this.defineFinder = defineFinder;
            eventInfoFinder = eventFinder;
            definer = new THHNodeDefiner(getActionDefine, eventInfoFinder);
            convertedActionDefines = new List<GeneratedActionDefine>();
        }
        /// <summary>
        /// 将目标SerializableCardPool转换为新版CardPool。
        /// </summary>
        /// <returns></returns>
        public CardPool Convert()
        {
            var genActionDefines = ConvertActionDefines(_cardPool.actionDefineList);
            var cardDefines = _cardPool.cardDefineList.ConvertAll(c => ConvertCardDefine(c));
            var buffDefines = _cardPool.buffDefineList.ConvertAll(b => ConvertBuffDefine(b));

            var pool = new CardPool()
            {
                cardDefineList = cardDefines,
                buffDefineList = buffDefines,
                actionDefineList = new List<GeneratedActionDefine>(genActionDefines),
                cardSkinList = _cardPool.cardSkinList,
                buffSkinList = _cardPool.buffSkinList,
            };
            return pool;
        }
        private ActionDefine getActionDefine(string name)
        {
            if (string.IsNullOrEmpty(name))
                return null;
            return defineFinder?.Invoke(name) ?? convertedActionDefines.FirstOrDefault(d => d.defineName == name || (d.obsoleteNames != null && d.obsoleteNames.Contains(name)));
        }
        #region 转换定义
        /// <summary>
        /// 将目标SerializableCardDefine转换为新版CardDefine。
        /// </summary>
        /// <returns></returns>
        private CardDefine ConvertCardDefine(SerializableCardDefine seri)
        {
            var props = new Dictionary<string, object>();
            foreach (var propPair in seri.propDict)
            {
                props.Add(propPair.Key, propPair.Value);
            }
            var effectList = seri.effectList;
            List<GeneratedEffect> effects = new List<GeneratedEffect>();
            for (int i = 0; i < effectList.Count; i++)
            {
                var effect = effectList[i];
                ConvertEffect(effects, effect, seri, null, i);
            }
            CardDefine v2 = new CardDefine(seri.id, seri.type, props, effects.ToArray())
            {
                cardPoolId = poolId
            };
            return v2;
        }

        /// <summary>
        /// 将目标SerializableBuffDefine转换为新版GeneratedBuffDefine。
        /// </summary>
        /// <returns></returns>
        public GeneratedBuffDefine ConvertBuffDefine(SerializableBuffDefine seri)
        {
            var seriEffects = seri.effectList;
            List<GeneratedEffect> effects = new List<GeneratedEffect>();
            for (int i = 0; i < seriEffects.Count; i++)
            {
                var effect = seriEffects[i];
                ConvertEffect(effects, effect, null, seri, i);
            }
            GeneratedBuffDefine buff = new GeneratedBuffDefine(seri.id, seri.propModifierList.ToArray(), effects)
            {
                cardPoolId = poolId
            };
            return buff;
        }

        /// <summary>
        /// 将目标SerializableActionDefine转换为新版GeneratedActionDefine。
        /// </summary>
        /// <returns></returns>
        public GeneratedActionDefine[] ConvertActionDefines(IEnumerable<SerializableActionDefine> defines)
        {
            Dictionary<SerializableActionDefine, GeneratedActionDefine> dict = new Dictionary<SerializableActionDefine, GeneratedActionDefine>();
            foreach (var seri in defines)
            {
                PortDefine[] inputs = null;
                PortDefine[] consts = null;
                PortDefine[] outputs = null;
                if (seri.inputList != null)
                {
                    inputs = new PortDefine[seri.inputList.Count];
                    for (int i = 0; i < inputs.Length; i++)
                    {
                        var input = seri.inputList[i];
                        inputs[i] = PortDefine.Value(typeFinder(input.typeName), input.name, input.name);
                    }
                }
                if (seri.constList != null)
                {
                    consts = new PortDefine[seri.constList.Count];
                    for (int i = 0; i < consts.Length; i++)
                    {
                        var input = seri.constList[i];
                        consts[i] = PortDefine.Const(typeFinder(input.typeName), input.name, input.name);
                    }
                }
                if (seri.outputList != null)
                {
                    outputs = new PortDefine[seri.outputList.Count];
                    for (int i = 0; i < outputs.Length; i++)
                    {
                        var input = seri.outputList[i];
                        outputs[i] = PortDefine.Value(typeFinder(input.typeName), input.name, input.name);
                    }
                }

                var graph = new ActionGraph();
                GeneratedActionDefine action = new GeneratedActionDefine(graph, seri.id, seri.category, seri.name, inputs, consts, outputs);

                dict.Add(seri, action);
            }

            convertedActionDefines.AddRange(dict.Values);

            foreach (var pair in dict)
            {
                var seri = pair.Key;
                var action = pair.Value;
                var graph = action.graph;

                action.InitNodes();
                graph.DefineGeneratedActionDefine(action);

                var entryNode = action.getEntryNode();
                var returnNode = action.getReturnNode();

                ActionNodeGraphParams param = new ActionNodeGraphParams()
                {
                    graph = action.graph,
                    graphInfo = getGraphInfo(action.id),
                    actionNodeList = seri.actionNodeList,
                    actionNodeDict = new Dictionary<int, ActionNode>()
                };
                var rootNode = ConvertActionNodeChain(param, seri.rootActionId);
                if (rootNode != null)
                {
                    graph.connect(entryNode.getExitPort(), rootNode.getEnterPort());
                }

                foreach (var returnRef in seri.seriReturnList)
                {
                    if (returnRef == null || returnRef.valueRef == null)
                        continue;
                    var valueRef = ConvertActionValueRef(returnRef.valueRef, param);
                    ConnectValueRef(returnNode.inputPorts.ElementAt(returnRef.returnIndex), valueRef, param);
                }

            }
            return dict.Values.ToArray();
        }
        #endregion

        #region 效果
        /// <summary>
        /// 将一个SerializableGeneratedEffect根据其类型转换为特定的GeneratedEffect。
        /// </summary>
        /// <param name="effect"></param>
        /// <param name="card"></param>
        /// <param name="buff"></param>
        /// <param name="effectIndex"></param>
        /// <returns></returns>
        /// <exception cref="FormatException"></exception>
        /// <exception cref="InvalidCastException"></exception>
        private void ConvertEffect(List<GeneratedEffect> list, SerializableGeneratedEffect effect, SerializableCardDefine card, SerializableBuffDefine buff, int effectIndex)
        {
            var typeName = effect.typeName;

            if (typeName == "GeneratedBattlecry")
            {
                ConvertGeneratedBattlecry(list, effect, card, buff, effectIndex);
            }
            else if (typeName == "GeneratedDeathRattle")
            {
                ConvertGeneratedDeathRattle(list, effect, card, buff, effectIndex);
            }
            else if (typeName == "GeneratedAura")
            {
                ConvertGeneratedAura(list, effect, card, buff, effectIndex);
            }
            else if (typeName == "GeneratedEffect")
            {
                ConvertCommonEffect(list, effect, card, buff, effectIndex);
            }
            else
                throw new InvalidCastException($"无法将\"{typeName}\"类型的效果反序列化");
        }
        /// <summary>
        /// 将一个宣言效果转换为主动效果。
        /// </summary>
        /// <param name="effect"></param>
        /// <param name="card"></param>
        /// <param name="buff"></param>
        /// <param name="effectIndex"></param>
        /// <returns></returns>
        /// <exception cref="FormatException"></exception>
        private void ConvertGeneratedBattlecry(List<GeneratedEffect> list, SerializableGeneratedEffect effect, SerializableCardDefine card, SerializableBuffDefine buff, int effectIndex)
        {
            try
            {
                int cardId = card?.id ?? 0;
                int buffId = buff?.id ?? 0;

                // 创建节点图。
                var graph = new ActionGraph();
                GeneratedActiveEffect active = new GeneratedActiveEffect(graph);
                active.name = effect.name;
                active.Init();
                graph.DefineNodes(definer);

                // 宣言触发器。
                var seriTrigger = effect.triggerList?[0];
                if (seriTrigger != null)
                {
                    // 获取通用效果的触发器的节点图信息。
                    var graphInfo = getGraphInfo(cardId, buffId, effectIndex, "activeTrigger");

                    // 将触发器的序列化文档转换为根节点。
                    var param = new ActionNodeGraphParams(graph: graph, graphInfo: graphInfo, eventName: seriTrigger.eventName, actionNodeList: seriTrigger.actionList);
                    ConvertTriggerToActiveEntryNode(seriTrigger, param);

                    // 添加离散节点。
                    CreateFreeNodesToGraph(param);
                }
                // 添加属性
                foreach (var pair in effect.propDict)
                {
                    active.propDict.Add(pair.Key, pair.Value);
                }

                active.setTags(EFFECTTAG.ACTIVE_PLAYED);
                list.Add(active);
            }
            catch (Exception e)
            {
                throw new FormatException("反序列化宣言效果触发器失败", e);
            }
        }
        /// <summary>
        /// 将一个遗言效果转换为被动效果。
        /// </summary>
        /// <param name="effect"></param>
        /// <param name="card"></param>
        /// <param name="buff"></param>
        /// <param name="effectIndex"></param>
        /// <returns></returns>
        /// <exception cref="FormatException"></exception>
        private void ConvertGeneratedDeathRattle(List<GeneratedEffect> list, SerializableGeneratedEffect effect, SerializableCardDefine card, SerializableBuffDefine buff, int effectIndex)
        {
            try
            {
                int cardId = card?.id ?? 0;
                int buffId = buff?.id ?? 0;

                // 创建节点图。
                var graph = new ActionGraph();
                GeneratedPassiveEffect deathrattle = new GeneratedPassiveEffect(graph);
                deathrattle.name = effect.name;
                deathrattle.Init();
                graph.DefineNodes(definer);

                // 遗言触发器。
                var seriTrigger = effect.triggerList?[0];
                if (seriTrigger != null)
                {
                    // 获取通用效果的触发器的节点图信息。
                    var graphInfo = getGraphInfo(cardId, buffId, effectIndex, "activeTrigger");

                    // 将触发器的序列化文档转换为根节点。
                    var param = new ActionNodeGraphParams(graph: graph, graphInfo: graphInfo, eventName: seriTrigger.eventName, actionNodeList: seriTrigger.actionList);
                    var entryNode = ConvertTriggerToPassiveEntryNode(seriTrigger, param);

                    // 添加离散节点。
                    CreateFreeNodesToGraph(param);
                }

                // 添加属性
                foreach (var pair in effect.propDict)
                {
                    deathrattle.propDict.Add(pair.Key, pair.Value);
                }
                deathrattle.setTags(EFFECTTAG.PASSIVE_LASTWORD);
                list.Add(deathrattle);
            }
            catch (Exception e)
            {
                throw new FormatException("反序列化遗言效果触发器失败", e);
            }
        }
        /// <summary>
        /// 将一个SerializableGeneratedEffect转换为光环效果。
        /// </summary>
        /// <param name="effect"></param>
        /// <param name="card"></param>
        /// <param name="buff"></param>
        /// <param name="effectIndex"></param>
        /// <returns></returns>
        /// <exception cref="FormatException"></exception>
        private void ConvertGeneratedAura(List<GeneratedEffect> list, SerializableGeneratedEffect effect, SerializableCardDefine card, SerializableBuffDefine buff, int effectIndex)
        {
            try
            {
                int cardId = card?.id ?? 0;
                int buffId = buff?.id ?? 0;

                // 创建节点。
                var graph = new ActionGraph();
                GeneratedAura aura = new GeneratedAura(graph);
                aura.name = effect.name;
                aura.Init();
                graph.DefineNodes(definer);

                // 创建光环数据节点。
                var dataNode = aura.getEntryNode();

                // 光环的生效区域。
                var pileList = effect.pileList;
                dataNode.setConst(AuraEntryNode.pileListName, pileList);

                // 光环的作用区域。
                if (effect.propDict.TryGetValue(AuraEntryNode.rangeListName, out var rangeListObj) && rangeListObj is PileNameCollection rangeList)
                {
                    dataNode.setConst(AuraEntryNode.rangeListName, rangeList);
                }

                // 光环的增益定义。
                if (effect.propDict.TryGetValue("getAuraBuffDefine", out object buffDefineObj) && buffDefineObj is SerializableActionValueRef buffDefineRef)
                {
                    // 获取光环增益定义的节点图信息。
                    var graphInfo = getGraphInfo(cardId, buffId, effectIndex, "getAuraBuffDefine");

                    // 将对增益定义的节点引用转换为根节点。
                    var param = new ActionNodeGraphParams(new Vector2(-300, 200), graph: graph, graphInfo: graphInfo, actionNodeList: buffDefineRef.actionNodeList);
                    var rootNode = ConvertActionValueRef(buffDefineRef, param);

                    // 连接光环数据节点的“buffDefine”输入和根节点。
                    ConnectValueRef(aura.getAuraBuffDefine, rootNode, param);

                    // 添加离散节点。
                    CreateFreeNodesToGraph(param);
                }

                // 光环的施加条件。
                if (effect.propDict.TryGetValue("affectCondition", out object conditionObj) && conditionObj is SerializableFunctionNode condFunc)
                {
                    // 获取光环作用条件的节点图信息。
                    var graphInfo = getGraphInfo(cardId, buffId, effectIndex, "affectCondition");

                    // 转换作用条件的节点图。
                    var param = new ActionNodeGraphParams(new Vector2(-1200, -300), graph: graph, graphInfo: graphInfo, actionNodeList: condFunc.actionNodeList);
                    if (condFunc.returns.Length > 0)
                    {
                        var rootNode = ConvertActionValueRef(condFunc.returns[0].valueRef, param);

                        // 连接光环数据节点的“affectCondition”输入和根节点。
                        ConnectValueRef(aura.affectCondition, rootNode, param);
                    }
                    // 添加离散节点。
                    CreateFreeNodesToGraph(param);
                }

                // 光环的更新动作。
                if (effect.propDict.TryGetValue("updateAction", out object updateObj) && updateObj is SerializableFunctionNode updateFunc)
                {
                    // 获取光环更新动作的节点图信息。
                    var graphInfo = getGraphInfo(cardId, buffId, effectIndex, "updateAction");

                    // 转换更新动作的节点图。
                    var param = new ActionNodeGraphParams(new Vector2(300, 0),graph: graph, graphInfo: graphInfo, actionNodeList: updateFunc.actionNodeList);
                    if (updateFunc.actionNodeId != 0)
                    {
                        var rootNode = ConvertActionNodeChain(param, updateFunc.actionNodeId);

                        // 连接光环数据节点的“buffDefine”输入和根节点。
                        graph.connect(aura.updateAction, rootNode.getEnterPort());
                    }

                    // 添加离散节点。
                    CreateFreeNodesToGraph(param);
                }
                // 添加属性
                foreach (var pair in effect.propDict)
                {
                    aura.propDict.Add(pair.Key, pair.Value);
                }
                list.Add(aura);
            }
            catch (Exception e)
            {
                throw new FormatException("反序列化光环效果触发器失败", e);
            }
        }
        /// <summary>
        /// 将一个通用效果转换。
        /// </summary>
        /// <param name="effect"></param>
        /// <param name="card"></param>
        /// <param name="buff"></param>
        /// <param name="effectIndex"></param>
        /// <returns></returns>
        /// <exception cref="FormatException"></exception>
        private void ConvertCommonEffect(List<GeneratedEffect> list, SerializableGeneratedEffect effect, SerializableCardDefine card, SerializableBuffDefine buff, int effectIndex)
        {
            try
            {
                for (int i = 0; i < effect.triggerList.Count; i++)
                {
                    var trigger = effect.triggerList[i];
                    if (trigger == null)
                        continue;
                    GeneratedEffect e;
                    if (trigger.eventName == "ActiveEvent" && !effect.tagList.Contains(KEYWORD.DEATHRATTLE))
                    {
                        e = ConvertCommonToActive(effect, card, buff, effectIndex, i);
                    }
                    else
                    {
                        e = ConvertCommonToPassive(effect, card, buff, effectIndex, i);
                    }
                    // 添加属性
                    foreach (var pair in effect.propDict)
                    {
                        e.propDict.Add(pair.Key, pair.Value);
                    }
                    list.Add(e);
                }
            }
            catch (Exception e)
            {
                throw new FormatException("反序列化效果的生效效果失败", e);
            }
        }

        private GeneratedActiveEffect ConvertCommonToActive(SerializableGeneratedEffect effect, SerializableCardDefine card, SerializableBuffDefine buff, int effectIndex, int triggerIndex)
        {
            int cardId = card?.id ?? 0;
            int buffId = buff?.id ?? 0;

            var trigger = effect.triggerList[triggerIndex];

            var graph = new ActionGraph();
            GeneratedActiveEffect active = new GeneratedActiveEffect(graph);
            active.name = effect.name;
            active.Init();
            graph.DefineNodes(definer);

            // 生效事件。
            if (effect.onEnableRootActionNodeId != 0)
            {
                // 获取通用效果的生效事件节点图信息。
                var graphInfo = getGraphInfo(cardId, buffId, effectIndex, "onEnableAction");

                // 将对生效事件的控制引用转换为根节点。
                var param = new ActionNodeGraphParams(new Vector2(-1000, 500), graph: graph, graphInfo: graphInfo, actionNodeList: effect.onEnableActionList);
                var actionNode = ConvertActionNodeChain(param, effect.onEnableRootActionNodeId);

                // 连接光环生效事件节点的控制输出和根节点的控制输入。
                graph.connect(active.onEnableAction, actionNode.getEnterPort());

                // 添加离散节点。
                CreateFreeNodesToGraph(param);
            }

            // 失效事件。
            if (effect.onDisableRootActionNodeId != 0)
            {
                // 获取通用效果的失效事件节点图信息。
                var graphInfo = getGraphInfo(cardId, buffId, effectIndex, "onDisableAction");

                // 将对失效事件的控制引用转换为根节点。
                var param = new ActionNodeGraphParams(new Vector2(-1000, -500), graph: graph, graphInfo: graphInfo, actionNodeList: effect.onDisalbeActionList);
                var actionNode = ConvertActionNodeChain(param, effect.onDisableRootActionNodeId);

                // 连接光环失效事件节点的控制输出和根节点的控制输入。
                graph.connect(active.onDisableAction, actionNode.getEnterPort());

                // 添加离散节点。
                CreateFreeNodesToGraph(param);
            }

            // 触发器。
            if (trigger != null)
            {
                // 获取通用效果的触发器的节点图信息。
                var graphInfo = getGraphInfo(cardId, buffId, effectIndex, triggerIndex);

                // 将触发器的序列化文档转换为根节点。
                var param = new ActionNodeGraphParams(graph: graph, graphInfo: graphInfo, eventName: trigger.eventName, actionNodeList: trigger.actionList);
                ConvertTriggerToActiveEntryNode(trigger, param);

                // 添加离散节点。
                CreateFreeNodesToGraph(param);
            }
            if (effect.tagList.Contains(KEYWORD.BATTLECRY))
            {
                active.setTags(EFFECTTAG.ACTIVE_PLAYED);
            }
            return active;
        }

        private GeneratedPassiveEffect ConvertCommonToPassive(SerializableGeneratedEffect effect, SerializableCardDefine card, SerializableBuffDefine buff, int effectIndex, int triggerIndex)
        {
            int cardId = card?.id ?? 0;
            int buffId = buff?.id ?? 0;

            var trigger = effect.triggerList[triggerIndex];

            var graph = new ActionGraph();

            // 创建效果。
            GeneratedPassiveEffect passive;
            if (effect.tagList.Contains(KEYWORD.DEATHRATTLE) || trigger.eventName == "ActiveEvent")
            {
                // 遗言/发动事件
                passive = new GeneratedPassiveEffect(graph);
            }
            else
            {
                // 事件效果
                passive = new GeneratedEventEffect(graph);
            }
            passive.Init();
            graph.DefineNodes(definer);
            passive.name = effect.name;

            // 生效事件。
            if (effect.onEnableRootActionNodeId != 0)
            {
                // 获取通用效果的生效事件节点图信息。
                var graphInfo = getGraphInfo(cardId, buffId, effectIndex, "onEnableAction");

                // 将对生效事件的控制引用转换为根节点。
                var param = new ActionNodeGraphParams(new Vector2(1000, 500), graph: graph, graphInfo: graphInfo, actionNodeList: effect.onEnableActionList);
                var actionNode = ConvertActionNodeChain(param, effect.onEnableRootActionNodeId);

                // 连接光环生效事件节点的控制输出和根节点的控制输入。
                graph.connect(passive.onEnableAction, actionNode.getEnterPort());

                // 添加离散节点。
                CreateFreeNodesToGraph(param);
            }

            // 失效事件。
            if (effect.onDisableRootActionNodeId != 0)
            {
                // 获取通用效果的失效事件节点图信息。
                var graphInfo = getGraphInfo(cardId, buffId, effectIndex, "onDisableAction");

                // 将对失效事件的控制引用转换为根节点。
                var param = new ActionNodeGraphParams(new Vector2(-1000, -500), graph: graph, graphInfo: graphInfo, actionNodeList: effect.onDisalbeActionList);
                var actionNode = ConvertActionNodeChain(param, effect.onDisableRootActionNodeId);

                // 连接光环失效事件节点的控制输出和根节点的控制输入。
                graph.connect(passive.onDisableAction, actionNode.getEnterPort());

                // 添加离散节点。
                CreateFreeNodesToGraph(param);
            }

            // 触发器。
            if (trigger != null)
            {
                // 获取通用效果的触发器的节点图信息。
                var graphInfo = getGraphInfo(cardId, buffId, effectIndex, triggerIndex);

                // 将触发器的序列化文档转换为根节点。
                var param = new ActionNodeGraphParams(graph: graph, graphInfo: graphInfo, eventName: trigger.eventName, actionNodeList: trigger.actionList);
                ConvertTriggerToPassiveEntryNode(trigger, param);

                // 添加离散节点。
                CreateFreeNodesToGraph(param);
            }

            if (passive is GeneratedEventEffect eventEffect)
            {
                if (eventEffect.eventEntry != null)
                {
                    eventEffect.eventEntry.setConst(EventEntryNode.pileListName, effect.pileList);
                }
            }
            if (effect.tagList.Contains(KEYWORD.DEATHRATTLE))
            {
                // 遗言
                passive.setTags(EFFECTTAG.PASSIVE_LASTWORD);
            }
            return passive;
        }
        #endregion

        #region 节点
        /// <summary>
        /// 将一个SerializableAction转换为动作节点并加入节点图中，并对与其相连的节点也进行此操作，形成一串节点链。
        /// </summary>
        /// <param name="param"></param>
        /// <param name="rootActionId"></param>
        /// <returns></returns>
        /// <exception cref="KeyNotFoundException"></exception>
        private ActionNode ConvertActionNodeChain(ActionNodeGraphParams param, int rootActionId)
        {
            // 参数修复。
            if (rootActionId == 0)
                return null;
            if (param.actionNodeList == null)
                return null;
            if (param.actionNodeDict == null)
                param.actionNodeDict = new Dictionary<int, ActionNode>();

            // 参数。
            var graph = param.graph;
            var actionNodeList = param.actionNodeList;
            var actionNodeDict = param.actionNodeDict;
            var info = param.graphInfo;
            Vector2 posOffset = param.posOffset;
            if (actionNodeDict.TryGetValue(rootActionId, out var action))
            {
                return action;
            }

            // 寻找ID匹配的序列化动作节点。
            SerializableActionNode seriActionNode = actionNodeList.Find(s => s.id == rootActionId);
            if (seriActionNode == null)
                throw new KeyNotFoundException("不存在id为" + rootActionId + "的动作节点");

            // 创建节点。
            float posX = 0, posY = 0;
            if (info != null && info.actionInfoDict.TryGetValue(rootActionId.ToString(), out var nodeInfo))
            {
                posX = nodeInfo.posX;
                posY = nodeInfo.posY;
            }


            var define = getActionDefine(seriActionNode.defineName);
            ActionNode actionNode = graph.createActionNode(define, posX + posOffset.x, posY + posOffset.y);
            actionNodeDict.Add(rootActionId, actionNode);

            if (seriActionNode.consts != null)
            {
                for (int i = 0; i < seriActionNode.consts.Length; i++)
                {
                    var value = seriActionNode.consts[i];
                    var constDefine = define.constDefines.ElementAt(i);
                    actionNode.setConst(constDefine.name, value);
                }
            }

            // 创建并连接与该节点相连的的其他节点（输入）。
            var inputs = seriActionNode.inputs;
            if (inputs != null)
            {
                for (int i = 0; i < inputs.Length; i++)
                {
                    if (inputs[i] == null)
                        continue;
                    var valueRef = ConvertActionValueRef(inputs[i], param);
                    IPort input = getActionNodeValueInput(actionNode, i);
                    ConnectValueRef(input, valueRef, param);
                }
            }

            // 创建并连接与该节点相连的的其他节点（后续动作）。
            var branches = seriActionNode.branches;
            if (branches != null)
            {
                for (int i = 0; i < branches.Length; i++)
                {
                    var actionId = branches[i];
                    if (actionId == 0)
                        continue;
                    if (!actionNodeDict.TryGetValue(actionId, out var targetNode))
                    {
                        targetNode = ConvertActionNodeChain(param, actionId);
                    }
                    var input = targetNode?.getEnterPort();
                    var output = getActionNodeControlOutput(actionNode, i);
                    if (input == null || output == null)
                        continue;
                    graph.connect(output, input);
                }
            }
            return actionNode;
        }
        /// <summary>
        /// 将触发器入口节点转换为一个主动效果入口节点。
        /// </summary>
        /// <param name="seriTrigger"></param>
        /// <param name="param"></param>
        /// <returns></returns>
        /// <exception cref="FormatException"></exception>
        private ActiveEntryNode ConvertTriggerToActiveEntryNode(SerializableTrigger seriTrigger, ActionNodeGraphParams param)
        {
            // 参数。
            var graph = param.graph;
            var posOffset = param.posOffset;

            // 创建节点。
            var trigger = graph.findActiveEntryNode();
            if (trigger == null)
            {
                float posX = 0, posY = 0;
                posX += posOffset.x;
                posY += posOffset.y;
                trigger = graph.createActiveEntryNode(posX, posY);
            }

            // 触发条件。
            if (seriTrigger.condition != null)
            {
                try
                {
                    var valueRef = ConvertActionValueRef(seriTrigger.condition, param);
                    ConnectValueRef(trigger.getTriggerCondtionValuePort(), valueRef, param);
                }
                catch (Exception e)
                {
                    throw new FormatException("反序列化触发器条件失败", e);
                }
            }

            // 目标条件。
            for (int i = 0; i < seriTrigger.targetCheckerList.Count; i++)
            {
                var seriChecker = seriTrigger.targetCheckerList[i];
                if (seriChecker == null)
                    continue;
                try
                {
                    var checker = new TargetChecker(seriChecker.targetType, seriChecker.errorTip);
                    trigger.AddTargetChecker(checker);
                    // 创建连接到触发器触发条件的各节点。
                    if (seriChecker.condition != null)
                    {
                        var valueRef = ConvertActionValueRef(seriChecker.condition, param);
                        ConnectValueRef(trigger.getTargetConditionPort(i), valueRef, param);
                    }
                }
                catch (Exception e)
                {
                    throw new FormatException("反序列化触发器目标条件" + i + "失败", e);
                }
            }

            // 根动作节点。
            if (seriTrigger.actionId != 0)
            {
                try
                {
                    var node = ConvertActionNodeChain(param, seriTrigger.actionId);
                    var input = node.getEnterPort();
                    var output = trigger.getActionOutputPort();
                    graph.connect(output, input);
                }
                catch (Exception e)
                {
                    throw new FormatException("反序列化触发器动作失败", e);
                }
            }

            return trigger;
        }

        /// <summary>
        /// 将触发器入口节点转换为一个事件入口节点。
        /// </summary>
        /// <param name="seriTrigger"></param>
        /// <param name="param"></param>
        /// <returns></returns>
        /// <exception cref="FormatException"></exception>
        private PassiveEntryNode ConvertTriggerToPassiveEntryNode(SerializableTrigger seriTrigger, ActionNodeGraphParams param)
        {
            // 参数。
            var graph = param.graph;
            var posOffset = param.posOffset;

            // 创建节点。
            var passiveEntry = graph.findPassiveEntryNode();
            if (passiveEntry == null)
            {
                float posX = 0, posY = 0;
                posX += posOffset.x;
                posY += posOffset.y;
                if (seriTrigger.eventName != null && seriTrigger.eventName != "ActiveEvent")
                {
                    var eventTypeInfo = eventInfoFinder?.Invoke(seriTrigger.eventName);
                    passiveEntry = graph.createEventEntryNode(eventTypeInfo, posX, posY);
                }
                else
                {
                    passiveEntry = graph.createPassiveEntryNode(posX, posY);
                    passiveEntry.Define();
                }
            }

            // 触发条件。
            if (seriTrigger.condition != null)
            {
                try
                {
                    var valueRef = ConvertActionValueRef(seriTrigger.condition, param);
                    ConnectValueRef(passiveEntry.getTriggerCondtionValuePort(), valueRef, param);
                }
                catch (Exception e)
                {
                    throw new FormatException("反序列化触发器条件失败", e);
                }
            }

            // 根动作节点。
            if (seriTrigger.actionId != 0)
            {
                try
                {
                    var node = ConvertActionNodeChain(param, seriTrigger.actionId);
                    var input = node.getEnterPort();
                    var output = passiveEntry.getActionOutputPort();
                    graph.connect(output, input);
                }
                catch (Exception e)
                {
                    throw new FormatException("反序列化触发器动作失败", e);
                }
            }

            return passiveEntry;
        }
        /// <summary>
        /// 创建节点图内的所有离散节点。
        /// </summary>
        /// <param name="graph"></param>
        /// <param name="graphInfo"></param>
        private void CreateFreeNodesToGraph(ActionNodeGraphParams param)
        {
            if (param.graphInfo == null)
                return;
            if (param.graphInfo.freeNodeList == null && param.graphInfo.freeActionNodeList != null)
            {
                foreach (var freeNode in param.graphInfo.freeActionNodeList)
                {
                    CreateFreeNodeToGraphOld(freeNode, param.graphInfo, param);
                }
            }
            else
            {
                foreach (var freeNode in param.graphInfo.freeNodeList)
                {
                    CreateFreeNodeToGraph(freeNode.id, param.graphInfo, param);
                }
            }
        }
        /// <summary>
        /// 创建一个节点图内的离散节点。
        /// </summary>
        /// <param name="nodeId"></param>
        /// <param name="graphInfo"></param>
        /// <param name="param"></param>
        /// <returns></returns>
        private ActionNode CreateFreeNodeToGraph(int nodeId, SerializableActionNodeGraphInfo graphInfo, ActionNodeGraphParams param)
        {
            // 参数。
            var graph = param.graph;
            var nodes = param.actionNodeDict;
            var infoDict = graphInfo.actionInfoDict;

            // 如果该节点已被加入，中断。
            if (nodes.TryGetValue(nodeId, out ActionNode node))
            {
                return node;
            }

            // 创建离散节点。
            float posX = 0, posY = 0;
            var nodeInfo = infoDict.FirstOrDefault(p => p.Key == nodeId.ToString()).Value;
            if (nodeInfo != null)
            {
                posX = nodeInfo.posX;
                posY = nodeInfo.posY;
            }

            var seriNode = graphInfo.freeNodeList.FirstOrDefault(p => p.id == nodeId);
            if (seriNode == null)
                return null;

            var freeNode = graph.createActionNode(seriNode.defineName, posX + param.posOffset.x, posY + param.posOffset.y);
            nodes.Add(nodeId, freeNode);

            // 创建并连接输入的节点。
            for (int i = 0; i < seriNode.inputs.Length; i++)
            {
                var seriInput = seriNode.inputs[i];
                if (seriInput == null)
                    continue;
                var targetNodeId = seriInput.actionNodeId;
                if (targetNodeId == 0)
                    continue;
                // 如果该节点未被加入，创建一个。
                if (!nodes.TryGetValue(targetNodeId, out ActionNode targetNode))
                {
                    targetNode = CreateFreeNodeToGraph(targetNodeId, graphInfo, param);
                }

                // 连接节点。
                var output = getActionNodeValueOutput(targetNode, seriInput.index);
                var input = getActionNodeValueInput(freeNode, i);
                if (output == null || input == null)
                    continue;
                graph.connect(output, input);
            }


            // 创建并连接后续动作节点。
            for (int i = 0; i < seriNode.branches.Length; i++)
            {
                var targetNodeId = seriNode.branches[i];
                if (targetNodeId == 0)
                    continue;
                // 如果该节点未被加入，创建一个。
                if (!nodes.TryGetValue(targetNodeId, out ActionNode targetNode))
                {
                    targetNode = CreateFreeNodeToGraph(targetNodeId, graphInfo, param);
                }

                // 连接节点。
                var output = getActionNodeControlOutput(freeNode, i);
                var input = targetNode.getEnterPort();
                if (output == null || input == null)
                    continue;
                graph.connect(output, input);
            }

            return freeNode;
        }

        /// <summary>
        /// 创建一个节点图内的离散节点。（使用老的freeActionNodeList）
        /// </summary>
        /// <param name="nodeId"></param>
        /// <param name="graphInfo"></param>
        /// <param name="param"></param>
        /// <returns></returns>
        private ActionNode CreateFreeNodeToGraphOld(SerializableFreeNode seriNode, SerializableActionNodeGraphInfo graphInfo, ActionNodeGraphParams param)
        {
            // 参数。
            var graph = param.graph;
            var nodes = param.actionNodeDict;
            var infoDict = graphInfo.actionInfoDict;

            var nodeId = seriNode._id;
            // 如果该节点已被加入，中断。
            if (nodes.TryGetValue(nodeId, out ActionNode node))
            {
                return node;
            }

            // 创建离散节点。
            float posX = 0, posY = 0;
            var nodeInfo = infoDict.FirstOrDefault(p => p.Key == nodeId.ToString()).Value;
            if (nodeInfo != null)
            {
                posX = nodeInfo.posX;
                posY = nodeInfo.posY;
            }

            var freeNode = graph.createActionNode(seriNode.defineName, posX + param.posOffset.x, posY + param.posOffset.y);
            nodes.Add(nodeId, freeNode);

            // 创建并连接输入的节点。
            for (int i = 0; i < seriNode.inputs.Length; i++)
            {
                var input = seriNode.inputs[i];
                if (input.action == null)
                    continue;
                // 如果该节点未被加入，创建一个。
                if (!nodes.TryGetValue(input.action._id, out ActionNode targetNode))
                {
                    targetNode = CreateFreeNodeToGraphOld(input.action, graphInfo, param);
                }

                // 连接节点。
                var outputPort = getActionNodeValueOutput(targetNode, input.index);
                var inputPort = getActionNodeValueInput(freeNode, i);
                if (outputPort == null || inputPort == null)
                    continue;
                graph.connect(outputPort, inputPort);
            }


            // 创建并连接后续动作节点。
            for (int i = 0; i < seriNode.branches.Length; i++)
            {
                var seriBranch = seriNode.branches[i];
                if (seriBranch == null)
                    continue;
                // 如果该节点未被加入，创建一个。
                if (!nodes.TryGetValue(seriBranch._id, out ActionNode targetNode))
                {
                    targetNode = CreateFreeNodeToGraphOld(seriBranch, graphInfo, param);
                }

                // 连接节点。
                var output = getActionNodeControlOutput(freeNode, i);
                var input = targetNode.getEnterPort();
                if (output == null || input == null)
                    continue;
                graph.connect(output, input);
            }

            return freeNode;
        }
        #endregion

        #region 引用/端点
        private ValueInput getActionNodeValueInput(ActionNode node, int index)
        {
            if (node == null)
                return null;
            var define = getActionDefine(node.defineName);
            if (define == null)
                return null;
            if (node.getEnterPort() != null)
            {
                index++;
            }
            var input = node.inputPorts.ElementAtOrDefault(index);
            var lastInputDefine = define.inputDefines.LastOrDefault();
            if (input == null && lastInputDefine != null && lastInputDefine.isParams && index >= define.inputDefines.Count())
            {
                do
                {
                    input = node.extendParamsPort(define.inputDefines.Last().name);
                }
                while (index >= node.inputPorts.Count());
            }
            if (input is ValueInput valueInput)
            {
                return valueInput;
            }
            return null;
        }
        private ValueOutput getActionNodeValueOutput(ActionNode node, int index)
        {
            if (node == null)
                return null;
            var input = node.getOutputPorts<ValueOutput>().ElementAtOrDefault(index);
            if (input is ValueOutput valueInput)
            {
                return valueInput;
            }
            return null;
        }
        private ControlOutput getActionNodeControlOutput(ActionNode node, int index)
        {
            if (node == null)
                return null;
            var output = node.getOutputPorts<ControlOutput>().ElementAtOrDefault(index);
            if (output is ControlOutput controlOutput)
            {
                return controlOutput;
            }
            return null;
        }
        /// <summary>
        /// 获取一个ActionValueRef所引用的节点输出端点。
        /// </summary>
        /// <param name="valueRef"></param>
        /// <param name="param"></param>
        /// <returns></returns>
        private IPort GetValueRefOutputPort(ActionValueRef valueRef, ActionNodeGraphParams param)
        {
            var graph = param.graph;
            IPort port = null;
            if (valueRef.action != null)
            {
                // 普通的动作节点。
                port = getActionNodeValueOutput(valueRef.action, valueRef.index);
            }
            else if (valueRef.actionNodeId != 0 && param.actionNodeDict.TryGetValue(valueRef.actionNodeId, out var targetNode))
            {
                // 普通的动作节点。
                port = getActionNodeValueOutput(targetNode, valueRef.index);
            }
            else if (!string.IsNullOrEmpty(valueRef.eventVarName))
            {
                // 触发器事件入口节点。
                Node node = graph?.findActiveEntryNode();
                if (node == null)
                    node = graph?.findEventEntryNode(param.eventName);
                if (node == null)
                    node = graph?.findPassiveEntryNode();

                if (node != null)
                {
                    var varName = valueRef.eventVarName;
                    port = node.getOutputPort<ValueOutput>(varName);
                }
            }
            else
            {
                // FunctionNode入口节点，或者GeneratedActionDefine的入口节点。
                port = graph?.nodes?.Select(n =>
                {
                    if (n is AuraEntryNode auraEntryNode)
                        return auraEntryNode.getOutputPortAt(valueRef.argIndex + 1);
                    if (n is GeneratedActionEntryNode entryNode)
                        return entryNode.getOutputPortAt(valueRef.argIndex + 1);
                    return null;
                })?.Where(p => p != null)?.FirstOrDefault();
            }
            return port;
        }
        /// <summary>
        /// 连接一个节点的值输入节点，以及一个ActionValueRef所引用的输出端点。
        /// </summary>
        /// <param name="input"></param>
        /// <param name="valueRef"></param>
        /// <param name="param"></param>
        private void ConnectValueRef(IPort input, ActionValueRef valueRef, ActionNodeGraphParams param)
        {
            var output = GetValueRefOutputPort(valueRef, param);
            if (input == null || output == null)
                return;
            param.graph.connect(output, input);
        }
        /// <summary>
        /// 将SerializableActionValueRef转换为ActionValueRef，并创建所引用路径上的所有节点。
        /// </summary>
        /// <param name="seri"></param>
        /// <param name="param"></param>
        /// <returns></returns>
        private ActionValueRef ConvertActionValueRef(SerializableActionValueRef seri, ActionNodeGraphParams param)
        {
            var actionNodeDict = param.actionNodeDict;

            // 有引用其他节点，创建这些节点。
            if (seri.actionNodeId != 0)
            {
                if (actionNodeDict.TryGetValue(seri.actionNodeId, out ActionNode actionNode))
                    return new ActionValueRef(actionNode, seri.index);
                else
                    return new ActionValueRef(ConvertActionNodeChain(param, seri.actionNodeId), seri.index);
            }
            // 连接到触发器。
            else if (!string.IsNullOrEmpty(seri.eventVarName))
                return new ActionValueRef(seri.eventVarName);
            // 连接到FunctionNode或者自定义动作的输出值。
            else
                return new ActionValueRef(seri.argIndex);
        }
        #endregion

        #region 获取节点图信息
        private SerializableActionNodeGraphInfo getGraphInfo(int cardId, int buffId, int effectIndex, int triggerIndex)
        {
            if (buffId != 0)
                return _graphInfos.FirstOrDefault(i => i.buffId == buffId && i.effectIndex == effectIndex && i.entryType == "triggerList" && i.triggerIndex == triggerIndex);
            else
                return _graphInfos.FirstOrDefault(i => i.cardId == cardId && i.effectIndex == effectIndex && i.entryType == "triggerList" && i.triggerIndex == triggerIndex);
        }
        private SerializableActionNodeGraphInfo getGraphInfo(int cardId, int buffId, int effectIndex, string entryType)
        {
            if (buffId != 0)
                return _graphInfos.FirstOrDefault(i => i.buffId == buffId && i.effectIndex == effectIndex && i.entryType == entryType);
            else
                return _graphInfos.FirstOrDefault(i => i.cardId == cardId && i.effectIndex == effectIndex && i.entryType == entryType);
        }
        private SerializableActionNodeGraphInfo getGraphInfo(int actionId)
        {
            if (actionId == 0)
                return null;
            return _graphInfos.FirstOrDefault(i => i.actionDefineId == actionId);
        }
        #endregion

        /// <summary>
        /// 指定一个SerializableActionNodeGraphInfo，从卡池中寻找该GraphInfo属于的节点列表。
        /// </summary>
        /// <param name="graphInfo"></param>
        /// <returns></returns>
        private List<SerializableActionNode> getActionNodeList(SerializableActionNodeGraphInfo graphInfo)
        {
            // 卡牌定义。
            var cardDefine = _cardPool.cardDefineList.Find(c => c.id == graphInfo.cardId);
            if (cardDefine != null)
            {
                var effects = cardDefine.effectList;
                if (tryGetEffectsActionNodeList(graphInfo, effects, out var nodeList))
                    return nodeList;
            }
            // 增益定义。
            var buffDefine = _cardPool.buffDefineList.Find(c => c.id == graphInfo.buffId);
            if (buffDefine != null)
            {
                var effects = buffDefine.effectList;
                if (tryGetEffectsActionNodeList(graphInfo, effects, out var nodeList))
                    return nodeList;
            }
            // 自定义动作。
            var genActionDefine = _cardPool.actionDefineList.Find(c => c.id == graphInfo.actionDefineId);
            if (genActionDefine != null)
            {
                return genActionDefine.actionNodeList;
            }
            return new List<SerializableActionNode>();
        }
        private bool tryGetEffectsActionNodeList(SerializableActionNodeGraphInfo graphInfo, List<SerializableGeneratedEffect> effects, out List<SerializableActionNode> nodeList)
        {
            var effectIndex = graphInfo.effectIndex;
            if (effectIndex >= 0 && effectIndex < effects.Count)
            {
                SerializableGeneratedEffect effect = effects[effectIndex];
                if (effect != null)
                {
                    if (graphInfo.entryType == "onEnableAction")
                    {
                        nodeList = effect.onEnableActionList;
                        return true;
                    }
                    else if (graphInfo.entryType == "onDisableAction")
                    {
                        nodeList = effect.onDisalbeActionList;
                        return true;
                    }
                    else
                    {
                        var triggerIndex = graphInfo.triggerIndex;
                        var triggerList = effect.triggerList;
                        if (triggerList != null && triggerIndex >= 0 && triggerIndex < triggerList.Count)
                        {
                            var trigger = effect.triggerList[graphInfo.triggerIndex];
                            if (trigger != null)
                            {
                                nodeList = trigger.actionList;
                                return true;
                            }
                        }
                    }
                }
            }
            nodeList = null;
            return false;
        }


        /// <summary>
        /// 一个用于替代冗长方法参数的结构。
        /// </summary>
        private struct ActionNodeGraphParams
        {
            public ActionGraph graph;
            public string eventName;
            public Vector2 posOffset;
            public List<SerializableActionNode> actionNodeList;
            public SerializableActionNodeGraphInfo graphInfo;
            public Dictionary<int, ActionNode> actionNodeDict;

            public ActionNodeGraphParams(Vector2 posOffset,
                ActionGraph graph = null,
                List<SerializableActionNode> actionNodeList = null,
                SerializableActionNodeGraphInfo graphInfo = null,
                Dictionary<int, ActionNode> actionNodeDict = null,
                string eventName = null)
            {
                this.graph = graph;
                this.eventName = eventName;
                this.posOffset = posOffset;
                this.actionNodeList = actionNodeList;
                this.graphInfo = graphInfo;
                this.graph = graph;
                if (actionNodeDict == null)
                    actionNodeDict = new Dictionary<int, ActionNode>();
                this.actionNodeDict = actionNodeDict;
            }
            public ActionNodeGraphParams(ActionGraph graph = null,
                List<SerializableActionNode> actionNodeList = null,
                SerializableActionNodeGraphInfo graphInfo = null,
                Dictionary<int, ActionNode> actionNodeDict = null,
                string eventName = null) : this(Vector2.zero, graph, actionNodeList, graphInfo, actionNodeDict, eventName)
            {
            }
        }
    }
}
