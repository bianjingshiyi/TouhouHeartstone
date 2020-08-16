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

        int id;
        public int ID => id;

        partial void onAwake()
        {
            HeadImageButton.onClick.AddListener(() => { onDeckSelect?.Invoke(id); });
            NameInputField.onEndEdit.AddListener((s) => { onDeckRename?.Invoke(id, s); });
            DeleteButtonIconGlass.asButton.onClick.AddListener(() => { onDeckRemove?.Invoke(id); });
            AddButtonBlack.asButton.onClick.AddListener(() => { onDeckAdd?.Invoke(); });
        }

        public void SetContent(int id, string name, Sprite img)
        {
            this.id = id;

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
    }
}