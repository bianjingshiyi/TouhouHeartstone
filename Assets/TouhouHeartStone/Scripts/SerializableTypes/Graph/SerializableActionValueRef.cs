using MessagePack;
using System;
using System.Collections.Generic;

namespace Game
{
    [MessagePackObject]
    [Obsolete]
    public class SerializableActionValueRef
    {
        #region 属性字段
        [Key(0)]
        public int actionNodeId;
        [Key(1)]
        public int index;
        [Key(2)]
        public string eventVarName;
        [Key(3)]
        public int argIndex;
        [Key(4)]
        public List<SerializableActionNode> actionNodeList;
        #endregion
    }
}
