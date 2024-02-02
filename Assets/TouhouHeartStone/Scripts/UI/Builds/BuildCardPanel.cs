using System;
using System.Collections.Generic;
using System.Linq;
using Game;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace UI
{
    /// <summary>
    /// 卡牌构筑界面
    /// </summary>
    partial class BuildCardPanel : MonoBehaviour
    {
        #region 公有方法
        public void setLoadingUIActive(bool value)
        {
            _loadingImgObj.SetActive(value);
        }
        public void showErrorTip(string msg)
        {
            _errorTipText.text = msg;
            _errorTipText.gameObject.SetActive(false);
            _errorTipText.gameObject.SetActive(true);
            _errorTipText.gameObject.GetComponent<SEPlayer>()?.Play();
        }
        public void showErrorTip(bool value)
        {
            _errorTipText.gameObject.SetActive(value);
        }

        #region 新建卡组面板
        public void showMasterSelectPanel(DeckListItemViewData[] viewDatas)
        {
            _masterSelectionPanel.SetActive(true);
            _masterSelectionDeckViewPanel.update(viewDatas);
        }
        public void hideMasterSelectPanel()
        {
            _masterSelectionPanel.SetActive(false);
        }
        #endregion

        #region 卡组列表
        /// <summary>
        /// 显示右边的卡组列表
        /// </summary>
        /// <param name="value"></param>
        public void setDeckListActive(bool value)
        {
            _deckListObj.SetActive(value);
        }
        /// <summary>
        /// 更新卡组列表
        /// </summary>
        /// <param name="deckViewData"></param>
        public void updateDeckList(DeckListItemViewData[] deckViewData)
        {
            _deckList.updateList(deckViewData.Length, (i, item) =>
            {
                DeckListItem deckListItem = item.GetComponentInChildren<DeckListItem>();
                deckListItem.updateView(deckViewData[i]);
            }, item =>
            {
                DeckListItem deckListItem = item.GetComponent<DeckListItem>();
                deckListItem.onClick += onClickDeckListItemCallback;
                deckListItem.onClickDeleteButton += onClickDeckListItemDeleteButtonCallback;
                deckListItem.onNameInputFieldValueChanged += onDeckListItemNameInputFieldValueChanged;
            }, item =>
            {
                DeckListItem deckListItem = item.GetComponent<DeckListItem>();
                deckListItem.onClick -= onClickDeckListItemCallback;
                deckListItem.onClickDeleteButton -= onClickDeckListItemDeleteButtonCallback;
                deckListItem.onNameInputFieldValueChanged -= onDeckListItemNameInputFieldValueChanged;
            });
        }
        #endregion

        #region 卡组编辑
        public void setEditDeckPanelActive(bool value)
        {
            _editCardListRightPanel.SetActive(value);
        }
        public void setEnableDragDeckCard(bool value)
        {
            _enableDragDeckCard = value;
        }
        public void setCharacterName(string name)
        {
            _characterNameText.text = name;
        }
        public void setDeckCardCount(int count, int max)
        {
            _deckCardCountText.text = $"{count}/{max}";
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
        public void updateDeckInfo(DeckListItemViewData viewData)
        {
            _editDeckHeader.updateView(viewData);
        }
        /// <summary>
        /// 更新自定义卡池卡牌编辑栏卡牌列表
        /// </summary>
        public void updateDeckCardList(DeckCardViewData[] viewData)
        {
            _deckCardList.update(viewData);
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

        #endregion

        #region 私有方法

        #region 生命周期
        private void Awake()
        {
            _errorTipText.gameObject.SetActive(false);
            setLoadingUIActive(false);

            _returnButton.onClick.AddListener(() => onClickReturnButton?.Invoke());
            _addDeckButton.onClick.AddListener(() => onClickAddDeckButton?.Invoke());

            _masterSelectionCloseButton.onClick.AddListener(hideMasterSelectPanel);
            _masterSelectionDeckViewPanel.onClickItemButton += (index) => onClickMasterSelectPanelItem?.Invoke(index);

            _saveDeckButton.onClick.AddListener(onClickSaveDeckButtonCallback);
            _exportDeckButton.onClick.AddListener(onClickExportDeckButtonCallback);
            _exitEditButton.onClick.AddListener(onClickExitEditButtonCallback);
            _importDeckButton.onClick.AddListener(onClickImportDeckButtonCallback);
            _uploadDeckButton.onClick.AddListener(onClickUploadDeckButtonCallback);
            _downloadDeckButton.onClick.AddListener(onClickDownloadDeckButtonCallback);

            _editDeckHeader.onClick += onClickEditDeckImageButtonCallback;
            _editDeckHeader.onClickNameButton += onClickEditDeckNameButtonCallback;
            _editDeckHeader.onNameInputFieldValueChanged += onEndEditDeckNameInputFieldCallback;

            if (_deckCardList)
            {
                _deckCardList.onDeckItemBeginDrag += onBeginDragDeckCardListItemCallback;
                _deckCardList.onDeckItemDrag += onDragDeckCardListItemCallback;
                _deckCardList.onDeckItemEndDrag += onEndDragDeckCardListItemCallback;
                _deckCardList.onDeckItemPointerEnter += onDeckCardPointerEnterCallback;
                _deckCardList.onDeckItemPointerExit += onDeckCardPointerExitCallback;
                _deckCardList.onDeckItemDeselect += onDeckCardDeselectCallback;
            }
        }
        #endregion

        #region 事件回调
        private void onClickSaveDeckButtonCallback()
        {
            onClickSaveDeckButton?.Invoke();
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

        #region 卡组列表
        private void onClickDeckListItemCallback(DeckListItem item)
        {
            onClickDeckListItem?.Invoke(_deckList.indexOf(item));
        }
        private void onClickDeckListItemDeleteButtonCallback(DeckListItem item)
        {
            onClickDeckListItemDeleteButton?.Invoke(_deckList.indexOf(item));
        }
        private void onDeckListItemNameInputFieldValueChanged(DeckListItem item, string value)
        {
            onDeckListItemNameValueChanged?.Invoke(_deckList.indexOf(item), value);
        }
        #endregion

        #region 编辑卡组
        private void onClickEditDeckImageButtonCallback(DeckListItem item)
        {
            onClickEditDeckImageButton?.Invoke();
        }
        private void onClickEditDeckNameButtonCallback(DeckListItem item)
        {
            //编辑名称模式
            _editDeckHeader.setNameInputFieldVisible(true);
        }
        private void onEndEditDeckNameInputFieldCallback(DeckListItem item, string value)
        {
            _editDeckHeader.setNameInputFieldVisible(false);
            onDeckNameInputFieldValueChanged?.Invoke(value);
        }
        #endregion

        #region 卡组卡牌编辑
        private void onBeginDragDeckCardListItemCallback(int index, PointerEventData eventData)
        {
            if (!_enableDragDeckCard)
                return;
            onStartDragDeckCardListItem?.Invoke(index, eventData);
        }
        private void onDragDeckCardListItemCallback(int index, PointerEventData eventData)
        {
            if (!_enableDragDeckCard)
                return;
            onDragDeckCardListItem?.Invoke(index, eventData);
        }
        private void onEndDragDeckCardListItemCallback(int index, PointerEventData eventData)
        {
            if (!_enableDragDeckCard)
                return;
            onEndDragDeckCardListItem?.Invoke(index, eventData);
        }
        private void onDeckCardPointerEnterCallback(int index, PointerEventData eventData)
        {
            onDeckCardPointerEnter?.Invoke(index, eventData);
        }
        private void onDeckCardPointerExitCallback(int index, PointerEventData eventData)
        {
            onDeckCardPointerExit?.Invoke(index, eventData);
        }
        private void onDeckCardDeselectCallback(int index)
        {
            _dragCard.hide();
        }
        #endregion

        #endregion

        #endregion

        #region 事件
        public event Action onClickReturnButton;

        public event Action<int> onClickMasterSelectPanelItem;
        public event Action onClickExportDeckButton;
        public event Action onClickExitEditButton;
        public event Action onClickImportDeckButton;
        public event Action onClickDownloadDeckButton;
        public event Action onClickUploadDeckButton;

        /// <summary>
        /// 当点击右侧卡组列表添加按钮
        /// </summary>
        public event Action onClickAddDeckButton;
        /// <summary>
        /// 当点击保存卡池按钮
        /// </summary>
        public event Action onClickSaveDeckButton;

        #region 卡组列表
        /// <summary>
        /// 当点击右侧卡组列表项，参数为索引
        /// </summary>
        public event Action<int> onClickDeckListItem;
        /// <summary>
        /// 当点击右侧卡组列表项的删除按钮，参数为列表索引
        /// </summary>
        public event Action<int> onClickDeckListItemDeleteButton;
        /// <summary>
        /// 当重命名右侧卡组列表项的名字，参数为列表索引和名字的值
        /// </summary>
        public event Action<int, string> onDeckListItemNameValueChanged;
        #endregion

        #region 编辑卡组
        /// <summary>
        /// 当卡组名称被更改，参数为更改的名称。
        /// </summary>
        public event Action<string> onDeckNameInputFieldValueChanged;

        public event Action<int, PointerEventData> onStartDragDeckCardListItem;
        public event Action<int, PointerEventData> onDragDeckCardListItem;
        /// <summary>
        /// 当拖拽卡组卡牌列表中的项目结束时调用，参数为是否位于卡组卡牌列表区域当中
        /// </summary>
        public event Action<int, PointerEventData> onEndDragDeckCardListItem;
        public event Action<int, PointerEventData> onDeckCardPointerEnter;
        public event Action<int, PointerEventData> onDeckCardPointerExit;
        public event Action onClickEditDeckImageButton;
        #endregion

        #endregion

        #region 属性字段
        public Card dragCard => _dragCard;
        public CardViewDeckCardListItem dragDeckListItem => _dragDeckListItem;
        public DeckCardList deckCardList => _deckCardList;
        public Button addDeckButton => _addDeckButton;
        public DeckViewPanel masterSelectionDeckViewPanel => _masterSelectionDeckViewPanel;
        public Button saveDeckButton => _saveDeckButton;


        [SerializeField]
        Button _returnButton;
        [SerializeField]
        Text _errorTipText;
        [SerializeField]
        GameObject _loadingImgObj;

        [Header("套牌列表")]
        [SerializeField]
        GameObject _deckListObj;
        [SerializeField]
        ElementList _deckList;
        [SerializeField]
        Button _addDeckButton;

        [Header("新建套牌")]
        [SerializeField]
        GameObject _masterSelectionPanel;
        [SerializeField]
        Button _masterSelectionCloseButton;
        [SerializeField]
        DeckViewPanel _masterSelectionDeckViewPanel;

        [Header("编辑套牌")]
        [SerializeField]
        DeckListItem _editDeckHeader;
        [SerializeField]
        Text _characterNameText;
        [SerializeField]
        Text _deckCardCountText;

        [Header("卡组卡牌列表")]
        /// <summary>
        /// 当前能否拖拽卡牌
        /// </summary>
        [SerializeField]
        bool _enableDragDeckCard = true;
        [SerializeField]
        DeckCardList _deckCardList;
        [SerializeField]
        Card _dragCard;
        [SerializeField]
        CardViewDeckCardListItem _dragDeckListItem;
        [SerializeField]
        GameObject _editCardListRightPanel;
        [SerializeField]
        RectTransform _deckListViewportRect;

        [Header("Buttons")]
        [SerializeField]
        Button _exitEditButton;
        [SerializeField]
        Button _exportDeckButton;
        [SerializeField]
        Button _saveDeckButton;
        [SerializeField]
        Button _importDeckButton;
        [SerializeField]
        Button _uploadDeckButton;
        [SerializeField]
        Button _downloadDeckButton;
        #endregion
    }
}