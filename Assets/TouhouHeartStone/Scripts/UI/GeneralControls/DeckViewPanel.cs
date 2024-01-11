using System;
using System.Collections.Generic;
using UnityEngine;
using UIHelper = BJSYGameCore.UI.UIHelper;

namespace UI
{
    public class DeckViewPanel : MonoBehaviour
    {
        #region 公有方法

        public void update(DeckListItemViewData[] deckViewData)
        {
            UIHelper.updateList(_deckListRoot, _deckListItemTemplate, _deckItemList, deckViewData.Length, (i, item) =>
            {
                DeckListItem deckListItem = item.GetComponentInChildren<DeckListItem>();
                var viewData = deckViewData[i];
                deckListItem.updateView(viewData);
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
            deckListItem.onToggleValueChanged += onDeckListItemToggleValueChangedCallback;
        }
        private void onDestroyDeckListItem(RectTransform item)
        {
            DeckListItem deckListItem = item.GetComponent<DeckListItem>();
            deckListItem.onClick -= onClickDeckListItemCallback;
            deckListItem.onClickDeleteButton -= onClickItemDeleteButtonCallback;
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
        private void onDeckListItemToggleValueChangedCallback(DeckListItem item, bool value)
        {
            onItemToggleValueChanged?.Invoke(_deckItemList.IndexOf(item.transform as RectTransform), value);
        }

        #endregion

        #endregion

        #region 事件
        public event Action<int> onClickItemButton;
        public event Action<int> onClickItemDeleteButton;
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