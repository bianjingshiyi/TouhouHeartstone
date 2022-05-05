using System;
using System.Collections.Generic;
using TouhouCardEngine;

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
}
