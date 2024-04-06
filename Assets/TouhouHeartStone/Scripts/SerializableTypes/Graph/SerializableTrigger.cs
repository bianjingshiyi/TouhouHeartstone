using System;
using System.Collections.Generic;
using TouhouCardEngine;

namespace Game
{
    [Obsolete]
    [Serializable]
    public class SerializableTrigger
    {
        #region 属性字段
        public string eventName;
        public SerializableActionValueRef condition;
        public List<SerializableTargetChecker> targetCheckerList = new List<SerializableTargetChecker>();
        public int actionId;
        public List<SerializableActionNode> actionList = new List<SerializableActionNode>();
        #endregion
    }
}
