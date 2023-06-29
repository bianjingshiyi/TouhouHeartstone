using System;
using System.Collections.Generic;
using System.Linq;
using TouhouCardEngine;

namespace UI
{
    [Serializable]
    public class BuffSkin
    {
        public int buffId;
        public string language;
        public string name;
        public string desc;
        public string imagePath;
        List<EffectNodeAnimationRef> _effectNodeAnimationRefs = new List<EffectNodeAnimationRef>();


        public void setEffectNodeAnimationRef(int effectIndex, int nodeId, DefineReference animationRef)
        {
            if (animationRef == null)
            {
                _effectNodeAnimationRefs.RemoveAll(r => r.effectIndex == effectIndex && r.nodeId == nodeId);
                return;
            }
            var existed = getAnimRef(effectIndex, nodeId);
            if (existed != null)
            {
                existed.animRef = animationRef;
                return;
            }
            _effectNodeAnimationRefs.Add(new EffectNodeAnimationRef(effectIndex, nodeId, animationRef));
        }
        public DefineReference getEffectNodeAnimationRef(int effectIndex, int nodeId)
        {
            var existed = getAnimRef(effectIndex, nodeId);
            if (existed != null)
            {
                return existed.animRef;
            }
            return null;
        }
        private EffectNodeAnimationRef getAnimRef(int effectIndex, int nodeId)
        {
            return _effectNodeAnimationRefs.FirstOrDefault(r => r.effectIndex == effectIndex && r.nodeId == nodeId);
        }
    }
}
