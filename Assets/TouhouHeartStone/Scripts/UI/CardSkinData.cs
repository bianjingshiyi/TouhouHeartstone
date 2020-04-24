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
        public override string ToString()
        {
            return name + "<" + id + ">";
        }
    }
}
