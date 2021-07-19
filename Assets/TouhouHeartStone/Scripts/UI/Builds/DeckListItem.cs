using UnityEngine;
using System;

namespace UI
{
    partial class DeckListItem
    {
        public Action<int> onDeckSelect;
        public Action<int> onDeckRemove;
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
            HeadImageButton.onClick.AddListener(() => { onDeckSelect?.Invoke(id); });
            NameInputField.onEndEdit.AddListener((s) => { onDeckRename?.Invoke(id, s); });
            DeleteButtonIconGlass.asButton.onClick.AddListener(() => { onDeckRemove?.Invoke(id); });
            AddButtonBlack.asButton.onClick.AddListener(() => { onDeckAdd?.Invoke(); });

            DeleteButtonIconGlass.asButton.onClick.AddListener(() => onFileRemove?.Invoke(key));
            HeadImageButton.onClick.AddListener(() => onFileSelect?.Invoke(key));
            HeadImageButton.onClick.AddListener(onClickCallback);
            DeleteButtonIconGlass.asButton.onClick.AddListener(onClickDeleteButtonCallback);
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

        public void SetWarning(bool val)
        {
            NotCompleteImage.gameObject.SetActive(val);
        }

        public enum Mode
        {
            DeckListItem, // 牌组列表
            DeckTitle, // 当前编辑的牌组
            AddDeck, // 添加牌组按钮
            DeckSelect // 牌组选择（人物编辑）
        }

        public void SetMode(Mode mode)
        {
            ImageMask.gameObject.SetActive(mode != Mode.AddDeck);
            AddButtonBlack.gameObject.SetActive(mode == Mode.AddDeck);

            DeckNameImage.gameObject.SetActive(mode == Mode.DeckListItem || mode == Mode.DeckSelect);
            NameInputField.gameObject.SetActive(mode == Mode.DeckTitle);
            DeleteButtonIconGlass.gameObject.SetActive(mode == Mode.DeckListItem);
        }
        void onClickCallback()
        {
            onClick?.Invoke(this);
        }
        void onClickDeleteButtonCallback()
        {
            onClickDeleteButton?.Invoke(this);
        }
        public event Action<DeckListItem> onClick;
        public event Action<DeckListItem> onClickDeleteButton;
    }
}