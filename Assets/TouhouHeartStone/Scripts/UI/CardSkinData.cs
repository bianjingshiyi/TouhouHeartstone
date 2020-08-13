using System;
using UnityEngine;

namespace UI
{
    [Serializable]
    public class CardSkinData
    {
        [SerializeField]
        int _id;
        public int id
        {
            get { return _id; }
            set { _id = value; }
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
        public Sprite image
        {
            get { return _image; }
            set { _image = value; }
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
        public AudioClip sound_onUse
        {
            get { return _sound_onUse; }
            set { _sound_onUse = value; }
        }
        [SerializeField]
        Projectile _projectile;
        public Projectile projectile
        {
            get { return _projectile; }
            set { _projectile = value; }
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
