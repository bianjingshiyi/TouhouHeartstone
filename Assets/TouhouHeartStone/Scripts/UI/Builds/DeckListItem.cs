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

            _toggle?.onValueChanged.AddListener((value) => onToggleValueChanged?.Invoke(value));
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
            AddCardPool // 添加卡池按钮
        }

        public void SetMode(Mode mode)
        {
            ImageMask.gameObject.SetActive(mode != Mode.AddDeck && mode != Mode.AddCardPool);
            AddButtonBlack.gameObject.SetActive(mode == Mode.AddDeck);
            _addCardPoolButton.gameObject.SetActive(mode == Mode.AddCardPool);

            DeckNameImage.gameObject.SetActive(mode == Mode.DeckListItem || mode == Mode.DeckSelect);
            NameInputField.gameObject.SetActive(mode == Mode.DeckTitle);
            DeleteButtonIconGlass.gameObject.SetActive(mode == Mode.DeckListItem);
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
        public event Action<bool> onToggleValueChanged;
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