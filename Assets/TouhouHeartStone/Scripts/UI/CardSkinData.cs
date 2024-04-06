using System;
using System.Collections.Generic;
using System.Linq;
using Game;
using MongoDB.Bson.Serialization.Attributes;
using UnityEngine;

namespace UI
{
    public class CardSkinData : ISkin
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
            _language = other.language;
            foreach (var animRef in other._animationRefs)
            {
                _animationRefs.Add(new SkinAnimationRef(animRef));
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
        List<SkinAnimationRef> _animationRefs = new List<SkinAnimationRef>();
        public override string ToString()
        {
            string name = this.name;
            if (!string.IsNullOrEmpty(subname))
                name += $"-{subname}";
            name += $"<{id}>";
            return name;
        }
    }
}
