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
