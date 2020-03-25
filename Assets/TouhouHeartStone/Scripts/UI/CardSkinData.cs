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
        }
        [SerializeField]
        string _cardName;
        public string cardName
        {
            get { return _cardName; }
        }
        [SerializeField]
        Sprite _image;
        public Sprite image
        {
            get { return _image; }
        }
        [SerializeField]
        string _desc;
        public string desc
        {
            get { return _desc; }
        }
    }
}
