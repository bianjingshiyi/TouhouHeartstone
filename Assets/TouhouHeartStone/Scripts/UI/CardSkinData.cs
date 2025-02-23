using System;
using System.Collections.Generic;
using System.Linq;
using Game;
using MongoDB.Bson.Serialization.Attributes;
using TouhouHeartstone;
using UnityEngine;

namespace UI
{
    public class CardSkinData : ISkin, ICloneable
    {
        /// <summary>
        /// 构造一个空的卡牌皮肤
        /// </summary>
        public CardSkinData()
        {
        }
        /// <summary>
        /// 构造一个是另外一个卡牌皮肤的复制的卡牌皮肤
        /// </summary>
        public CardSkinData(CardSkinData other)
        {
            _id = other._id;
            _skinId = other._skinId;
            _illustrator = other._illustrator;
            _name = other._name;
            _subname = other._subname;
            _imagePath = other._imagePath;
            _desc = other._desc;
            foreach (var animRef in other._animationRefs)
            {
                _animationRefs.Add(new SkinAnimationRef(animRef));
            }
            foreach (var soundRef in other._soundRefs)
            {
                _soundRefs.Add(new SkinSoundRef(soundRef));
            }
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
        public SkinSoundRef[] getAllSoundRefs()
        {
            return _soundRefs.ToArray();
        }
        public SkinSoundRef getSoundRef(string name)
        {
            return _soundRefs.FirstOrDefault(r => r.name == name);
        }
        public void addSoundRef(SkinSoundRef soundRef)
        {
            _soundRefs.Add(soundRef);
        }
        public int removeSoundRef(string name)
        {
            return _soundRefs.RemoveAll(r => r.name == name);
        }
        public CardSkinData Clone()
        {
            CardSkinData newSkin = new CardSkinData()
            {
                _id = _id,
                _skinId = _skinId,
                _illustrator = _illustrator,
                _skinName = _skinName,
                _name = _name,
                _subname = _subname,
                _imagePath = _imagePath,
                _desc = _desc,
            };
            if (_animationRefs != null && _animationRefs.Count > 0)
            {
                foreach (SkinAnimationRef animRef in _animationRefs)
                {
                    newSkin._animationRefs.Add(animRef.Clone());
                }
            }
            if (_soundRefs != null && _soundRefs.Count > 0)
            {
                foreach (SkinSoundRef soundRef in _soundRefs)
                {
                    newSkin._soundRefs.Add(soundRef.Clone());
                }
            }
            return newSkin;
        }
        object ICloneable.Clone()
        {
            return Clone();
        }
        public static string[] getSoundNamesByType(string cardType)
        {
            switch (cardType)
            {
                case DEFINE.SERVANT:
                    return servantCardSkinSoundNames;
                case DEFINE.ITEM:
                    return itemCardSkinSoundNames;
                case DEFINE.MASTER:
                    return masterCardSkinSoundNames;
                default:
                    return defaultCardSkinSoundNames;
            }
        }
        [BsonElement]
        [SerializeField]
        int _id;
        /// <summary>
        /// 皮肤所属卡牌的id
        /// </summary>
        [BsonIgnore]
        public int id
        {
            get { return _id; }
            set { _id = value; }
        }
        [BsonElement]
        [SerializeField]
        int _skinId;
        /// <summary>
        /// 皮肤id
        /// </summary>
        [BsonIgnore]
        public int skinId
        {
            get { return _skinId; }
            set { _skinId = value; }
        }
        [BsonElement]
        [SerializeField]
        string _illustrator;
        [BsonIgnore]
        public string illustrator
        {
            get { return _illustrator; }
            set { _illustrator = value; }
        }
        [BsonElement]
        [SerializeField]
        string _skinName;
        [BsonIgnore]
        public string skinName
        {
            get { return _skinName; }
            set { _skinName = value; }
        }
        [BsonElement]
        [SerializeField]
        string _name;
        /// <summary>
        /// 皮肤显示的标题名称
        /// </summary>
        [BsonIgnore]
        public string nameKey
        {
            get { return _name; }
            set { _name = value; }
        }
        [BsonElement]
        [SerializeField]
        string _subname;
        /// <summary>
        /// 皮肤显示的副标题名称
        /// </summary>
        [BsonIgnore]
        public string subnameKey
        {
            get { return _subname; }
            set { _subname = value; }
        }
        [BsonElement]
        [SerializeField]
        string _imagePath;
        [BsonIgnore]
        public string imagePathKey
        {
            get { return _imagePath; }
            set { _imagePath = value; }
        }
        [BsonElement]
        [SerializeField]
        string _desc;
        [BsonIgnore]
        public string descKey
        {
            get { return _desc; }
            set { _desc = value; }
        }
        [BsonElement]
        List<SkinAnimationRef> _animationRefs = new();
        [BsonElement]
        List<SkinSoundRef> _soundRefs = new();
        public override string ToString()
        {
            string name = this.nameKey;
            if (!string.IsNullOrEmpty(subnameKey))
                name += $"-{subnameKey}";
            name += $"<{id}>";
            return name;
        }
        public const string SOUND_NAME_ON_USE = "OnUse";
        public const string SOUND_NAME_ON_EQUIP = "OnEquip";
        public const string SOUND_NAME_ON_ENTER = "OnEnter";
        public const string SOUND_NAME_ON_ATTACK = "OnAttack";
        public const string SOUND_NAME_ON_DEATH = "OnDeath";
        public static string[] defaultCardSkinSoundNames = new string[] { SOUND_NAME_ON_USE };
        public static string[] servantCardSkinSoundNames = new string[] { SOUND_NAME_ON_ENTER, SOUND_NAME_ON_ATTACK, SOUND_NAME_ON_DEATH };
        public static string[] itemCardSkinSoundNames = new string[] { SOUND_NAME_ON_EQUIP, SOUND_NAME_ON_DEATH };
        public static string[] masterCardSkinSoundNames = new string[] { SOUND_NAME_ON_ATTACK, SOUND_NAME_ON_DEATH };
    }
}
