using MessagePack;
using System;
using System.Collections.Generic;
using TouhouCardEngine;
using UI;

namespace Game
{
    [MessagePackObject]
    [Serializable]
    public class SerializableBuffSkin
    {
        /// <summary>
        /// 给序列化用的默认构造器
        /// </summary>
        public SerializableBuffSkin()
        {
        }
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
        [Key(0)]
        public int buffId;
        [Key(1)]
        public string language;
        [Key(2)]
        public string name;
        [Key(3)]
        public string desc;
        [Key(4)]
        public string imagePath;
        [Key(5)]
        public List<SerializableSkinAnimationRef> _animationRefs = new List<SerializableSkinAnimationRef>();
        [Key(6)]
        public DefineReference persistentAnimRef;
    }
}
