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

        partial void onAwake()
        {
            HeadImageButton.onClick.AddListener(() => { onDeckSelect?.Invoke(id); });
            NameInputField.onEndEdit.AddListener((s) => { onDeckRename?.Invoke(id, s); });
            DeleteButtonBlack.asButton.onClick.AddListener(() => { onDeckRemove?.Invoke(id); });
            AddButtonBlack.asButton.onClick.AddListener(() => { onDeckAdd?.Invoke(); });
        }

        public void SetContent(int id, string name, Sprite img)
        {
            this.id = id;

            HeadImageButton.image.sprite = img;
            NameInputField.text = name;
            Text.text = name;
        }

        public enum Mode
        {
            DeckListItem,
            DeckTitle,
            AddDeck
        }

        public void SetMode(Mode mode)
        {
            ImageMask.gameObject.SetActive(mode != Mode.AddDeck);
            AddButtonBlack.gameObject.SetActive(mode == Mode.AddDeck);

            DeckNameImage.gameObject.SetActive(mode == Mode.DeckListItem);
            NameInputField.gameObject.SetActive(mode == Mode.DeckTitle);
            // DeleteButtonBlack.gameObject.SetActive(mode == Mode.DeckListItem);
        }
    }
}