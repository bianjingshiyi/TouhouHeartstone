using UnityEngine;
using System;
using UnityEngine.UI;
using BJSYGameCore.UI;

namespace UI
{
    public class DeckListItem : UIObject
    {
        #region 公有方法
        public void updateView(DeckListItemViewData viewData)
        {
            _nameText.text = viewData.name;
            _nameInputField.text = viewData.name;
            _image.sprite = viewData.image;
            _imageButton.interactable = !viewData.banned;
            _warningImage.gameObject.SetActive(viewData.isWarning);
            _cardPoolWarningImage.gameObject.SetActive(viewData.hasPrivatePools);
            _fullButtonText.text = viewData.buttonText;
            if (_toggle)
                _toggle.SetIsOnWithoutNotify(viewData.selected);

            SetMode(viewData.mode);
        }
        public void setNameInputFieldVisible(bool visible)
        {
            _nameInputFieldVisible = visible;
            updateNameInputFieldVisible();
            if (visible)
                _nameInputField.ActivateInputField();
            else
                _nameInputField.DeactivateInputField();
        }
        public void SetMode(Mode mode)
        {
            _mode = mode;
            _imageObj.SetActive(mode != Mode.Button); // 图片
            _fullButton.gameObject.SetActive(mode == Mode.Button); // 黑按钮
            _deleteButton.gameObject.SetActive(mode == Mode.CanDelete);
            _toggleObj.SetActive(mode == Mode.CanSelect);
            updateNameInputFieldVisible();
        }
        #endregion

        #region 私有方法

        #region 生命周期
        protected override void Awake()
        {
            base.Awake();
            _imageButton.onClick.AddListener(() => onClick?.Invoke(this));
            _deleteButton.onClick.AddListener(() => onClickDeleteButton?.Invoke(this));
            _nameButton.onClick.AddListener(() => onClickNameButton?.Invoke(this));
            _nameInputField.onEndEdit.AddListener(value => onNameInputFieldValueChanged?.Invoke(this, value));
            _fullButton.onClick.AddListener(() => onClick?.Invoke(this));
            if (_toggle)
                _toggle.onValueChanged.AddListener((value) => onToggleValueChanged?.Invoke(this, value));
        }
        #endregion

        private void updateNameInputFieldVisible()
        {
            _nameInputField.gameObject.SetActive(_nameInputFieldVisible && _mode == Mode.CanEditName);
            _nameButton.gameObject.SetActive(!_nameInputFieldVisible && _mode == Mode.CanEditName);
            _nameText.gameObject.SetActive(!_nameInputFieldVisible);
        }

        #endregion

        #region 事件
        public event Action<DeckListItem> onClick;
        public event Action<DeckListItem> onClickDeleteButton;
        public event Action<DeckListItem, string> onNameInputFieldValueChanged;
        public event Action<DeckListItem> onClickNameButton;
        public event Action<DeckListItem, bool> onToggleValueChanged;
        #endregion

        #region 属性字段
        private Mode _mode;
        private bool _nameInputFieldVisible;

        [Header("主体")]
        [SerializeField]
        Text _nameText;
        [SerializeField]
        Image _image;
        [SerializeField]
        GameObject _imageObj;

        [Header("UI组件")]
        [SerializeField]
        Button _deleteButton;
        [SerializeField]
        InputField _nameInputField;
        [SerializeField]
        Button _nameButton;
        [SerializeField]
        GameObject _toggleObj;
        [SerializeField]
        Toggle _toggle;

        [Header("大按钮")]
        [SerializeField]
        Button _imageButton;
        [SerializeField]
        Button _fullButton;
        [SerializeField]
        Text _fullButtonText;

        [Header("提醒图标")]
        [SerializeField]
        private Image _warningImage;
        [SerializeField]
        private Image _cardPoolWarningImage;
        #endregion

        #region 内嵌类
        public enum Mode
        {
            /// <summary>
            /// 只显示卡图和名称
            /// </summary>
            Default,
            /// <summary>
            /// 可以被删除
            /// </summary>
            CanDelete,
            /// <summary>
            /// 可以修改名称
            /// </summary>
            CanEditName,
            /// <summary>
            /// 可以选择
            /// </summary>
            CanSelect,
            /// <summary>
            /// 纯按钮
            /// </summary>
            Button,
        }
        #endregion
    }
    public struct DeckListItemViewData
    {
        public string name;
        public Sprite image;
        public bool isWarning;
        public bool hasPrivatePools;
        public DeckListItem.Mode mode;
        public bool banned;
        public bool selected;
        public string buttonText;
    }
}