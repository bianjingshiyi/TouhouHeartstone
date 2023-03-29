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
    /// <summary>
    /// 用于兼容老版本的卡池所用的结构类。
    /// </summary>
    [Obsolete]
    public class SerializableFreeNode
    {
        public int _id;
        public string defineName;
        public SerializableFreeNode[] branches;
        public SerializableFreeNodeRef[] inputs;
        public object[] consts;
        public bool[] regVar;
    }
    [Obsolete]
    public class SerializableFreeNodeRef
    {
        public SerializableFreeNode action;
        public int index;
        public int actionNodeId;
        public string eventVarName;
        public int argIndex;
    }

}
