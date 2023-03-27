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
    public class ActionNodeGraphInfo
    {
        public int cardId;
        public int buffId;
        public int effectIndex;
        public int actionDefineId;
    }
    [Serializable]
    public class SerializableActionNodeGraphInfo
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
        public List<SerializableActionNode> freeNodeList = new List<SerializableActionNode>();
        /// <summary>
        /// 用于兼容老版本的卡池。
        /// </summary>
        [BsonIgnoreIfNull]
        public List<ActionNode> freeActionNodeList;
    }
}
