using System;
using UI;
using Controllers;
using TouhouCardEngine;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Game
{
    [Serializable]
    public class CardPool
    {
        public List<CardDefine> cardDefineList = new List<CardDefine>();
        public List<CardSkinData> cardSkinList = new List<CardSkinData>();
        public List<ActionNodeGraphInfo> actionNodeGraphInfoList = new List<ActionNodeGraphInfo>();
        public List<GeneratedBuffDefine> buffDefineList = new List<GeneratedBuffDefine>();
        public List<BuffSkin> buffSkinList = new List<BuffSkin>();
        public List<SerializableActionDefine> actionDefineList = new List<SerializableActionDefine>();
        public List<GeneratedActionDefine> generatedActionDefineList = new List<GeneratedActionDefine>();
    }
    [Serializable]
    public class SerializableCardPool
    {
        #region 公有方法
        #region 构造方法
        public SerializableCardPool(CardPool cardPool)
        {
            if (cardPool == null)
                throw new ArgumentNullException(nameof(cardPool));
            cardDefineList = cardPool.cardDefineList.ConvertAll(c => new SerializableCardDefine(c));
            cardSkinList = cardPool.cardSkinList;
            actionNodeGraphInfoList = cardPool.actionNodeGraphInfoList.ConvertAll(c => new SerializableActionNodeGraphInfo(c));
            buffDefineList = cardPool.buffDefineList.ConvertAll(b => new SerializableBuffDefine(b));
            buffSkinList = cardPool.buffSkinList;
            if (cardPool.actionDefineList.Count > 0)
                actionDefineList = cardPool.actionDefineList;
            else if (cardPool.generatedActionDefineList.Count > 0)
                actionDefineList = cardPool.generatedActionDefineList.ConvertAll(a => new SerializableActionDefine(a));
            else
                actionDefineList = new List<SerializableActionDefine>();
        }
        #endregion
        public CardPool toCardPool(Func<string, Type> typeFinder)
        {
            var cardDefines = cardDefineList.ConvertAll(c => c.toCardDefine(typeFinder));
            var buffDefines = buffDefineList.ConvertAll(b => b.toGeneratedBuffDefine(typeFinder));
            var genActionDefines = actionDefineList.ConvertAll(a => a.toGeneratedActionDefine(typeFinder));
            var nodeGraphs = new List<ActionNodeGraphInfo>();
            foreach (var graph in actionNodeGraphInfoList)
            {
                List<SerializableActionNode> actionNodeList = getActionNodeList(graph, cardDefineList, buffDefineList, actionDefineList);
                var nodeGraph = graph.toActionNodeGraphInfo(actionNodeList);
                nodeGraphs.Add(nodeGraph);
            }
            return new CardPool()
            {
                cardDefineList = cardDefines,
                buffDefineList = buffDefines,
                generatedActionDefineList = genActionDefines,
                cardSkinList = cardSkinList,
                buffSkinList = buffSkinList,
                actionNodeGraphInfoList = nodeGraphs,
            };
        }
        private List<SerializableActionNode> getActionNodeList(SerializableActionNodeGraphInfo graphInfo, 
            List<SerializableCardDefine> cardDefines, 
            List<SerializableBuffDefine> buffDefines, 
            List<SerializableActionDefine> genActionDefines)
        {
            // 卡牌定义。
            var cardDefine = cardDefines.Find(c => c.id == graphInfo.cardId);
            if (cardDefine != null)
            {
                var effects = cardDefine.effectList;
                if (tryGetEffectsActionNodeList(graphInfo, effects, out var nodeList))
                    return nodeList;
            }
            // 增益定义。
            var buffDefine = buffDefines.Find(c => c.id == graphInfo.buffId);
            if (buffDefine != null)
            {
                var effects = buffDefine.effectList;
                if (tryGetEffectsActionNodeList(graphInfo, effects, out var nodeList))
                    return nodeList;
            }
            // 自定义动作。
            var genActionDefine = genActionDefines.Find(c => c.id == graphInfo.actionDefineId);
            if (genActionDefine != null)
            {
                return genActionDefine.actionNodeList;
            }
            return new List<SerializableActionNode>();
        }
        private bool tryGetEffectsActionNodeList(SerializableActionNodeGraphInfo graphInfo, List<SerializableEffect> effects, out List<SerializableActionNode> nodeList)
        {
            var effectIndex = graphInfo.effectIndex;
            if (effectIndex >= 0 && effectIndex < effects.Count)
            {
                SerializableEffect effect = effects[effectIndex];
                if (effect != null)
                {
                    if (graphInfo.entryType == nameof(GeneratedEffect.onEnableAction))
                    {
                        nodeList = effect.onEnableActionList;
                        return true;
                    }
                    else if (graphInfo.entryType == nameof(GeneratedEffect.onDisableAction))
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
        #endregion
        #region 属性字段
        public List<SerializableCardDefine> cardDefineList = new List<SerializableCardDefine>();
        public List<CardSkinData> cardSkinList = new List<CardSkinData>();
        public List<SerializableActionNodeGraphInfo> actionNodeGraphInfoList = new List<SerializableActionNodeGraphInfo>();
        public List<SerializableBuffDefine> buffDefineList = new List<SerializableBuffDefine>();
        public List<BuffSkin> buffSkinList = new List<BuffSkin>();
        public List<SerializableActionDefine> actionDefineList;
        #endregion
    }
}
