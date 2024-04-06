using MessagePack;
using System;
using System.Collections.Generic;
using System.Linq;
using TouhouCardEngine;

namespace Game
{
    [MessagePackObject]
    [Serializable]
    public sealed class SerializableActionNode : SerializableNode
    {
        #region 公有方法
        #region 构造函数
        /// <summary>
        /// 给序列化用的默认构造器
        /// </summary>
        public SerializableActionNode() : base()
        {
        }
        public SerializableActionNode(ActionNode actionNode) : base(actionNode)
        {
            defineRef = actionNode.defineRef;
        }
        #endregion

        public ActionNode ToActionNode(ActionGraph graph)
        {
            var node = new ActionNode(id, defineRef)
            {
                posX = posX,
                posY = posY,
            };
            node.defineName = defineName;
            IEnumerable<InputDefaultValue> defaultValues =
                inputDefaultValues?.Select(dv => dv.deserialize())
                ?? constDict?.Select(c => new InputDefaultValue(c.Key, -1, c.Value));
            InitNode(node, graph, defaultValues);
            return node;
        }
        public override Node ToNode(ActionGraph graph) => ToActionNode(graph);
        #endregion
        #region 属性字段
        [Key(5)]
        public ActionReference defineRef;
        [IgnoreMember]
        [Obsolete]
        public Dictionary<string, object> constDict;
        [IgnoreMember]
        [Obsolete]
        public string defineName;
        [IgnoreMember]
        [Obsolete]
        public int[] branches;
        [IgnoreMember]
        [Obsolete]
        public SerializableActionValueRef[] inputs;
        [IgnoreMember]
        [Obsolete]
        public bool[] regVar;
        [IgnoreMember]
        [Obsolete]
        public object[] consts;
        #endregion
    }
}
