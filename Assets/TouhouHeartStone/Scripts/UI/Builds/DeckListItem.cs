using UnityEngine;
using System;
using UnityEngine.UI;

namespace UI
{
    partial class DeckListItem
    {
        public Action<int> onDeckSelect;
        public Action<int, string> onDeckRename;
        public Action onDeckAdd;


        public Action<string> onFileRemove;
        public Action<string> onFileSelect;

        string key;
        int id;
        public int ID => id;
        public string KEY => key;

        partial void onAwake()
        {
            NameInputField.onEndEdit.AddListener((s) => { onDeckRename?.Invoke(id, s); });
            AddButtonBlack.asButton.onClick.AddListener(() => { onDeckAdd?.Invoke(); });
            _addCardPoolButton.onClick.AddListener(() => onCardPoolAdd?.Invoke());

            DeleteButtonIconGlass.asButton.onClick.AddListener(() => onFileRemove?.Invoke(key));
            HeadImageButton.onClick.AddListener(() => onFileSelect?.Invoke(key));

            HeadImageButton.onClick.AddListener(onClickCallback);
            DeleteButtonIconGlass.asButton.onClick.AddListener(onClickDeleteButtonCallback);
            NameInputField.onEndEdit.AddListener(onEndEditNameInputField);

            _toggle?.onValueChanged.AddListener((value) => onToggleValueChanged?.Invoke(this, value));
        }
        public void updateView(string name, Sprite image)
        {
            _nameText.text = name;
            _image.sprite = image;
        }
        public void SetContent(int id, string name, Sprite img)
        {
            this.id = id;

            HeadImageButton.image.sprite = img;
            NameInputField.text = name;
            Text.text = name;
        }

        public void SetContent(string key, string name, Sprite img)
        {
            this.key = key;
            HeadImageButton.image.sprite = img;
            NameInputField.text = name;
            Text.text = name;
        }

        public void SetSelectable(bool val)
        {
            HeadImageButton.interactable = val;
        }

        public void setWarning(bool val)
        {
            NotCompleteImage.gameObject.SetActive(val);
        }

        public void setCardRentWarning (bool val) 
        {
            _cardPoolWarningImage.gameObject.SetActive(val);
        }
        public void SetToggleValue(bool val)
        {
            if (_toggle != null)
                _toggle.isOn = val;
        }
        public enum Mode
        {
            DeckListItem, // 牌组列表
            DeckTitle, // 当前编辑的牌组
            AddDeck, // 添加牌组按钮
            DeckSelect, // 牌组选择（人物编辑）
            EditCardPool, // 编辑卡池按钮
            CardPoolSelect, // 卡池选择
        }

        public void SetMode(Mode mode)
        {
            ImageMask.gameObject.SetActive(true);
            AddButtonBlack.gameObject.SetActive(false);
            _addCardPoolButton.gameObject.SetActive(false);

            DeckNameImage.gameObject.SetActive(false);
            NameInputField.gameObject.SetActive(false);
            DeleteButtonIconGlass.gameObject.SetActive(false);
            _toggleObj.SetActive(false);

            switch (mode)
            {
                case Mode.DeckListItem: // 牌组列表
                    DeckNameImage.gameObject.SetActive(true); // 显示名称
                    DeleteButtonIconGlass.gameObject.SetActive(true); //显示删除按钮
                    break;
                case Mode.DeckTitle: // 当前编辑的牌组
                    NameInputField.gameObject.SetActive(true); // 显示编辑输入栏
                    break;
                case Mode.AddDeck: // 添加牌组按钮
                    ImageMask.gameObject.SetActive(false); // 隐藏图片
                    AddButtonBlack.gameObject.SetActive(true); // 显示添加牌组按钮
                    break;
                case Mode.DeckSelect: // 牌组选择（人物编辑）
                    DeckNameImage.gameObject.SetActive(true); // 显示名称
                    break;
                case Mode.EditCardPool: // 编辑卡池按钮
                    ImageMask.gameObject.SetActive(false); // 隐藏图片
                    _addCardPoolButton.gameObject.SetActive(true); // 显示编辑卡池按钮
                    break;
                case Mode.CardPoolSelect: // 卡池选择
                    DeckNameImage.gameObject.SetActive(true); // 显示名称
                    _toggleObj.SetActive(true); // 显示勾选框
                    break;
            }
            _mode = mode;
        }
        void onClickCallback()
        {
            onDeckSelect?.Invoke(id);
            onClick?.Invoke(this);
        }
        void onClickDeleteButtonCallback()
        {
            onClickDeleteButton?.Invoke(this);
        }
        void onEndEditNameInputField(string value)
        {
            onNameInputFieldValueChanged?.Invoke(this, value);
        }
        public event Action<DeckListItem> onClick;
        public event Action<DeckListItem> onClickDeleteButton;
        public event Action<DeckListItem, string> onNameInputFieldValueChanged;
        public event Action<DeckListItem, bool> onToggleValueChanged;
        public event Action onCardPoolAdd;
        #region 属性字段

        [SerializeField]
        Text _nameText;
        [SerializeField]
        Image _image;
        [SerializeField]
        Image _cardPoolWarningImage;
        [SerializeField]
        Button _addCardPoolButton;
        [SerializeField]
        GameObject _toggleObj;
        [SerializeField]
        Toggle _toggle;

        public Image HeadImage => _image;

        private Mode _mode;
        public Mode mode 
        { 
            get => _mode;
            set => SetMode(value);
        }
        #endregion
    }
}