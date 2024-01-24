using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace UI
{
    public class CharacterSelectItem : MonoBehaviour
    {
        public void update(CharacterViewData viewData)
        {
            _text.text = viewData.name;
            _image.sprite = viewData.image;
        }
        public RectTransform getButtonTransform()
        {
            return _button.transform as RectTransform;
        }
        private void Awake()
        {
            _button.onClick.AddListener(onClickCallback);
        }
        private void onClickCallback()
        {
            onClick?.Invoke(this);
        }
        public event Action<CharacterSelectItem> onClick;
        [SerializeField]
        private TextMeshProUGUI _text;
        [SerializeField]
        private Button _button;
        [SerializeField]
        private Image _image;
    }
}