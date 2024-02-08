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
        public string language;
        public string name;
        public string desc;
        public string imagePath;
        List<SkinAnimationRef> _animationRefs = new List<SkinAnimationRef>();
        public DefineReference persistentAnimRef;

        public BuffSkin()
        {

        }
        public BuffSkin(BuffSkin other)
        {
            buffId = other.buffId;
            language = other.language;
            name = other.name;
            desc = other.desc;
            imagePath = other.imagePath;
            persistentAnimRef = other.persistentAnimRef;
            foreach (var animRef in other._animationRefs)
            {
                _animationRefs.Add(animRef);
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
    [Serializable]
    public class SerializableBuffSkin
    {
        public SerializableBuffSkin(BuffSkin skin)
        {
            buffId = skin.buffId;
            language = skin.language;
            name = skin.name;
            desc = skin.desc;
            imagePath = skin.imagePath;
            persistentAnimRef = skin.persistentAnimRef;
            foreach (var animRef in skin.getAllAnimRefs())
            {
                _animationRefs.Add(new SerializableSkinAnimationRef(animRef));
            }
        }
        public BuffSkin toSkin()
        {
            var skin = new BuffSkin()
            {
                buffId = buffId,
                language = language,
                name = name,
                desc = desc,
                imagePath = imagePath,
                persistentAnimRef = persistentAnimRef
            };
            if (_animationRefs != null)
            {
                foreach (var animRef in _animationRefs)
                {
                    skin.addAnimRef(animRef.toRef());
                }
            }
            return skin;
        }
        public int buffId;
        public string language;
        public string name;
        public string desc;
        public string imagePath;
        List<SerializableSkinAnimationRef> _animationRefs = new List<SerializableSkinAnimationRef>();
        public DefineReference persistentAnimRef;
    }
}
