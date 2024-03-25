using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace UI
{
    public class SoundItem : MonoBehaviour
    {
        #region 公有方法
        public void updateView(Sprite sprite)
        {
            if (!sprite)
                _image.gameObject.SetActive(false);
            _image.sprite = sprite;
        }
        #endregion

        #region 私有方法

        #region 生命周期
        private void Awake()
        {
            _button.onClick.AddListener(() => onClick?.Invoke(this));
            _playButton.onClick.AddListener(() => onPlayClick?.Invoke(this));
        }
        #endregion

        #endregion

        #region 事件
        public event Action<SoundItem> onClick;
        public event Action<SoundItem> onPlayClick;
        #endregion

        public bool interactable 
        {
            get => _interactable;
            set
            {
                _interactable = value;
                _button.interactable = _interactable;
            }
        }
        private bool _interactable = true;
        [SerializeField]
        Button _button;
        [SerializeField]
        Button _playButton;
        [SerializeField]
        Image _image;
    }
}