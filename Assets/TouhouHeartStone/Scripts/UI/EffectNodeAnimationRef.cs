using System;
using TouhouCardEngine;

namespace UI
{
    [Serializable]
    public class EffectNodeAnimationRef
    {
        /// <summary>
        /// 构造一个空的效果节点动画引用。
        /// </summary>
        public EffectNodeAnimationRef(int effectIndex, int nodeId, DefineReference animRef)
        {
            this.effectIndex = effectIndex;
            this.nodeId = nodeId;
            this.animRef = animRef;
        }
        public int effectIndex;
        public int nodeId;
        public DefineReference animRef;
    }
}
