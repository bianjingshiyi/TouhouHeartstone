using System;
using System.Collections.Generic;
using System.Linq;
using Game;
using UnityEngine;
using UnityEngine.Serialization;
using UIHelper = BJSYGameCore.UI.UIHelper;

namespace UI
{
    public class DeckViewPanel : MonoBehaviour
    {
        #region 公有方法

        public void update((string name, Sprite image, bool isWarning)[] deckViewData)
        {
            clear();
            UIHelper.updateList(_deckListRoot, _deckListItemTemplate, _deckItemList, deckViewData.Length, (i, item) =>
            {
                DeckListItem deckListItem = item.GetComponentInChildren<DeckListItem>();
                deckListItem.updateView(deckViewData[i].name, deckViewData[i].image);
                deckListItem.setWarning(deckViewData[i].isWarning);
                deckListItem.SetMode(DeckListItem.Mode.DeckSelect);
                deckListItem.SetSelectable(!deckViewData[i].isWarning);
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
            clear();
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

        public void clear()
        {
            UIHelper.updateList(_deckListRoot, _deckListItemTemplate, _deckItemList, 0, (i, rectTransform) => { },
                rectTransform => { }, rectTransform => { });
        }

        public void setItems((int index, string name, Sprite image)[] viewData)
        {
            clear();
            UIHelper.updateList(_deckListRoot, _deckListItemTemplate, _deckItemList, viewData.Length,
                (i, item) =>
                {
                    DeckListItem deckListItem = item.GetComponentInChildren<DeckListItem>();
                    deckListItem.SetContent(viewData[i].index, viewData[i].name, viewData[i].image);
                    deckListItem.SetMode(DeckListItem.Mode.DeckSelect);
                },
                item =>
                {
                    DeckListItem deckListItem = item.GetComponent<DeckListItem>();
                    deckListItem.onDeckSelect += onSelectDeckListItemCallback;
                }, item =>
                {
                    DeckListItem deckListItem = item.GetComponent<DeckListItem>();
                    deckListItem.onDeckSelect -= onSelectDeckListItemCallback;
                }
            );
        }

        public void setItems(CardManager cardManager,IEnumerable<DeckDefine> decks, Action<DeckDefine, DeckListItem> action = null)
        {
            var viewData = decks.Select((define, index) => ((int) define.ID, define.Name, cardManager.getSkin(define.MasterID).image)).ToArray();
            setItems(viewData);
        }

        public void setItems(CardManager cardManager, IEnumerable<int> cards)
        {
            var viewData = cards.Select(i =>
            {
                var skin = cardManager.getSkin(i);
                return (i,skin?.name,skin?.image);
            }).ToArray();
            setItems(viewData);
        }

        public void clearEvents()
        {
            onSelect = null;
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

        private void onSelectDeckListItemCallback(int id)
        {
            onSelect?.Invoke(id);
        }

        #endregion

        #endregion

        #region 事件

        /// <summary>
        /// 当点击右侧卡组列表项，参数为索引
        /// </summary>
        public event Action<int> onClickDeckListItem;

        public event Action<int> onSelect;

        #endregion

        #region 属性字段

        [SerializeField] private RectTransform _deckListRoot;
        [SerializeField] private RectTransform _deckListItemTemplate;
        [SerializeField] List<RectTransform> _deckItemList = new List<RectTransform>();

        #endregion
    }
}