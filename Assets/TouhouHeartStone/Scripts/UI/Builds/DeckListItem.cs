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
            _canEditName = viewData.canEditName;
            _nameText.text = viewData.name;
            _nameInputField.inputField.text = viewData.name;
            _image.sprite = viewData.image;
            _imageButton.interactable = !viewData.banned;
            _warningImage.gameObject.SetActive(viewData.isWarning);
            _errorImage.gameObject.SetActive(viewData.isError);
            _waitingImage.gameObject.SetActive(viewData.isWaiting);
            _cardPoolWarningImage.gameObject.SetActive(viewData.hasPrivatePools);
            _fullButtonText.text = viewData.buttonText;
            if (_toggle)
                _toggle.SetIsOnWithoutNotify(viewData.selected);

            SetMode(viewData.mode);
        }
        public void initLanguages(string[] languageCodes)
        {
            _nameInputField.setLanguages(languageCodes);
        }
        public void setLanguageIndex(int index)
        {
            _nameInputField.setLanguageWithoutNotify(index);
        }
        public void setNameInputFieldVisible(bool visible)
        {
            _nameInputFieldVisible = visible;
            updateNameInputFieldVisible();
            if (visible)
                _nameInputField.inputField.ActivateInputField();
            else
                _nameInputField.inputField.DeactivateInputField();
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
            _nameInputField.onEndEdit += value => onNameInputFieldValueChanged?.Invoke(this, value);
            _nameInputField.onDeselect += () => onNameInputFieldDeselect?.Invoke(this);
            _nameInputField.OnLanguageChanged += value => onNameInputFieldLanguageChanged?.Invoke(this, value);
            _fullButton.onClick.AddListener(() => onClick?.Invoke(this));
            if (_toggle)
                _toggle.onValueChanged.AddListener((value) => onToggleValueChanged?.Invoke(this, value));
        }
        #endregion

        private void updateNameInputFieldVisible()
        {
            _nameInputField.gameObject.SetActive(_nameInputFieldVisible && _canEditName);
            _nameButton.gameObject.SetActive(!_nameInputFieldVisible && _canEditName);
            _nameText.gameObject.SetActive(!_nameInputFieldVisible);
        }

        #endregion

        #region 事件
        public event Action<DeckListItem> onClick;
        public event Action<DeckListItem> onClickDeleteButton;
        public event Action<DeckListItem, string> onNameInputFieldValueChanged;
        public event Action<DeckListItem> onNameInputFieldDeselect;
        public event Action<DeckListItem, int> onNameInputFieldLanguageChanged;
        public event Action<DeckListItem> onClickNameButton;
        public event Action<DeckListItem, bool> onToggleValueChanged;
        #endregion

        #region 属性字段
        private Mode _mode;
        private bool _canEditName;
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
        LocalizedInputField _nameInputField;
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
        [SerializeField]
        private Image _errorImage;
        [SerializeField]
        private Image _waitingImage;
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
        public bool isError;
        public bool hasPrivatePools;
        public bool isWaiting;
        public DeckListItem.Mode mode;
        public bool banned;
        public bool selected;
        public bool canEditName;
        public string buttonText;
    }
}