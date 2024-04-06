using MessagePack;
using System;
using System.Collections.Generic;

namespace Game
{
    [MessagePackObject]
    [Serializable]
    public sealed class SerializableFunctionNode
    {
        #region 属性字段
        [Key(0)]
        public int actionNodeId;
        [Key(1)]
        public SerializableReturnValueRef[] returns;
        [Key(2)]
        public string functionName;
        [Key(3)]
        public object[] consts;
        [Key(4)]
        public List<SerializableActionNode> actionNodeList;
        #endregion
    }
}
