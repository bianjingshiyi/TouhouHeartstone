using System.Collections.Generic;
using UnityEngine;
using System;
using BJSYGameCore.UI;
using UnityEngine.UI;
namespace UI
{
    /// <summary>
    /// 所有需要浏览卡牌收藏的界面的父类
    /// </summary>
    class CardViewPanel : MonoBehaviour
    {
        #region 公有方法
        public void setCardPoolOptions(string[] cardPoolNames)
        {
            _cardPoolViewPanel.setCardPoolOptions(cardPoolNames);
        }
        public void setCardPoolValue(bool[] value)
        {
            _cardPoolViewPanel.setCardPoolValues(value);
        }
        public void setSortTypes(string[] sortTypes)
        {
            _cardPoolViewPanel.setSortTypes(sortTypes);
        }
        public void setSortType(int index)
        {
            _cardPoolViewPanel.setSortType(index);
        }
        public void setCharacterFilters(string[] characterFilters)
        {
            _cardPoolViewPanel.setCharacterFilters(characterFilters);
        }
        public void setCharacterFilter(bool[] value)
        {
            _cardPoolViewPanel.setCharacterFilter(value);
        }
        public void setCostFilters(string[] costFilters)
        {
            _cardPoolViewPanel.setCostFilters(costFilters);
        }
        public void setCostFilter(bool[] value)
        {
            _cardPoolViewPanel.setCostFilter(value);
        }
        public void setTypeFilters(string[] typeFilters)
        {
            _cardPoolViewPanel.setTypeFilters(typeFilters);
        }
        public void setTypeFilter(bool[] value)
        {
            _cardPoolViewPanel.setTypeFilter(value);
        }
        /// <summary>
        /// 显示左边的卡牌总览
        /// </summary>
        /// <param name="value"></param>
        public void setCardListActive(bool value)
        {
            _cardPoolViewPanel.setCardListActive(value);
        }
        /// <summary>
        /// 显示右边的卡组列表
        /// </summary>
        /// <param name="value"></param>
        public void setDeckListActive(bool value)
        {
            _deckList.SetActive(value);
        }
        /// <summary>
        /// 更新自定义卡池卡牌编辑栏卡牌列表
        /// </summary>
        public void updateDeckCardList((string name, Sprite image, int count)[] viewData)
        {
            UIHelper.updateList(_cardPoolCardListRoot, _cardPoolCardListItemTemplate, _cardPoolCardItemList, viewData.Length,
                (i, item) =>
                {
                    item.GetComponent<CardViewDeckCardListItem>().updateView(viewData[i].name, viewData[i].image, viewData[i].count);
                }, onDeckCardListItemEnable, onDeckCardListItemDisable, dontDestroy: true);
        }
        /// <summary>
        /// 设置卡牌列表数量（会引发卡牌列表刷新事件）
        /// </summary>
        /// <param name="count"></param>
        public void updateCardListCount(int count)
        {
            _cardPoolViewPanel.updateCardListCount(count);
        }
        public int getCardItemIndex(RectTransform rect)
        {
            return _cardPoolViewPanel.getCardItemIndex(rect);
        }
        /// <summary>
        /// 更新自定义卡池列表
        /// </summary>
        /// <param name="deckViewData"></param>
        public void updateDeckList((string name, Sprite image, bool errorTip)[] deckViewData)
        {
            UIHelper.updateList(_deckListRoot, _deckListItemTemplate, _deckItemList, deckViewData.Length, (i, item) =>
            {
                DeckListItem deckListItem = item.GetComponentInChildren<DeckListItem>();
                deckListItem.updateView(deckViewData[i].name, deckViewData[i].image);
                deckListItem.setWarning(deckViewData[i].errorTip);
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
        public void updateDeckList((string name, Sprite image)[] deckViewData)
        {
            (string name, Sprite image, bool errorTip)[] deckViewData3 = new (string name, Sprite image, bool errorTip)[deckViewData.Length];
            for (int i = 0; i < deckViewData3.Length; i++)
            {
                deckViewData3[i].name = deckViewData[i].name;
                deckViewData3[i].image = deckViewData[i].image;
            }
            updateDeckList(deckViewData3);
        }
        public void setEditDeckPanelActive(bool value)
        {
            _cardPoolEditPanel.SetActive(value);
        }
        public void updateCurCardPoolInfo(string name, Sprite image)
        {
            _curCardPoolNameText.text = name;
            _curCardPoolImage.sprite = image;
            _deckNameInputField.SetTextWithoutNotify(name);
        }
        #endregion
        #region 私有方法
        #region 生命周期
        protected virtual void Awake()
        {
            _returnButton.onClick.AddListener(onClickReturnButtonCallback);
            if (_deckListItemTemplate.transform.parent == _deckListRoot)
            {
                _deckListItemTemplate.gameObject.SetActive(false);
            }
            _addCardPoolButton.onClick.AddListener(onClickAddCardPoolButtonCallback);
            _saveCardPoolButton.onClick.AddListener(onClickSaveCardPoolButtonCallback);
            _returnToCardPoolViewButton.onClick.AddListener(onClickReturnToCardPoolViewButtonCallback);
            _deleteCurCardPoolButton.onClick.AddListener(onClickDeleteCurCardPoolButtonCallback);
            if (_cardPoolCardListItemTemplate.transform.parent == _cardPoolCardListRoot)
            {
                _cardPoolCardListItemTemplate.gameObject.SetActive(false);
            }
            _editDeckNameButton.onClick.AddListener(onClickEditDeckNameButtonCallback);
            _deckNameInputField.onEndEdit.AddListener(onEndEditDeckNameInputFieldCallback);
            _cardPoolViewPanel.onEnableCardListItem += onEnableCardListItem;
            _cardPoolViewPanel.onDisableCardListItem += onDisableCardListItem;
            //_deckList.GetComponent<DeckViewPanel>()?.showAddButton();
        }
        #endregion
        #region 事件回调
        private void onClickReturnButtonCallback()
        {
            onClickReturnButton?.Invoke();
        }
        protected virtual void onEnableCardListItem(int index, RectTransform itemRect)
        {
            EditorCardListItem editorCardListItem = itemRect.GetComponent<EditorCardListItem>();
            editorCardListItem.onClick += onClickCardListItemCallback;
        }
        protected virtual void onDisableCardListItem(int index, RectTransform itemRect)
        {
            EditorCardListItem editorCardListItem = itemRect.GetComponent<EditorCardListItem>();
            editorCardListItem.onClick -= onClickCardListItemCallback;
        }
        private void onClickCardListItemCallback(EditorCardListItem item)
        {
            int index = _cardPoolViewPanel.getCardItemIndex(item.transform as RectTransform);
            onClickCardListItem?.Invoke(index);
        }
        private void onClickDeckListItemCallback(DeckListItem item)
        {
            onClickDeckListItem?.Invoke(_deckItemList.IndexOf(item.transform as RectTransform));
        }
        private void onClickDeckListItemDeleteButtonCallback(DeckListItem item)
        {
            onClickDeckListItemDeleteButton?.Invoke(_deckItemList.IndexOf(item.transform as RectTransform));
        }
        private void onDeckListItemNameInputFieldValueChanged(DeckListItem item, string value)
        {
            onDeckListItemNameValueChanged?.Invoke(_deckItemList.IndexOf(item.transform as RectTransform), value);
        }
        protected virtual void onDeckCardListItemEnable(RectTransform itemRect)
        {
        }
        protected virtual void onDeckCardListItemDisable(RectTransform itemRect)
        {
        }
        private void onClickAddCardPoolButtonCallback()
        {
            onClickDeckListAddButton?.Invoke();
        }
        private void onClickSaveCardPoolButtonCallback()
        {
            onClickSaveDeckButton?.Invoke();
        }
        private void onClickReturnToCardPoolViewButtonCallback()
        {
            onClickReturnToCardPoolViewButton?.Invoke();
        }
        private void onClickDeleteCurCardPoolButtonCallback()
        {
            onClickDeleteCurCardPoolButton?.Invoke();
        }
        private void onClickEditDeckNameButtonCallback()
        {
            //编辑名称模式
            _deckNameInputField.gameObject.SetActive(true);
            _curCardPoolNameText.gameObject.SetActive(false);
        }
        private void onEndEditDeckNameInputFieldCallback(string value)
        {
            _deckNameInputField.gameObject.SetActive(false);
            _curCardPoolNameText.gameObject.SetActive(true);
            _curCardPoolNameText.text = value;
            onDeckNameInputFieldValueChanged?.Invoke(value);
        }
        #endregion
        #endregion
        #region 事件
        /// <summary>
        /// 当点击返回按钮
        /// </summary>
        public event Action onClickReturnButton;
        /// <summary>
        /// 当更新卡牌列表，参数为更新的列表项索引和UI物体
        /// </summary>
        public event Action<int, RectTransform> onUpdateCardListItem
        {
            add { _cardPoolViewPanel.onUpdateCardListItem += value; }
            remove { _cardPoolViewPanel.onUpdateCardListItem -= value; }
        }
        /// <summary>
        /// 当点击卡牌列表，参数为点击的列表项索引
        /// </summary>
        public event Action<int> onClickCardListItem;
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
        /// <summary>
        /// 当点击右侧卡组列表添加按钮
        /// </summary>
        public event Action onClickDeckListAddButton;
        /// <summary>
        /// 当点击保存卡池按钮
        /// </summary>
        public event Action onClickSaveDeckButton;
        /// <summary>
        /// 当点击返回浏览卡池按钮
        /// </summary>
        public event Action onClickReturnToCardPoolViewButton;
        /// <summary>
        /// 当点击删除当前卡池按钮
        /// </summary>
        public event Action onClickDeleteCurCardPoolButton;
        /// <summary>
        /// 当更改卡池过滤选项，参数为更改后的卡池选择状态
        /// </summary>
        public event Action<bool[]> onCardPoolValuesChanged
        {
            add { _cardPoolViewPanel.onCardPoolsValueChanged += value; }
            remove { _cardPoolViewPanel.onCardPoolsValueChanged -= value; }
        }
        /// <summary>
        /// 当更改排序类型，参数为更改的索引
        /// </summary>
        public event Action<int> onChangeSortTypeValue;
        /// <summary>
        /// 当改变角色过滤器的值，参数为更改的索引
        /// </summary>
        public event Action<int> onChangeCharacterFilterValue;
        /// <summary>
        /// 当牌库名称被更改，参数为更改的名称。
        /// </summary>
        public event Action<string> onDeckNameInputFieldValueChanged;
        #endregion
        #region 属性字段
        public CardPoolViewPanel cardPoolViewPanel => _cardPoolViewPanel;
        public List<RectTransform> deckCardItemList => _cardPoolCardItemList;
        [SerializeField]
        Button _returnButton;
        [SerializeField]
        RectTransform _cardPoolCardListRoot;
        [SerializeField]
        RectTransform _cardPoolCardListItemTemplate;
        [SerializeField]
        List<RectTransform> _cardPoolCardItemList = new List<RectTransform>();
        [SerializeField]
        CardPoolViewPanel _cardPoolViewPanel;
        [SerializeField]
        GameObject _deckList;
        [SerializeField]
        RectTransform _deckListRoot;
        [SerializeField]
        RectTransform _deckListItemTemplate;
        [SerializeField]
        List<RectTransform> _deckItemList = new List<RectTransform>();
        [SerializeField]
        Button _addCardPoolButton;
        [SerializeField]
        GameObject _cardPoolEditPanel;
        [SerializeField]
        Button _saveCardPoolButton;
        [SerializeField]
        Button _returnToCardPoolViewButton;
        [SerializeField]
        Button _deleteCurCardPoolButton;
        [SerializeField]
        Text _curCardPoolNameText;
        [SerializeField]
        Image _curCardPoolImage;
        [SerializeField]
        Button _editDeckNameButton;
        [SerializeField]
        InputField _deckNameInputField;
        #endregion
    }
}