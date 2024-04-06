using MessagePack;
using System;
using System.Collections.Generic;
using System.Linq;
using TouhouCardEngine;

namespace Game
{
    [MessagePackObject]
    [Serializable]
    public class SerializableActionDefine
    {
        #region 公有方法
        #region 构造函数
        /// <summary>
        /// 给序列化用的默认构造器
        /// </summary>
        public SerializableActionDefine()
        {
        }
        public SerializableActionDefine(GeneratedActionDefine generatedActionDefine)
        {
            if (generatedActionDefine == null)
                throw new ArgumentNullException(nameof(generatedActionDefine));
            id = generatedActionDefine.defineId;
            type = (int)generatedActionDefine.type;
            category = generatedActionDefine.category;
            name = generatedActionDefine.editorName;
            graph = new SerializableActionNodeGraph(generatedActionDefine.graph);
            // 不包括动作入口端点
            inputs.AddRange(generatedActionDefine.inputDefines.Where(d => d.name != ActionDefine.enterPortName).Select(d => new SerializablePortDefine(d)));
            // 不包括动作出口端点
            outputs.AddRange(generatedActionDefine.outputDefines.Where(d => d.name != ActionDefine.exitPortName).Select(d => new SerializablePortDefine(d)));
        }
        #endregion
        public GeneratedActionDefine toGeneratedActionDefine(TypeFinder typeFinder)
        {
            // 铺设节点。
            var graph = new ActionGraph();
            var nodes = this.graph.GetNodes(graph);
            graph.AddNodes(nodes);

            IEnumerable<SerializablePortDefine> inputPorts = inputs;
            if (consts != null)
            {
                inputPorts = inputPorts.Concat(consts);
                var paramsInput = inputPorts.FirstOrDefault(p => p.isParams);
                if (paramsInput != null)
                {
                    inputPorts = inputPorts.Where(p => p != paramsInput).Append(paramsInput);
                }
            }
            var inputDefines = inputPorts.Select(s => s.ToPortDefine(typeFinder)).ToArray();
            var define = new GeneratedActionDefine(graph, id, (NodeDefineType)type, category, name,
                inputDefines,
                outputs.Select(s => s.ToPortDefine(typeFinder)).ToArray());
            return define;
        }
        #endregion
        #region 属性字段
        [Key(0)]
        public int id;
        [Key(1)]
        public int type;
        [Key(2)]
        public string name;
        [Key(3)]
        public string category;
        [Key(4)]
        public SerializableActionNodeGraph graph;
        [Key(5)]
        public List<SerializablePortDefine> inputs = new List<SerializablePortDefine>();
        [IgnoreMember]
        [Obsolete]
        public List<SerializablePortDefine> consts = new List<SerializablePortDefine>();
        [Key(6)]
        public List<SerializablePortDefine> outputs = new List<SerializablePortDefine>();

        [IgnoreMember]
        [Obsolete]
        public List<SerializableValueDefine> inputList = new List<SerializableValueDefine>();
        [IgnoreMember]
        [Obsolete]
        public List<SerializableValueDefine> constList = new List<SerializableValueDefine>();
        [IgnoreMember]
        [Obsolete]
        public List<SerializableValueDefine> outputList = new List<SerializableValueDefine>();
        [IgnoreMember]
        [Obsolete]
        public List<ReturnValueRef> returnList = null;
        [IgnoreMember]
        [Obsolete]
        public List<SerializableReturnValueRef> seriReturnList = new List<SerializableReturnValueRef>();
        [IgnoreMember]
        [Obsolete]
        public ActionNode action;
        [IgnoreMember]
        [Obsolete]
        public int rootActionId;
        [IgnoreMember]
        [Obsolete]
        public List<SerializableActionNode> actionNodeList = new List<SerializableActionNode>();
        #endregion
    }
}
