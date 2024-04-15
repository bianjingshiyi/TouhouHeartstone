using System;
using UnityEngine;
using UnityEngine.UI;

namespace UI
{
    public class LabeledSoundItem : MonoBehaviour
    {
        public void updateView(string name, Sprite image)
        {
            _text.text = name;
            _soundItem.updateView(image);
        }
        private void Awake()
        {
            _soundItem.onClick += (i, b) => onSoundItemClick?.Invoke(this, b);
        }
        public Action<LabeledSoundItem, SoundItem.Buttons> onSoundItemClick;
        public Text text => _text;
        public SoundItem soundItem => _soundItem;
        [SerializeField]
        private Text _text;
        [SerializeField]
        private SoundItem _soundItem;
    }
}