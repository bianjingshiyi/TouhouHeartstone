using System;
using System.Collections.Generic;
using TouhouCardEngine;

namespace Game
{
    /// <summary>
    /// 用于兼容老卡池的数据类。
    /// </summary>
    [Obsolete]
    public class SerializableGeneratedEffect
    {
        public string name;
        public string typeName;
        public Dictionary<string, object> propDict;
        public PileNameCollection pileList;
        public int onEnableRootActionNodeId;
        public int onDisableRootActionNodeId;
        public List<SerializableActionNode> onEnableActionList = new List<SerializableActionNode>();
        public List<SerializableActionNode> onDisalbeActionList = new List<SerializableActionNode>();
        public List<SerializableTrigger> triggerList;
        public EffectTagCollection tagList;
    }
}
