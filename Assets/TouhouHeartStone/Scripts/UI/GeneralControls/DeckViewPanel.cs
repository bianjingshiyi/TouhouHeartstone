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
            }, item =>
            {
                DeckListItem deckListItem = item.GetComponent<DeckListItem>();
                deckListItem.onClick += onClickDeckListItemCallback;
            }, item =>
            {
                DeckListItem deckListItem = item.GetComponent<DeckListItem>();
                deckListItem.onClick -= onClickDeckListItemCallback;
            });
        }

        public void update((string name, Sprite image)[] deckViewData)
        {
            UIHelper.updateList(_deckListRoot, _deckListItemTemplate, _deckItemList, deckViewData.Length, (i, item) =>
            {
                DeckListItem deckListItem = item.GetComponentInChildren<DeckListItem>();
                deckListItem.updateView(deckViewData[i].name, deckViewData[i].image);
                deckListItem.SetMode(DeckListItem.Mode.DeckSelect);
            }, item =>
            {
                DeckListItem deckListItem = item.GetComponent<DeckListItem>();
                deckListItem.onClick += onClickDeckListItemCallback;
            }, item =>
            {
                DeckListItem deckListItem = item.GetComponent<DeckListItem>();
                deckListItem.onClick -= onClickDeckListItemCallback;
            });
        }

        public void updateCardPools((string name, Sprite image)[] cardPoolViewData, Action onAddClick)
        {
            int count = cardPoolViewData.Length + 1;
            UIHelper.updateList(_deckListRoot, _deckListItemTemplate, _deckItemList, count, (i, item) =>
            {
                DeckListItem deckListItem = item.GetComponentInChildren<DeckListItem>();
                if (i < count - 1)
                {
                    deckListItem.updateView(cardPoolViewData[i].name, cardPoolViewData[i].image);
                    deckListItem.SetMode(DeckListItem.Mode.DeckSelect);
                }
                else
                {
                    deckListItem.SetMode(DeckListItem.Mode.AddCardPool);
                }
            }, item =>
            {
                DeckListItem deckListItem = item.GetComponent<DeckListItem>();
                deckListItem.onClick += onClickDeckListItemCallback;
                deckListItem.onCardPoolAdd += onAddClick;
            }, item =>
            {
                DeckListItem deckListItem = item.GetComponent<DeckListItem>();
                deckListItem.onClick -= onClickDeckListItemCallback;
                deckListItem.onCardPoolAdd -= onAddClick;
            });
        }
        public void updateAddCardPools((string name, Sprite image, bool selected)[] deckViewData)
        {
            UIHelper.updateList(_deckListRoot, _deckListItemTemplate, _deckItemList, deckViewData.Length, (i, item) =>
            {
                DeckListItem deckListItem = item.GetComponentInChildren<DeckListItem>();
                deckListItem.updateView(deckViewData[i].name, deckViewData[i].image);
                deckListItem.SetMode(DeckListItem.Mode.DeckSelect);
                deckListItem.SetToggleValue(deckViewData[i].selected);
            }, item =>
            {
                DeckListItem deckListItem = item.GetComponent<DeckListItem>();
                deckListItem.onClick += onClickDeckListItemCallback;
                deckListItem.onToggleValueChanged += (value) => onDeckListItemToggleValueChangedCallback(deckListItem, value);
            }, item =>
            {
                DeckListItem deckListItem = item.GetComponent<DeckListItem>();
                deckListItem.onClick -= onClickDeckListItemCallback;
                deckListItem.onToggleValueChanged += (value) => onDeckListItemToggleValueChangedCallback(deckListItem, value);
            });
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

        private void onClickDeckListItemCallback(DeckListItem item)
        {
            onClickDeckListItem?.Invoke(_deckItemList.IndexOf(item.transform as RectTransform));
        }
        private void onDeckListItemToggleValueChangedCallback(DeckListItem item, bool value)
        {
            onDeckListItemToggleValueChanged?.Invoke(_deckItemList.IndexOf(item.transform as RectTransform), value);
        }

        #endregion

        #endregion

        #region 事件

        /// <summary>
        /// 当点击右侧卡组列表项，参数为索引
        /// </summary>
        public event Action<int> onClickDeckListItem;
        /// <summary>
        /// 当更改卡组列表的勾选框的值，参数为索引、更改的值
        /// </summary>
        public event Action<int, bool> onDeckListItemToggleValueChanged;
        
        #endregion

        #region 属性字段

        [SerializeField] private RectTransform _deckListRoot;
        [SerializeField] private RectTransform _deckListItemTemplate;
        [SerializeField] List<RectTransform> _deckItemList = new List<RectTransform>();

        public List<RectTransform> DeckItemList => _deckItemList;
        #endregion
    }
}