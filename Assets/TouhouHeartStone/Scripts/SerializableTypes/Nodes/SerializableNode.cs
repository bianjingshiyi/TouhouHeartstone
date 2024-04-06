using MessagePack;
using System;
using System.Collections.Generic;
using System.Linq;
using TouhouCardEngine;

namespace Game
{
    [Union(0, typeof(SerializableEventRuleEntryNode))]
    [Union(1, typeof(SerializableGameRuleEntryNode))]
    [Union(2, typeof(SerializableActionNode))]
    [Union(3, typeof(SerializableActiveEntryNode))]
    [Union(4, typeof(SerializablePassiveEntryNode))]
    [Union(5, typeof(SerializableEventEntryNode))]
    [Union(6, typeof(SerializableAuraEntryNode))]
    [Union(7, typeof(SerializableGeneratedEntryNode))]
    [Union(8, typeof(SerializableGeneratedReturnNode))]
    [MessagePackObject]
    public abstract class SerializableNode
    {
        /// <summary>
        /// 给序列化用的默认构造器
        /// </summary>
        public SerializableNode()
        {
        }
        public SerializableNode(Node node)
        {
            if (node == null)
                throw new ArgumentNullException(nameof(node));
            id = node.id;
            posX = node.posX;
            posY = node.posY;
            inputDefaultValues = node.inputDefaultValues != null ? node.inputDefaultValues.Select(v => new SerializableInputDefaultValue(v)).ToList() : null;
            propDict = node.propDict != null ? new Dictionary<string, object>(node.propDict) : null;
        }
        public abstract Node ToNode(ActionGraph graph);
        protected void InitNode(Node node, ActionGraph graph, IEnumerable<InputDefaultValue> defaultValues = null)
        {
            node.graph = graph;
            if (propDict != null)
            {
                foreach (var pair in propDict)
                {
                    node.setProp(pair.Key, pair.Value);
                }
            }
            defaultValues = defaultValues ?? inputDefaultValues?.ConvertAll(s => s.deserialize());
            if (defaultValues != null)
            {
                foreach (var defaultValue in defaultValues)
                {
                    node.setInputDefaultValue(defaultValue.name, defaultValue.paramIndex, defaultValue.value);
                }
            }
        }
        [Key(0)]
        public int id;
        [Key(1)]
        public float posX;
        [Key(2)]
        public float posY;
        [Key(3)]
        public List<SerializableInputDefaultValue> inputDefaultValues;
        [Key(4)]
        public Dictionary<string, object> propDict;
    }
}
