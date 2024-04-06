using MessagePack;
using System;
using TouhouCardEngine;
using TouhouHeartstone;

namespace Game
{
    [MessagePackObject]
    [Serializable]
    public class SerializablePassiveEffect : SerializableEffect
    {
        /// <summary>
        /// 给序列化用的默认构造器
        /// </summary>
        public SerializablePassiveEffect() : base()
        {
        }
        public SerializablePassiveEffect(GeneratedPassiveEffect effect) : base(effect)
        {
        }
        public override GeneratedEffect Deserialize(INodeDefiner definer)
        {
            var graph = this.graph.toActionGraph(definer);
            GeneratedPassiveEffect effect = new GeneratedPassiveEffect(graph);
            apply(effect);
            return effect;
        }
    }
}
