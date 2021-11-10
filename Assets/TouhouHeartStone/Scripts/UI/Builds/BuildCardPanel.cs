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
        public void setRightFilterDetialPanelActive(bool value)
        {
            _filterDetailRightPanel.SetActive(value);
        }
        public void switchRightPanelModeButtonGroup(bool isToEditExternalCardPool)
        {
            _switchRightToDeckButton.gameObject.SetActive(!isToEditExternalCardPool);
            _switchRightToFilterButton.gameObject.SetActive(isToEditExternalCardPool);
        }
        public void setSortTypes(string[] sortTypes)
        {
            _sortDropdown.ClearOptions();
            _sortDropdown.AddOptions(new List<string>(sortTypes));
        }
        public void setSortType(int index)
        {
            _sortDropdown.SetValueWithoutNotify(index);
        }
        public void setCharacterFilters(string[] characterFilters)
        {
            _characterFilterDropdown.ClearOptions();
            _characterFilterDropdown.AddOptions(new List<string>(characterFilters));
        }
        public void setCharacterFilter(bool[] value)
        {
            _characterFilterDropdown.SetValueWithoutNotify(value);
        }
        public void setCostFilters(string[] costFilters)
        {
            _costFilterDropdown.ClearOptions();
            _costFilterDropdown.AddOptions(new List<string>(costFilters));
        }
        public void setCostFilter(bool[] value)
        {
            _costFilterDropdown.SetValueWithoutNotify(value);
        }
        public void setTypeFilters(string[] typeFilters)
        {
            _typeFilterDropdown.ClearOptions();
            _typeFilterDropdown.AddOptions(new List<string>(typeFilters));
        }
        public void setTypeFilter(bool[] value)
        {
            _typeFilterDropdown.SetValueWithoutNotify(value);
        }
        //public void addFilterDetailCardPoolItem(string cardPoolName, bool value)
        //{
        //    RectTransform itemRect = Instantiate(_filterDetailCardPoolItemTemplate, _filterDetailCardPoolItemRoot);
        //    _filterDetailCardPoolItemList.Add(itemRect);
        //    itemRect.gameObject.SetActive(true);
        //    BuildFilterDetailCardPoolItem item = itemRect.GetComponent<BuildFilterDetailCardPoolItem>();
        //    item.setName(cardPoolName);
        //    item.setValue(value);
        //    item.onValueChanged += onFilterDetialCardPoolItemToggleValueChangedCallback;
        //}
        //public void clearFilterDetailCardPoolItems()
        //{
        //    for (int i = 0; i < _filterDetailCardPoolItemList.Count; i++)
        //    {
        //        if (_filterDetailCardPoolItemList[i] == null)
        //            continue;
        //        BuildFilterDetailCardPoolItem item = _filterDetailCardPoolItemList[i].GetComponent<BuildFilterDetailCardPoolItem>();
        //        item.onValueChanged -= onFilterDetialCardPoolItemToggleValueChangedCallback;
        //        Destroy(_filterDetailCardPoolItemList[i].gameObject);
        //    }
        //    _filterDetailCardPoolItemList.Clear();
        //}
        public void updateFilterDetailCardPoolList((string name, bool value)[] viewData)
        {
            BJSYGameCore.UI.UIHelper.updateList(_filterDetailCardPoolItemRoot, _filterDetailCardPoolItemTemplate, _filterDetailCardPoolItemList, viewData.Length, (i, itemRect) =>
            {
                BuildFilterDetailCardPoolItem item = itemRect.GetComponent<BuildFilterDetailCardPoolItem>();
                item.setName(viewData[i].name);
                item.setValue(viewData[i].value);
            }, itemRect =>
            {
                BuildFilterDetailCardPoolItem item = itemRect.GetComponent<BuildFilterDetailCardPoolItem>();
                item.onValueChanged += onFilterDetialCardPoolItemToggleValueChangedCallback;
            }, itemRect =>
            {
                BuildFilterDetailCardPoolItem item = itemRect.GetComponent<BuildFilterDetailCardPoolItem>();
                item.onValueChanged -= onFilterDetialCardPoolItemToggleValueChangedCallback;
            });
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
        public void hideDragCard()
        {
            _dragDeckListItem.gameObject.SetActive(false);
            _dragCard.gameObject.SetActive(false);
        }
        public void showMasterSelectPanel((string name, Sprite image)[] viewData)
        {
            _masterSelectionPanel.updateItemList(viewData);
            _masterSelectionPanel.gameObject.SetActive(true);
        }
        public void hideMasterSelectPanel()
        {
            _masterSelectionPanel.gameObject.SetActive(false);
        }
        public void setLoadingUIActive(bool value)
        {
            LoadingImg.SetActive(value);
        }
        public void showMessage(string msg)
        {
            HintMessageText.text = msg;
            HintMessageText.gameObject.SetActive(false);
            HintMessageText.gameObject.SetActive(true);
            HintMessageText.gameObject.GetComponent<SEPlayer>()?.Play();
        }
        #endregion
        #region 私有方法
        #region 生命周期
        protected override void Awake()
        {
            base.Awake();

            HintMessageText.gameObject.SetActive(false);
            setLoadingUIActive(false);

            _characterFilterDropdown.onValueChanged.AddListener(onCharacterFilterValueChangedCallback);
            _costFilterDropdown.onValueChanged.AddListener(onCostFilterValueChangedCallback);
            _typeFilterDropdown.onValueChanged.AddListener(onTypeFilterValueChangedCallback);
            _sortDropdown.onValueChanged.AddListener(onSortTypeValueChangedCallback);
            _switchRightToDeckButton.onClick.AddListener(onSwitchRightToDeckButtonClickCallback);
            _switchRightToFilterButton.onClick.AddListener(onSwitchRightToFilterDetailButtonClickCallback);

            _exportDeckButton.onClick.AddListener(onClickExportDeckButtonCallback);
            _exitEditButton.onClick.AddListener(onClickExitEditButtonCallback);
            _importDeckButton.onClick.AddListener(onClickImportDeckButtonCallback);
            _uploadDeckButton.onClick.AddListener(onClickUploadDeckButtonCallback);
            _downloadDeckButton.onClick.AddListener(onClickDownloadDeckButtonCallback);
            if (_filterDetailCardPoolItemTemplate.transform.parent == _filterDetailCardPoolItemRoot)
            {
                _filterDetailCardPoolItemTemplate.gameObject.SetActive(false);
            }
        }
        #endregion
        #region 事件回调
        protected override void onEnableCardListItem(int index, RectTransform itemRect)
        {
            base.onEnableCardListItem(index, itemRect);
            EditorCardListItem item = itemRect.GetComponent<EditorCardListItem>();
            item.onBeginDrag += onBeginDragCardListItemCallback;
            item.onDrag += onDragCardListItemCallback;
            item.onEndDrag += onEndDragCardListItemCallback;
        }
        protected override void onDisableCardListItem(int index, RectTransform itemRect)
        {
            base.onDisableCardListItem(index, itemRect);
            EditorCardListItem item = itemRect.GetComponent<EditorCardListItem>();
            item.onBeginDrag -= onBeginDragCardListItemCallback;
            item.onDrag -= onDragCardListItemCallback;
            item.onEndDrag -= onEndDragCardListItemCallback;
        }
        private void onBeginDragCardListItemCallback(EditorCardListItem item, PointerEventData eventData)
        {
            _dragStartPosition = eventData.position;
            _isDraggingHorizontal = false;
            _isDraggingVertical = false;
            eventData.Use();
        }
        private void onDragCardListItemCallback(EditorCardListItem item, PointerEventData eventData)
        {
            if (!_isDraggingHorizontal && !_isDraggingVertical)
            {
                var delta = _dragStartPosition - eventData.position;
                if (delta.magnitude > 0)
                {
                    if (Math.Abs(delta.x) > Math.Abs(delta.y))
                    {
                        if (_enableDrag && Math.Abs(delta.x) > _dragThreshold)
                        {
                            _isDraggingHorizontal = true;
                            showDragCard(_deckListViewportRect.rect.Contains(_deckListViewportRect.InverseTransformPoint(eventData.position)));
                            onStartDargCardListItem?.Invoke(cardListLayoutGroup.getItemIndex(item.transform as RectTransform));
                        }
                    }
                    else
                    {
                        _isDraggingVertical = true;
                        _cardListScrollRect.OnBeginDrag(eventData);
                    }
                }
            }
            else if (_isDraggingHorizontal)
            {
                _dragCard.rectTransform.position = eventData.position;
                _dragDeckListItem.transform.position = eventData.position;
                showDragCard(_deckListViewportRect.rect.Contains(_deckListViewportRect.InverseTransformPoint(eventData.position)));
            }
            else if (_isDraggingVertical)
            {
                _cardListScrollRect.OnDrag(eventData);
            }
            eventData.Use();
        }
        private void onEndDragCardListItemCallback(EditorCardListItem item, PointerEventData eventData)
        {
            if (_isDraggingHorizontal)
            {
                // 结束拖拽
                hideDragCard();
                onEndDragCardListItem?.Invoke(_deckListViewportRect.rect.Contains(_deckListViewportRect.InverseTransformPoint(eventData.position)));
                _isDraggingHorizontal = false;
            }
            else if (_isDraggingVertical)
            {
                _cardListScrollRect.OnEndDrag(eventData);
                _isDraggingVertical = false;
            }
            eventData.Use();
        }
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
            eventData.Use();
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
                }
            }
            if (_isDraggingHorizontal)
            {
                _dragCard.rectTransform.position = eventData.position;
                _dragDeckListItem.transform.position = eventData.position;
                showDragCard(_deckListViewportRect.rect.Contains(_deckListViewportRect.InverseTransformPoint(eventData.position)));
            }
            eventData.Use();
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
                hideDragCard();
                onEndDragDeckCardListItem?.Invoke(_deckListViewportRect.rect.Contains(_deckListViewportRect.InverseTransformPoint(eventData.position)));
            }
            eventData.Use();
            //gameObject.SetActive(true);
            //GetComponentInParent<ScrollRect>()?.SendMessage("OnEndDrag", eventData, SendMessageOptions.DontRequireReceiver);
        }
        /// <summary>
        /// 修改了排序
        /// </summary>
        private void onSortTypeValueChangedCallback(int value)
        {
            onSortTypeValueChanged?.Invoke(value);
        }
        private void onCharacterFilterValueChangedCallback(bool[] value)
        {
            onCharacterFilterValueChanged?.Invoke(value);
        }
        private void onCostFilterValueChangedCallback(bool[] value)
        {
            onCostFilterValueChanged?.Invoke(value);
        }
        private void onTypeFilterValueChangedCallback(bool[] value)
        {
            onTypeFilterValueChanged?.Invoke(value);
        }
        private void onFilterDetialCardPoolItemToggleValueChangedCallback(BuildFilterDetailCardPoolItem item, bool value)
        {
            onFilterDetailCardPoolItemToggleValueChanged?.Invoke(_filterDetailCardPoolItemList.IndexOf(item.transform as RectTransform), value);
        }
        private void onSwitchRightToDeckButtonClickCallback()
        {
            onSwitchRightToDeckButtonClick?.Invoke();
        }
        private void onSwitchRightToFilterDetailButtonClickCallback()
        {
            onSwitchRightToFilterDetailButtonClick?.Invoke();
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
        public event Action onSwitchRightToFilterDetailButtonClick;
        public event Action onSwitchRightToDeckButtonClick;
        public event Action<int, bool> onFilterDetailCardPoolItemToggleValueChanged;
        public event Action<int> onClickMasterSelectPanelItem
        {
            add { _masterSelectionPanel.onClickItem += value; }
            remove { _masterSelectionPanel.onClickItem -= value; }
        }
        public event Action onClickExportDeckButton;
        public event Action onClickExitEditButton;
        public event Action onClickImportDeckButton;
        public event Action onClickDownloadDeckButton;
        public event Action onClickUploadDeckButton;
        #endregion
        /// <summary>
        /// 当前正在编辑的卡组ID
        /// </summary>
        int currentDeckID = 0;
        /// <summary>
        /// 当前正在编辑的卡组的Master卡的ID
        /// </summary>
        int currentEditingDeckMasterID = 0;
        /// <summary>
        /// 当前正在编辑的卡组的卡片
        /// </summary>
        List<int> currentDeckCards { get; set; } = new List<int>();
        /// <summary>
        /// 当前卡组是否修改
        /// </summary>
        bool deckChanged { get; set; } = false;
        [TranslateMsg]
        const string PILE_NOT_SAVE = "卡组尚未保存，退出后现有变更将丢失。\n是否退出？";
        [TranslateMsg]
        const string CARD_NOT_EXISTS = "ID为{0}的卡不存在";
        [TranslateMsg]
        const string PILE_CODE_INCOMPLETE = "卡组代码不完整";
        [TranslateMsg]
        const string PILE_CODE_CORRUPTED = "卡组数据已损坏";
        [TranslateMsg]
        const string PILE_IMPORT_ERROR = "导入错误\n{0}";
        [TranslateMsg]
        const string PILE_UPLOAD_WARN_MSG = "卡组数据上传功能将会覆盖您在云端的数据，是否上传？";
        [TranslateMsg]
        const string PILE_UPLOAD_NO_LOGIN = "游客无法使用此功能。";
        [TranslateMsg]
        const string PILE_DOWNLOAD_WARN_MSG = "卡组数据下载功能将会从云端同步你的卡组数据并与本地数据合并，可能会覆盖本地的某些卡组，是否下载？";
        [TranslateMsg]
        const string PILE_UPLOAD_SUCCES = "卡组上传完毕";
        [TranslateMsg]
        const string PILE_DOWNLOAD_SUCCES = "卡组下载完毕";
        BuildCardViewData[] displayedCardList = new BuildCardViewData[0];
        ///// <summary>
        ///// 所有可用的卡
        ///// </summary>
        //CardDefine[] avaliableCards { get; set; } = null;

        /// <summary>
        /// 切换模式
        /// </summary>
        /// <param name="deckMode">卡组列表模式</param>
        void uiModeSwitch(bool deckMode)
        {
            DeckListViewImage.gameObject.SetActive(deckMode);
            DeckCardViewImage.gameObject.SetActive(!deckMode);

            _enableDrag = !deckMode;

            // 退出组卡时清理
            if (deckMode)
            {
                currentDeckCards.Clear();
            }

            //updateCardRemainText();
            deckChanged = false;
        }
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



        [TranslateMsg]
        const string DRAG_INSTEAD_CLICK = "请将卡片拖拽到卡组列表中组卡";

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
        Dropdown _sortDropdown;
        [SerializeField]
        MultiSelectDropdown _characterFilterDropdown;
        [SerializeField]
        MultiSelectDropdown _costFilterDropdown;
        [SerializeField]
        MultiSelectDropdown _typeFilterDropdown;
        [SerializeField]
        ScrollRect _cardListScrollRect;
        [SerializeField]
        Button _switchRightToDeckButton;
        [SerializeField]
        Button _switchRightToFilterButton;
        [SerializeField]
        RectTransform _deckListViewportRect;
        [SerializeField]
        GameObject _filterDetailRightPanel;
        [SerializeField]
        RectTransform _filterDetailCardPoolItemRoot;
        [SerializeField]
        RectTransform _filterDetailCardPoolItemTemplate;
        [SerializeField]
        List<RectTransform> _filterDetailCardPoolItemList = new List<RectTransform>();
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
        bool _isDraggingVertical = false;
        [SerializeField]
        GameObject _editCardListRightPanel;
        [SerializeField]
        MasterCardSelection _masterSelectionPanel;
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