using System;
using System.Collections.Generic;
using System.Linq;
using Game;
using TouhouCardEngine;

namespace UI
{
    public class BuffSkin : ISkin
    {
        public int buffId;
        public string nameKey;
        public string descKey;
        public string imagePathKey;
        List<SkinAnimationRef> _animationRefs = new List<SkinAnimationRef>();
        public PersistentAnimationRef persistentAnimRef = new PersistentAnimationRef();

        public BuffSkin()
        {

        }
        public BuffSkin(BuffSkin other)
        {
            buffId = other.buffId;
            nameKey = other.nameKey;
            descKey = other.descKey;
            imagePathKey = other.imagePathKey;
            persistentAnimRef = other.persistentAnimRef;
            foreach (var animRef in other._animationRefs)
            {
                _animationRefs.Add(new SkinAnimationRef(animRef));
            }
        }
        int ISkin.skinId
        {
            get => buffId;
            set => buffId = value;
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
