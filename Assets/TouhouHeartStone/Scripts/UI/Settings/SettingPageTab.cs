using System;
using UnityEngine;
using UnityEngine.UI;

namespace UI
{
    public class SettingPageTab : MonoBehaviour
    {
        public void setName(string name)
        {
            _text.text = name;
        }
        public void setSelected(bool selected)
        {
            _image.sprite = selected ? _selectedSprite : _normalSprite;
        }
        private void Awake()
        {
            _button.onClick.AddListener(() => onClick?.Invoke(this));
        }
        public event Action<SettingPageTab> onClick;
        [SerializeField]
        private Button _button;
        [SerializeField]
        private Text _text;
        [SerializeField]
        private Image _image;
        [SerializeField]
        private Sprite _normalSprite;
        [SerializeField]
        private Sprite _selectedSprite;
    }
}
