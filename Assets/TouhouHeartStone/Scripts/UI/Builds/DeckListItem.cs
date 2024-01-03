using UnityEngine;
using System;
using UnityEngine.UI;
using BJSYGameCore.UI;

namespace UI
{
    public class DeckListItem : UIObject
    {
        #region 公有方法
        public void updateView(string name, Sprite image)
        {
            _nameText.text = name;
            _image.sprite = image;
            _nameInputField.text = name;
        }

        public void SetSelectable(bool val)
        {
            _imageButton.interactable = val;
        }

        public void setWarning(bool val)
        {
            _warningImage.gameObject.SetActive(val);
        }

        public void setCardRentWarning(bool val)
        {
            _cardPoolWarningImage.gameObject.SetActive(val);
        }
        public void setToggleValue(bool val)
        {
            if (_toggle)
                _toggle.SetIsOnWithoutNotify(val);
        }
        public void SetMode(Mode mode)
        {
            _imageObj.SetActive(mode != Mode.AddDeck && mode != Mode.EditCardPool); // 图片
            _createDeckButton.gameObject.SetActive(mode == Mode.AddDeck); // 添加牌组按钮
            _addCardPoolButton.gameObject.SetActive(mode == Mode.EditCardPool); // 编辑卡池按钮

            _nameInputField.gameObject.SetActive(mode == Mode.DeckTitle);
            _deleteButton.gameObject.SetActive(mode == Mode.DeckListItem);
            _toggleObj.SetActive(mode == Mode.CardPoolSelect);
        }
        #endregion

        #region 私有方法

        #region 生命周期
        protected override void Awake()
        {
            base.Awake();
            _imageButton.onClick.AddListener(() => onClick?.Invoke(this));
            _deleteButton.onClick.AddListener(() => onClickDeleteButton?.Invoke(this));
            _nameInputField.onEndEdit.AddListener(value => onNameInputFieldValueChanged?.Invoke(this, value));
            _createDeckButton.onClick.AddListener(() => onCreateDeckClick?.Invoke(this));
            _addCardPoolButton.onClick.AddListener(() => onAddCardPoolClick?.Invoke(this));
            if (_toggle)
                _toggle.onValueChanged.AddListener((value) => onToggleValueChanged?.Invoke(this, value));
        }
        #endregion

        #endregion

        #region 事件
        public event Action<DeckListItem> onClick;
        public event Action<DeckListItem> onClickDeleteButton;
        public event Action<DeckListItem, string> onNameInputFieldValueChanged;
        public event Action<DeckListItem, bool> onToggleValueChanged;
        public event Action<DeckListItem> onAddCardPoolClick;
        public event Action<DeckListItem> onCreateDeckClick;
        #endregion

        #region 属性字段

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
        GameObject _toggleObj;
        [SerializeField]
        Toggle _toggle;

        [Header("大按钮")]
        [SerializeField]
        Button _imageButton;
        [SerializeField]
        Button _addCardPoolButton;
        [SerializeField]
        Button _createDeckButton;

        [Header("提醒图标")]
        [SerializeField]
        private Image _warningImage;
        [SerializeField]
        private Image _cardPoolWarningImage;
        #endregion

        #region 内嵌类
        public enum Mode
        {
            DeckListItem, // 牌组列表
            DeckTitle, // 当前编辑的牌组
            AddDeck, // 添加牌组按钮
            DeckSelect, // 牌组选择（人物编辑）
            EditCardPool, // 编辑卡池按钮
            CardPoolSelect, // 卡池选择
        }
        #endregion
    }
}