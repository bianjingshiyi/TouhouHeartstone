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
        string _cardName;
        public string cardName
        {
            get { return _cardName; }
            set { _cardName = value; }
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
            return cardName + "<" + id + ">:" + desc;
        }
    }
}
