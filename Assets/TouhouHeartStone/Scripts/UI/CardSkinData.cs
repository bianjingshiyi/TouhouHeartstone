using System;
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
        }
        [SerializeField]
        int _id;
        public int id
        {
            get { return _id; }
            set { _id = value; }
        }
        [SerializeField]
        int _skinId;
        public int skinId
        {
            get { return _skinId; }
            set { _skinId = value; }
        }
        [SerializeField]
        string _illustrator;
        public string illustrator
        {
            get { return _illustrator; }
            set { _illustrator = value; }
        }
        [SerializeField]
        string _name;
        public string name
        {
            get { return _name; }
            set { _name = value; }
        }
        [SerializeField]
        string _subname;
        public string subname
        {
            get { return _subname; }
            set { _subname = value; }
        }
        [SerializeField]
        Sprite _image;
        [Obsolete("请使用imagePath并通过资源加载获取。")]
        public Sprite image
        {
            get { return _image; }
            set { _image = value; }
        }
        [SerializeField]
        string _imagePath;
        public string imagePath
        {
            get { return _imagePath; }
            set { _imagePath = value; }
        }
        [SerializeField]
        string _desc;
        public string desc
        {
            get { return _desc; }
            set { _desc = value; }
        }
        [SerializeField]
        AudioClip _sound_onUse;
        [Obsolete("请使用soundPath_onUse并使用资源加载器加载对应音效替代")]
        public AudioClip sound_onUse
        {
            get { return _sound_onUse; }
            set { _sound_onUse = value; }
        }
        [SerializeField]
        string _soundPath_onUse;
        /// <summary>
        /// 使用音效地址
        /// </summary>
        public string soundPath_onUse
        {
            get { return _soundPath_onUse; }
            set { _soundPath_onUse = value; }
        }
        [SerializeField]
        Projectile _projectile;
        [Obsolete("请使用projectilePath并使用资源加载器加载对应投射物替代")]
        public Projectile projectile
        {
            get { return _projectile; }
            set { _projectile = value; }
        }
        [SerializeField]
        string _projectilePath;
        public string projectilePath
        {
            get { return _projectilePath; }
            set { _projectilePath = value; }
        }
        [SerializeField]
        string _language;
        public string language
        {
            get { return _language; }
            set { _language = value; }
        }
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
