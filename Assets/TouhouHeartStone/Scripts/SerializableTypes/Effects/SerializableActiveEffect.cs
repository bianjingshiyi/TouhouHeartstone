using MessagePack;
using System;
using TouhouCardEngine;
using TouhouHeartstone;

namespace Game
{
    [MessagePackObject]
    [Serializable]
    public class SerializableActiveEffect : SerializableEffect
    {
        /// <summary>
        /// 给序列化用的默认构造器
        /// </summary>
        public SerializableActiveEffect() : base()
        {
        }
        public SerializableActiveEffect(GeneratedActiveEffect effect) : base(effect)
        {
        }
        public override GeneratedEffect Deserialize(INodeDefiner definer)
        {
            var graph = this.graph.toActionGraph(definer);
            GeneratedActiveEffect effect = new GeneratedActiveEffect(graph);
            apply(effect);
            return effect;
        }
    }
}
