using System;
using System.Collections.Generic;
using UnityEngine;
using UIHelper = BJSYGameCore.UI.UIHelper;

namespace UI
{
    public class DeckViewPanel : MonoBehaviour
    {
        #region 公有方法

        public void update((string name, Sprite image, bool isWarning, bool hasPrivatePools)[] deckViewData)
        {
            UIHelper.updateList(_deckListRoot, _deckListItemTemplate, _deckItemList, deckViewData.Length, (i, item) =>
            {
                DeckListItem deckListItem = item.GetComponentInChildren<DeckListItem>();
                deckListItem.updateView(deckViewData[i].name, deckViewData[i].image);
                deckListItem.setWarning(deckViewData[i].isWarning);
                deckListItem.SetMode(DeckListItem.Mode.DeckSelect);
                deckListItem.SetSelectable(!deckViewData[i].isWarning);
                deckListItem.setCardRentWarning(deckViewData[i].hasPrivatePools);
            },onAddDeckListItem, onDestroyDeckListItem);
        }

        public void update((string name, Sprite image)[] deckViewData)
        {
            UIHelper.updateList(_deckListRoot, _deckListItemTemplate, _deckItemList, deckViewData.Length, (i, item) =>
            {
                DeckListItem deckListItem = item.GetComponentInChildren<DeckListItem>();
                deckListItem.updateView(deckViewData[i].name, deckViewData[i].image);
                deckListItem.SetMode(DeckListItem.Mode.DeckSelect);
            }, onAddDeckListItem, onDestroyDeckListItem);
        }

        public void updateCardPools((string name, Sprite image)[] cardPoolViewData, bool showEditButton)
        {
            int count = cardPoolViewData.Length;
            if (showEditButton)
                count++;
            UIHelper.updateList(_deckListRoot, _deckListItemTemplate, _deckItemList, count, (i, item) =>
            {
                DeckListItem deckListItem = item.GetComponentInChildren<DeckListItem>();
                if (!showEditButton || i < count - 1)
                {
                    deckListItem.updateView(cardPoolViewData[i].name, cardPoolViewData[i].image);
                    deckListItem.SetMode(DeckListItem.Mode.DeckSelect);
                }
                else
                {
                    deckListItem.SetMode(DeckListItem.Mode.EditCardPool);
                }
            }, onAddDeckListItem, onDestroyDeckListItem);
        }
        public void updateAddCardPools((string name, Sprite image, bool selected)[] deckViewData)
        {
            UIHelper.updateList(_deckListRoot, _deckListItemTemplate, _deckItemList, deckViewData.Length, (i, item) =>
            {
                DeckListItem deckListItem = item.GetComponentInChildren<DeckListItem>();
                deckListItem.updateView(deckViewData[i].name, deckViewData[i].image);
                deckListItem.SetMode(DeckListItem.Mode.CardPoolSelect);
                deckListItem.setToggleValue(deckViewData[i].selected);
            }, onAddDeckListItem, onDestroyDeckListItem);
        }
        #endregion

        #region 私有方法

        #region 生命周期

        private void Awake()
        {
            if (_deckListItemTemplate.transform.parent == _deckListRoot)
            {
                _deckListItemTemplate.gameObject.SetActive(false);
            }
        }

        #endregion

        #region 事件回调
        private void onAddDeckListItem(RectTransform item)
        {
            DeckListItem deckListItem = item.GetComponent<DeckListItem>();
            deckListItem.onClick += onClickDeckListItemCallback;
            deckListItem.onClickDeleteButton += onClickItemDeleteButtonCallback;
            deckListItem.onAddCardPoolClick += onClickEditCardPoolCallback;
            deckListItem.onToggleValueChanged += onDeckListItemToggleValueChangedCallback;
        }
        private void onDestroyDeckListItem(RectTransform item)
        {
            DeckListItem deckListItem = item.GetComponent<DeckListItem>();
            deckListItem.onClick -= onClickDeckListItemCallback;
            deckListItem.onClickDeleteButton -= onClickItemDeleteButtonCallback;
            deckListItem.onAddCardPoolClick -= onClickEditCardPoolCallback;
            deckListItem.onToggleValueChanged -= onDeckListItemToggleValueChangedCallback;
        }
        private void onClickDeckListItemCallback(DeckListItem item)
        {
            onClickItemButton?.Invoke(_deckItemList.IndexOf(item.transform as RectTransform));
        }
        private void onClickItemDeleteButtonCallback(DeckListItem item)
        {
            onClickItemDeleteButton?.Invoke(_deckItemList.IndexOf(item.transform as RectTransform));
        }
        private void onClickEditCardPoolCallback(DeckListItem item)
        {
            onClickEditCardPools?.Invoke();
        }
        private void onDeckListItemToggleValueChangedCallback(DeckListItem item, bool value)
        {
            onItemToggleValueChanged?.Invoke(_deckItemList.IndexOf(item.transform as RectTransform), value);
        }

        #endregion

        #endregion

        #region 事件
        public event Action<int> onClickItemButton;
        public event Action<int> onClickItemDeleteButton;
        public event Action onClickEditCardPools;
        public event Action<int, bool> onItemToggleValueChanged;
        #endregion

        #region 属性字段

        [SerializeField] private RectTransform _deckListRoot;
        [SerializeField] private RectTransform _deckListItemTemplate;
        [SerializeField] List<RectTransform> _deckItemList = new List<RectTransform>();

        public List<RectTransform> DeckItemList => _deckItemList;
        #endregion
    }
}