using MessagePack;
using System;
using TouhouCardEngine;
using TouhouHeartstone;

namespace Game
{
    [MessagePackObject]
    [Serializable]
    public class SerializableEventEffect : SerializableEffect
    {
        /// <summary>
        /// 给序列化用的默认构造器
        /// </summary>
        public SerializableEventEffect() : base()
        {
        }
        public SerializableEventEffect(GeneratedEventEffect effect) : base(effect)
        {
        }
        public override GeneratedEffect Deserialize(INodeDefiner definer)
        {
            var graph = this.graph.toActionGraph(definer);
            GeneratedEventEffect effect = new GeneratedEventEffect(graph);
            apply(effect);
            return effect;
        }
    }
}
