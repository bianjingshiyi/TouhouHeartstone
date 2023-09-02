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
        List<SkinAnimationRef> _animationRefs = new List<SkinAnimationRef>();


        public DefineReference getEffectNodeAnimationRef(string key)
        {
            var existed = getAnimRef(key);
            if (existed != null)
            {
                return existed.animRef;
            }
            return null;
        }
        public SkinAnimationRef[] getAllAnimRefs()
        {
            return _animationRefs.ToArray();
        }
        public SkinAnimationRef getAnimRef(string key)
        {
            return _animationRefs.FirstOrDefault(r => r.key == key);
        }
        public void addAnimRef(SkinAnimationRef animRef)
        {
            _animationRefs.Add(animRef);
        }
        public int removeAnimRef(string key)
        {
            return _animationRefs.RemoveAll(r => r.key == key);
        }
    }
}
