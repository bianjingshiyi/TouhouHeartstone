using MessagePack;
using System;
using System.Collections.Generic;
using TouhouCardEngine;

namespace Game
{
    [Union(0, typeof(SerializablePassiveEffect))]
    [Union(1, typeof(SerializableActiveEffect))]
    [Union(2, typeof(SerializableEventEffect))]
    [Union(3, typeof(SerializableAuraEffect))]
    [MessagePackObject]
    [Serializable]
    public abstract class SerializableEffect
    {
        /// <summary>
        /// 给序列化用的默认构造器
        /// </summary>
        public SerializableEffect()
        {
        }
        public SerializableEffect(GeneratedEffect effect)
        {
            name = effect.name;
            propDict = effect.propDict;
            graph = new SerializableActionNodeGraph(effect.graph);
        }
        public abstract GeneratedEffect Deserialize(INodeDefiner definer);
        protected void apply(GeneratedEffect effect)
        {
            effect.name = name;
            effect.propDict = propDict;
        }
        [Key(0)]
        public string name;
        [Key(1)]
        public Dictionary<string, object> propDict;
        [Key(2)]
        public SerializableActionNodeGraph graph;
    }
}
