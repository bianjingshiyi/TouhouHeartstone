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
            _image.gameObject.SetActive(sprite);
            _image.sprite = sprite;
            _line.gameObject.SetActive(!sprite);
        }
        #endregion

        #region 私有方法

        #region 生命周期
        private void Awake()
        {
            _button.onClick.AddListener(() => onClick?.Invoke(this, Buttons.Sound));
            _playButton.onClick.AddListener(() => onClick?.Invoke(this, Buttons.Play));
            _deleteButton.onClick.AddListener(() => onClick?.Invoke(this, Buttons.Delete));
        }
        #endregion

        #endregion

        #region 事件
        public event Action<SoundItem, Buttons> onClick;
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
        Button _deleteButton;
        [SerializeField]
        Image _image;
        [SerializeField]
        GameObject _line;

        public enum Buttons
        {
            Sound,
            Play,
            Delete
        }
    }
}