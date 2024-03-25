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
            _soundItem.onClick += i => onSoundItemClick?.Invoke(this);
            _soundItem.onPlayClick += i => onSoundItemPlayClick?.Invoke(this);
        }
        public Action<LabeledSoundItem> onSoundItemClick;
        public Action<LabeledSoundItem> onSoundItemPlayClick;
        public Text text => _text;
        public SoundItem soundItem => _soundItem;
        [SerializeField]
        private Text _text;
        [SerializeField]
        private SoundItem _soundItem;
    }
}