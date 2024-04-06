using MessagePack;
using System;
using TouhouCardEngine;
using TouhouHeartstone;

namespace Game
{
    [MessagePackObject]
    [Serializable]
    public class SerializableAuraEffect : SerializableEffect
    {
        /// <summary>
        /// 给序列化用的默认构造器
        /// </summary>
        public SerializableAuraEffect() : base()
        {
        }
        public SerializableAuraEffect(GeneratedAura effect) : base(effect)
        {
        }

        public override GeneratedEffect Deserialize(INodeDefiner nodeDefiner)
        {
            var graph = this.graph.toActionGraph(nodeDefiner);
            GeneratedAura effect = new GeneratedAura(graph);
            apply(effect);
            return effect;
        }
    }
}
