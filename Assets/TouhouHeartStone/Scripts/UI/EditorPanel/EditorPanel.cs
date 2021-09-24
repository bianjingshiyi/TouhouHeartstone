using System.Collections.Generic;
using UnityEngine;
using Game;
using BJSYGameCore;
using System.IO;
using System;
using BJSYGameCore.UI;
using UnityEngine.UI;

namespace UI
{

    partial class EditorPanel
    {
        #region 公有方法
        /// <summary>
        /// 切换右侧栏到显示所有卡池
        /// </summary>
        public void switchRightPanelToView()
        {
            _deckList.SetActive(true);
            _cardList.SetActive(false);
            _cardPoolEditPanel.SetActive(false);
        }
        /// <summary>
        /// 切换右侧栏到显示编辑卡池
        /// </summary>
        public void switchRightPanelToEdit()
        {
            _deckList.SetActive(false);
            _cardList.SetActive(true);
            _cardPoolEditPanel.SetActive(true);
        }
        /// <summary>
        /// 更新卡池列表
        /// </summary>
        /// <param name="cardPoolInfos"></param>
        public void updateDeckList(CardPoolInfo[] cardPoolInfos)
        {
            BJSYGameCore.UI.UIHelper.updateList(_deckListRoot, _deckListItemTemplate, _deckItemList, cardPoolInfos.Length, (i, item) =>
            {
                DeckListItem deckListItem = item.GetComponentInChildren<DeckListItem>();
                deckListItem.Text.text = cardPoolInfos[i].cardPoolName;
            }, item =>
            {
                DeckListItem deckListItem = item.GetComponent<DeckListItem>();
                deckListItem.onClick += onClickDeckListItem;
                deckListItem.onClickDeleteButton += onClickDeckListItemDeleteButton;
            }, item =>
            {
                DeckListItem deckListItem = item.GetComponent<DeckListItem>();
                deckListItem.onClick -= onClickDeckListItem;
                deckListItem.onClickDeleteButton -= onClickDeckListItemDeleteButton;
            });
        }
        public void updateCurCardPoolInfo(string name, Sprite image)
        {
            _curCardPoolNameText.text = name;
            _curCardPoolImage.sprite = image;
        }
        /// <summary>
        /// 更新卡牌编辑栏卡牌列表
        /// </summary>
        public void updateCardPoolCardList(string[] cardNames, Sprite[] cardSprites)
        {
            BJSYGameCore.UI.UIHelper.updateList(_cardPoolCardListRoot, _cardPoolCardListItemTemplate, _cardPoolCardItemList, cardNames.Length,
                (i, item) =>
                {
                    item.GetComponent<EditorCardPoolListItem>().updateView(cardNames[i], cardSprites[i]);
                });
        }
        /// <summary>
        /// 设置卡牌列表数量（会引发卡牌列表刷新事件）
        /// </summary>
        /// <param name="count"></param>
        public void updateCardListCount(int count)
        {
            _cardListLayoutGroup.setCount(count);
        }
        /// <summary>
        /// 设置排序类型选项
        /// </summary>
        /// <param name="sortTypes"></param>
        public void setSortTypes(string[] sortTypes)
        {
        }
        /// <summary>
        /// 设置排序类型的值
        /// </summary>
        /// <param name="index"></param>
        public void setSortType(int index)
        {
        }
        /// <summary>
        /// 设置角色过滤器的选项
        /// </summary>
        /// <param name="filterNames"></param>
        public void setCharacterFilters(string[] filterNames)
        {
        }
        /// <summary>
        /// 设置角色过滤器的值
        /// </summary>
        /// <param name="index"></param>
        public void setCharaterFilter(int index)
        {
        }
        #endregion
        #region 私有方法
        protected override void Awake()
        {
            base.Awake();
            _returnButton.onClick.AddListener(onClickReturnButtonCallback);
            _addCardPoolButton.onClick.AddListener(onClickAddCardPoolButtonCallback);
            _saveCardPoolButton.onClick.AddListener(onClickSaveCardPoolButtonCallback);
            _returnToCardPoolViewButton.onClick.AddListener(onClickReturnToCardPoolViewButtonCallback);
            _deleteCurCardPoolButton.onClick.AddListener(onClickDeleteCurCardPoolButtonCallback);
            if (_cardListLayoutGroup.cellPrefab.transform.parent == _cardListLayoutGroup.transform)
            {
                _cardListLayoutGroup.cellPrefab.gameObject.SetActive(false);
            }
            _cardListLayoutGroup.onEnableItem += onEnableCardListItem;
            _cardListLayoutGroup.onDisableItem += onDisableCardListItem;
        }
        #region 事件回调
        void onClickReturnButtonCallback()
        {
            onClickReturnButton?.Invoke();
        }
        void onClickDeckListItem(DeckListItem item)
        {
            onClickCardPoolListItem?.Invoke(_deckItemList.IndexOf(item.transform as RectTransform));
        }
        void onClickDeckListItemDeleteButton(DeckListItem item)
        {
            onClickDeleteCardPoolButton?.Invoke(_deckItemList.IndexOf(item.transform as RectTransform));
        }
        void onClickAddCardPoolButtonCallback()
        {
            onClickAddCardPoolButton?.Invoke();
        }
        void onClickSaveCardPoolButtonCallback()
        {
            onClickSaveCardPoolButton?.Invoke();
        }
        void onClickReturnToCardPoolViewButtonCallback()
        {
            onClickReturnToCardPoolViewButton?.Invoke();
        }
        void onClickDeleteCurCardPoolButtonCallback()
        {
            onClickDeleteCurCardPoolButton?.Invoke();
        }
        private void onEnableCardListItem(int index, RectTransform item)
        {
            EditorCardListItem editorCardListItem = item.GetComponent<EditorCardListItem>();
            editorCardListItem.onClick += onClickCardListItemCallback;
        }
        private void onDisableCardListItem(int index, RectTransform item)
        {
            EditorCardListItem editorCardListItem = item.GetComponent<EditorCardListItem>();
            editorCardListItem.onClick -= onClickCardListItemCallback;
        }
        private void onClickCardListItemCallback(EditorCardListItem item)
        {
            int index = _cardListLayoutGroup.getItemIndex(item.transform as RectTransform);
            onClickCardListItem?.Invoke(index);
        }
        #endregion
        #endregion
        public event Action onClickReturnButton;
        /// <summary>
        /// 当点击卡池列表项，参数为列表项索引
        /// </summary>
        public event Action<int> onClickCardPoolListItem;
        /// <summary>
        /// 当点击删除卡池按钮，参数为点击删除的列表项索引
        /// </summary>
        public event Action<int> onClickDeleteCardPoolButton;
        /// <summary>
        /// 当点击添加卡池按钮
        /// </summary>
        public event Action onClickAddCardPoolButton;
        /// <summary>
        /// 当更新卡牌列表，参数为更新的列表项索引和UI物体
        /// </summary>
        public event Action<int, RectTransform> onUpdateCardListItem
        {
            add { _cardListLayoutGroup.onUpdateItem += value; }
            remove { _cardListLayoutGroup.onUpdateItem -= value; }
        }
        /// <summary>
        /// 当点击保存卡池按钮
        /// </summary>
        public event Action onClickSaveCardPoolButton;
        /// <summary>
        /// 当点击返回浏览卡池按钮
        /// </summary>
        public event Action onClickReturnToCardPoolViewButton;
        /// <summary>
        /// 当点击删除当前卡池按钮
        /// </summary>
        public event Action onClickDeleteCurCardPoolButton;
        /// <summary>
        /// 当点击卡牌列表，参数为点击的列表项索引
        /// </summary>
        public event Action<int> onClickCardListItem;
        /// <summary>
        /// 当更改排序类型，参数为更改的索引
        /// </summary>
        public event Action<int> onChangeSortTypeValue;
        /// <summary>
        /// 当改变角色过滤器的值，参数为更改的索引
        /// </summary>
        public event Action<int> onChangeCharacterFilterValue;
        [SerializeField]
        Button _returnButton;
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
        RectTransform _cardPoolCardListRoot;
        [SerializeField]
        RectTransform _cardPoolCardListItemTemplate;
        [SerializeField]
        List<RectTransform> _cardPoolCardItemList = new List<RectTransform>();
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
        GameObject _cardList;
        [SerializeField]
        VirtualListLayoutGroup _cardListLayoutGroup;
    }
    [Serializable]
    public class CardPoolInfo
    {
        public int cardPoolId;
        public string cardPoolName;
        public string imageResPath;
    }
}