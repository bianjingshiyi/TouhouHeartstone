using System;
using System.Collections.Generic;
using System.Linq;
using MongoDB.Bson.Serialization.Attributes;
using TouhouCardEngine;
using UnityEngine;

namespace Controllers
{
    /// <summary>
    /// 动作节点图的编辑器信息，保存了每个效果的所有动作节点信息。
    /// </summary>
    [Serializable]
    public class ActionNodeGraphInfo
    {
        public int cardId;
        public int buffId;
        public int effectIndex;
        public string entryType;
        public int triggerIndex;
        public int actionDefineId;
        public Dictionary<string, ActionNodeGraphNodeInfo> actionInfoDict = new Dictionary<string, ActionNodeGraphNodeInfo>();
        /// <summary>
        /// 离散的，并未与根节点相连的动作节点。
        /// </summary>
        public List<ActionNode> freeActionNodeList = new List<ActionNode>();
    }
    [Serializable]
    public class SerializableActionNodeGraphInfo
    {
        public SerializableActionNodeGraphInfo(ActionNodeGraphInfo info)
        {
            cardId = info.cardId;
            buffId = info.buffId;
            effectIndex = info.effectIndex;
            entryType = info.entryType;
            triggerIndex = info.triggerIndex;
            actionDefineId = info.actionDefineId;
            actionInfoDict = info.actionInfoDict;
            freeNodeList = info.freeActionNodeList.ConvertAll(n => new SerializableActionNode(n));
        }
        public ActionNodeGraphInfo toActionNodeGraphInfo(List<SerializableActionNode> actionNodeList, Dictionary<int, ActionNode> actionNodeDict = null)
        {
            if (actionNodeDict == null)
                actionNodeDict = new Dictionary<int, ActionNode>();


            List<ActionNode> freeList = new List<ActionNode>();
            if (freeNodeList != null)
            {
                var totalNodes = new List<SerializableActionNode>(freeNodeList.Union(actionNodeList));
                foreach (var seri in freeNodeList)
                {
                    if (actionNodeDict.ContainsKey(seri.id))
                        continue;
                    try
                    {
                        var graph = SerializableActionNode.toActionNodeGraph(seri.id, totalNodes, actionNodeDict);
                        freeList.Add(graph);
                    }
                    catch (KeyNotFoundException e)
                    {
                        Debug.LogWarning("反序列化ActionNodeGraphInfo时遇到异常：" + e);
                    }
                }
            }
            else if (freeActionNodeList != null)
            {
                // 用于兼容老版本卡池。
                freeList = freeActionNodeList;
                freeActionNodeList = null;
            }

            return new ActionNodeGraphInfo()
            {
                cardId = cardId,
                buffId = buffId,
                effectIndex = effectIndex,
                entryType = entryType,
                triggerIndex = triggerIndex,
                actionDefineId = actionDefineId,
                actionInfoDict = actionInfoDict,
                freeActionNodeList = freeList
            };
        }
        public int cardId;
        public int buffId;
        public int effectIndex;
        public string entryType;
        public int triggerIndex;
        public int actionDefineId;
        public Dictionary<string, ActionNodeGraphNodeInfo> actionInfoDict = new Dictionary<string, ActionNodeGraphNodeInfo>();
        /// <summary>
        /// 离散的，并未与根节点相连的动作节点。
        /// </summary>
        public List<SerializableActionNode> freeNodeList = new List<SerializableActionNode>();
        /// <summary>
        /// 用于兼容老版本的卡池。
        /// </summary>
        [BsonIgnoreIfNull]
        public List<ActionNode> freeActionNodeList;
    }
}
