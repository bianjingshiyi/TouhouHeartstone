using MessagePack;
using System;
using System.Collections.Generic;
using UI;

namespace Game
{
    [MessagePackObject]
    [Serializable]
    public class SerializableCardSkinData
    {
        /// <summary>
        /// 给序列化用的默认构造器
        /// </summary>
        public SerializableCardSkinData() { }
        public SerializableCardSkinData(CardSkinData skin)
        {
            _id = skin.id;
            _skinId = skin.skinId;
            _illustrator = skin.illustrator;
            _skinName = skin.skinName;
            _name = skin.name;
            _subname = skin.subname;
            _imagePath = skin.imagePath;
            _desc = skin.desc;
            _language = skin.language;
            foreach (var animRef in skin.getAllAnimRefs())
            {
                _animationRefs.Add(new SerializableSkinAnimationRef(animRef));
            }
            foreach (var soundRef in skin.getAllSoundRefs())
            {
                _soundRefs.Add(new SerializableSkinSoundRef(soundRef));
            }
        }
        public CardSkinData toSkin()
        {
            var skin = new CardSkinData()
            {
                id = _id,
                skinId = _skinId,
                illustrator = _illustrator,
                skinName = _skinName,
                name = _name,
                subname = _subname,
                imagePath = _imagePath,
                desc = _desc,
                language = _language
            };
            if (_animationRefs != null && _animationRefs.Count > 0)
            {
                foreach (var animRef in _animationRefs)
                {
                    skin.addAnimRef(animRef.toRef());
                }
            }
            if (_soundRefs != null && _soundRefs.Count > 0)
            {
                foreach (var soundRef in _soundRefs)
                {
                    skin.addSoundRef(soundRef.toRef());
                }
            }
            return skin;
        }
        [Key(0)]
        public int _id;
        [Key(1)]
        public int _skinId;
        [Key(2)]
        public string _illustrator;
        [Key(3)]
        public string _skinName;
        [Key(4)]
        public string _name;
        [Key(5)]
        public string _subname;
        [Key(6)]
        public string _imagePath;
        [Key(7)]
        public string _desc;
        [Key(8)]
        public string _language;
        [Key(9)]
        public List<SerializableSkinAnimationRef> _animationRefs = new List<SerializableSkinAnimationRef>();
        [Key(10)]
        public List<SerializableSkinSoundRef> _soundRefs = new List<SerializableSkinSoundRef>();
    }
}
