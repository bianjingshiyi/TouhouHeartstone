using System.Collections;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using TouhouCardEngine;
using TouhouHeartstone;
using System.Reflection;
using Game;
using TouhouHeartstone.Builtin;
using System;
using BJSYGameCore.UI;
using System.IO.Compression;
using System.IO;
using MukioI18n;
using NitoriNetwork.Common;
using System.Threading.Tasks;
using UnityEngine.UI;

namespace UI
{
    /// <summary>
    /// 卡牌构筑界面
    /// </summary>
    partial class BuildCardPanel : CardViewPanel
    {
        #region 公有方法
        public void setEnableDrag(bool value)
        {
            _enableDrag = value;
        }
        public void setCharacterName(string name)
        {
            _characterNameText.text = name;
        }
        public void setDeckCardCount(int count, int max)
        {
            _deckCardCountText.text = count + "/" + max;
        }
        public void setEditCardListActive(bool value)
        {
            _editCardListRightPanel.SetActive(value);
        }
        public void showDragCard(bool isDeckListItem)
        {
            _dragDeckListItem.gameObject.SetActive(isDeckListItem);
            _dragCard.gameObject.SetActive(!isDeckListItem);
        }
        public void moveDragCard(Vector2 position)
        {
            _dragCard.rectTransform.position = position;
            _dragDeckListItem.transform.position = position;
        }
        public void hideDragCard()
        {
            _dragDeckListItem.gameObject.SetActive(false);
            _dragCard.gameObject.SetActive(false);
        }
        public void showMasterSelectPanel((string name, Sprite image)[] viewData)
        {
            _masterSelectionPanel.SetActive(true);
            _masterSelectionDeckViewPanel.update(viewData);
        }
        public void hideMasterSelectPanel()
        {
            _masterSelectionPanel.SetActive(false);
        }
        public void setLoadingUIActive(bool value)
        {
            LoadingImg.SetActive(value);
        }
        public void showErrorTip(string msg)
        {
            HintMessageText.text = msg;
            HintMessageText.gameObject.SetActive(false);
            HintMessageText.gameObject.SetActive(true);
            HintMessageText.gameObject.GetComponent<SEPlayer>()?.Play();
        }
        public void showErrorTip(bool value)
        {
            HintMessageText.gameObject.SetActive(value);
        }
        /// <summary>
        /// 判断坐标是否位于卡组卡牌列表区域中
        /// </summary>
        /// <param name="position"></param>
        /// <returns></returns>
        public bool isPositionOverDeckCardList(Vector2 position)
        {
            return _deckListViewportRect.rect.Contains(_deckListViewportRect.InverseTransformPoint(position));
        }
        #endregion
        #region 私有方法
        #region 生命周期
        protected override void Awake()
        {
            base.Awake();

            HintMessageText.gameObject.SetActive(false);
            setLoadingUIActive(false);

            _exportDeckButton.onClick.AddListener(onClickExportDeckButtonCallback);
            _exitEditButton.onClick.AddListener(onClickExitEditButtonCallback);
            _importDeckButton.onClick.AddListener(onClickImportDeckButtonCallback);
            _uploadDeckButton.onClick.AddListener(onClickUploadDeckButtonCallback);
            _downloadDeckButton.onClick.AddListener(onClickDownloadDeckButtonCallback);
        }
        #endregion
        #region 事件回调
        protected override void onDeckCardListItemEnable(RectTransform itemRect)
        {
            base.onDeckCardListItemEnable(itemRect);
            CardViewDeckCardListItem item = itemRect.GetComponent<CardViewDeckCardListItem>();
            item.onBeginDrag += onBeginDragDeckCardListItemCallback;
            item.onDrag += onDragDeckCardListItemCallback;
            item.onEndDrag += onEndDragDeckCardListItemCallback;
        }
        protected override void onDeckCardListItemDisable(RectTransform itemRect)
        {
            base.onDeckCardListItemDisable(itemRect);
            CardViewDeckCardListItem item = itemRect.GetComponent<CardViewDeckCardListItem>();
            item.onBeginDrag -= onBeginDragDeckCardListItemCallback;
            item.onDrag -= onDragDeckCardListItemCallback;
            item.onEndDrag -= onEndDragDeckCardListItemCallback;
        }
        private void onBeginDragDeckCardListItemCallback(CardViewDeckCardListItem item, PointerEventData eventData)
        {
            if (!_enableDrag)
                return;

            _dragStartPosition = eventData.position;
            _isDraggingHorizontal = false;
            // eventData.Use();
        }
        private void onDragDeckCardListItemCallback(CardViewDeckCardListItem item, PointerEventData eventData)
        {
            if (!_enableDrag)
                return;

            var delta = _dragStartPosition - eventData.position;
            if (Math.Abs(delta.x) > _dragThreshold && Math.Abs(delta.x) > Math.Abs(delta.y))
            {
                if (!_isDraggingHorizontal)
                {
                    _isDraggingHorizontal = true;
                    showDragCard(_deckListViewportRect.rect.Contains(_deckListViewportRect.InverseTransformPoint(eventData.position)));
                    onStartDragDeckCardListItem?.Invoke(deckCardItemList.IndexOf(item.transform as RectTransform));
                    eventData.Use();
                }
            }
            if (_isDraggingHorizontal)
            {
                _dragCard.rectTransform.position = eventData.position;
                _dragDeckListItem.transform.position = eventData.position;
                showDragCard(_deckListViewportRect.rect.Contains(_deckListViewportRect.InverseTransformPoint(eventData.position)));
                eventData.Use();
            }
        }
        private void onEndDragDeckCardListItemCallback(CardViewDeckCardListItem item, PointerEventData eventData)
        {
            if (!_enableDrag)
            {
                return;
            }
            if (_isDraggingHorizontal)
            {
                //结束拖拽
                _isDraggingHorizontal = false;
                hideDragCard();
                Vector2 position = eventData.position;
                onEndDragDeckCardListItem?.Invoke(isPositionOverDeckCardList(position));
                eventData.Use();
            }
        }
        private void onClickExportDeckButtonCallback()
        {
            onClickExportDeckButton?.Invoke();
        }
        private void onClickExitEditButtonCallback()
        {
            onClickExitEditButton?.Invoke();
        }
        private void onClickImportDeckButtonCallback()
        {
            onClickImportDeckButton?.Invoke();
        }
        private void onClickUploadDeckButtonCallback()
        {
            onClickUploadDeckButton?.Invoke();
        }
        private void onClickDownloadDeckButtonCallback()
        {
            onClickDownloadDeckButton?.Invoke();
        }
        #endregion
        #endregion
        #region 事件
        public event Action<int> onStartDargCardListItem;
        public event Action<int> onStartDragDeckCardListItem;
        /// <summary>
        /// 当拖拽卡牌列表中的项目结束时调用，参数为是否位于卡组卡牌列表区域当中
        /// </summary>
        public event Action<bool> onEndDragCardListItem;
        /// <summary>
        /// 当拖拽卡组卡牌列表中的项目结束时调用，参数为是否位于卡组卡牌列表区域当中
        /// </summary>
        public event Action<bool> onEndDragDeckCardListItem;
        public event Action<int> onSortTypeValueChanged;
        public event Action<bool[]> onCharacterFilterValueChanged;
        public event Action<bool[]> onCostFilterValueChanged;
        public event Action<bool[]> onTypeFilterValueChanged;
        public event Action<int> onClickMasterSelectPanelItem
        {
            add { _masterSelectionDeckViewPanel.onClickDeckListItem += value; }
            remove { _masterSelectionDeckViewPanel.onClickDeckListItem -= value; }
        }
        public event Action onClickExportDeckButton;
        public event Action onClickExitEditButton;
        public event Action onClickImportDeckButton;
        public event Action onClickDownloadDeckButton;
        public event Action onClickUploadDeckButton;
        #endregion
        /// <summary>
        /// 当前正在编辑的卡组的卡片
        /// </summary>
        List<int> currentDeckCards { get; set; } = new List<int>();
        /// <summary>
        /// 当松开拖拽构筑牌库中的卡牌
        /// </summary>
        /// <param name="item"></param>
        /// <param name="pointer"></param>
        public void onReleaseDeckItem(BuildDeckListItem item, PointerEventData pointer)
        {
            DragCard.hide();
            Vector3 localPosition = DeckViewScroll.GetComponent<RectTransform>().InverseTransformPoint(pointer.position);
            if (DeckViewScroll.GetComponent<RectTransform>().rect.Contains(localPosition))
            {
                item.recovery();
            }
            else
            {
                if (currentDeckCards.Contains(item.card.id))
                    currentDeckCards.Remove(item.card.id);

                // 更新可用信息
                var cardCount = currentDeckCards.Where(i => i == item.card.id).Count();
                //updateCardRemainTextAt(item.card.id, cardCount);

                // 更新UI
                if (item.count < 1)
                    BuildDeckList.removeItem(item);

                //onDeckCardChange();
            }
        }
        public void onClickHandCard()
        {
            //if (_enableDrag)
            //    showMessage(lang._(DRAG_INSTEAD_CLICK));
        }

        public void DeckItemSelect(BuildDeckListItem item)
        {
            //DragCard.display();
            //DragCard.Card.display();
            //DragCard.BuildDeckListItem.hide();

            //DragCard.Card.setCard(item.card, ui.getManager<CardManager>().getSkin(item.card.id));

            //var rt = DragCard.rectTransform;
            //var rt2 = item.GetComponent<RectTransform>();

            //Vector2 point = new Vector2(rt2.position.x - (rt2.rect.width + rt.rect.width) / 2, rt2.position.y);
            //point.y = Math.Max(point.y, rt.rect.height / 2 + 10);

            //rt.position = point;
        }

        public void DeckItemDeselect(BuildDeckListItem item)
        {
            DragCard.hide();
        }
        #region 属性字段
        public Card dragCard => _dragCard;
        public CardViewDeckCardListItem dragDeckListItem => _dragDeckListItem;
        [SerializeField]
        float _dragThreshold = 50f;
        [SerializeField]
        RectTransform _deckListViewportRect;
        [SerializeField]
        Text _characterNameText;
        [SerializeField]
        Text _deckCardCountText;
        /// <summary>
        /// 当前能否拖拽卡牌
        /// </summary>
        [SerializeField]
        bool _enableDrag;
        [SerializeField]
        Card _dragCard;
        [SerializeField]
        CardViewDeckCardListItem _dragDeckListItem;
        [SerializeField]
        Vector2 _dragStartPosition;
        [SerializeField]
        bool _isDraggingHorizontal = false;
        [SerializeField]
        GameObject _editCardListRightPanel;
        [SerializeField]
        GameObject _masterSelectionPanel;
        [SerializeField] 
        DeckViewPanel _masterSelectionDeckViewPanel;
        [SerializeField]
        Button _exitEditButton;
        [SerializeField]
        Button _exportDeckButton;
        [SerializeField]
        Button _importDeckButton;
        [SerializeField]
        Button _uploadDeckButton;
        [SerializeField]
        Button _downloadDeckButton;
        #endregion
    }
}