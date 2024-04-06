using Controllers;
using MongoDB.Bson.Serialization.Attributes;
using System;
using System.Collections.Generic;

namespace Game
{
    [Serializable]
    [Obsolete]
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
        public List<SerializableFreeNode> freeActionNodeList;
    }
}
