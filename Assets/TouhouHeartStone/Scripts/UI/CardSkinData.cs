using MongoDB.Bson.Serialization.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using TouhouCardEngine;
using UnityEngine;

namespace UI
{
    [Serializable]
    public class CardSkinData
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
            _soundPath_onUse = other._soundPath_onUse;
            _projectilePath = other._projectilePath;
            _language = other.language;
            foreach (var animRef in other._effectNodeAnimationRefs)
            {
                _effectNodeAnimationRefs.Add(animRef);
            }
        }
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
        public string name
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
        public string subname
        {
            get { return _subname; }
            set { _subname = value; }
        }
        [BsonIgnore]
        [SerializeField]
        Sprite _image;
        [Obsolete("请使用imagePath并通过资源加载获取。")]
        [BsonIgnore]
        public Sprite image
        {
            get { return _image; }
            set { _image = value; }
        }
        [BsonElement]
        [SerializeField]
        string _imagePath;
        [BsonIgnore]
        public string imagePath
        {
            get { return _imagePath; }
            set { _imagePath = value; }
        }
        [BsonElement]
        [SerializeField]
        string _desc;
        [BsonIgnore]
        public string desc
        {
            get { return _desc; }
            set { _desc = value; }
        }
        [BsonIgnore]
        [SerializeField]
        AudioClip _sound_onUse;
        [Obsolete("请使用soundPath_onUse并使用资源加载器加载对应音效替代")]
        [BsonIgnore]
        public AudioClip sound_onUse
        {
            get { return _sound_onUse; }
            set { _sound_onUse = value; }
        }
        [BsonElement]
        [SerializeField]
        string _soundPath_onUse;
        /// <summary>
        /// 使用音效地址
        /// </summary>
        [BsonIgnore]
        public string soundPath_onUse
        {
            get { return _soundPath_onUse; }
            set { _soundPath_onUse = value; }
        }
        [BsonElement]
        [SerializeField]
        string _projectilePath;
        [BsonIgnore]
        public string projectilePath
        {
            get { return _projectilePath; }
            set { _projectilePath = value; }
        }
        [BsonElement]
        [SerializeField]
        string _language;
        [BsonIgnore]
        public string language
        {
            get { return _language; }
            set { _language = value; }
        }
        [BsonElement]
        List<EffectNodeAnimationRef> _effectNodeAnimationRefs = new List<EffectNodeAnimationRef>();
        public override string ToString()
        {
            string name = this.name;
            if (!string.IsNullOrEmpty(subname))
                name += "-" + subname;
            name += "<" + id + ">";
            return name;
        }
    }
}
