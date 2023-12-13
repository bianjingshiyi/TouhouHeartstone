using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Game;
using TouhouCardEngine;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace UI
{
    public class PVECardEditor : MonoBehaviour
    {
        #region 公有方法
        public async Task setCard(CardDefine define, CardManager cardManager)
        {
            await _card.setCardDefine(define, cardManager);
        }
        public void setCardSelected(bool value)
        {
            _cardPageContent.SetActive(value);
            _cardPageNotSelectedNote.SetActive(!value);
        }
        public void updateGemInventory(CardGemViewData[] viewDatas)
        {
            _gemInventoryList.Clear();
            _gemInventoryElementList.updateList(viewDatas.Length,
                (i, rect) =>
                {
                    CardGem gem = rect.GetComponent<CardGem>();
                    var viewData = viewDatas[i];
                    gem.update(viewData);
                    _gemInventoryList.Add(gem);
                },
                rect =>
                {
                    CardGem gem = rect.GetComponent<CardGem>();
                    gem.onBeginDrag += onInventoryGemBeginDragCallback;
                    gem.onDrag += onInventoryGemDragCallback;
                    gem.onEndDrag += onInventoryGemEndDragCallback;
                },
                rect =>
                {
                    CardGem gem = rect.GetComponent<CardGem>();
                    gem.onBeginDrag -= onInventoryGemBeginDragCallback;
                    gem.onDrag -= onInventoryGemDragCallback;
                    gem.onEndDrag -= onInventoryGemEndDragCallback;
                });
        }
        public void updateGemSlot(CardGemSlotGroupViewData[] viewDatas)
        {
            _gemSlotGroupList.Clear();
            _gemSlotGroupElementList.updateList(viewDatas.Length,
                (i, rect) =>
                {
                    CardGemSlotGroup gem = rect.GetComponent<CardGemSlotGroup>();
                    var viewData = viewDatas[i];
                    gem.update(viewData);
                    _gemSlotGroupList.Add(gem);
                },
                rect =>
                {
                    CardGemSlotGroup gem = rect.GetComponent<CardGemSlotGroup>();
                    gem.onSlotBeginDrag += onSlotGemBeginDragCallback;
                    gem.onSlotDrag += onSlotGemDragCallback;
                    gem.onSlotEndDrag += onSlotGemEndDragCallback;
                },
                rect =>
                {
                    CardGemSlotGroup gem = rect.GetComponent<CardGemSlotGroup>();
                    gem.onSlotBeginDrag -= onSlotGemBeginDragCallback;
                    gem.onSlotDrag -= onSlotGemDragCallback;
                    gem.onSlotEndDrag -= onSlotGemEndDragCallback;
                });
        }

        public CardGem getInventoryGem(int index)
        {
            return _gemInventoryList[index];
        }
        public CardGemSlot getSlot(int groupIndex, int slotIndex)
        {
            var group = _gemSlotGroupList[groupIndex];
            return group.getSlot(slotIndex);
        }
        public CardGem getSlotGem(int groupIndex, int slotIndex)
        {
            return getSlot(groupIndex, slotIndex).getGem();
        }
        public DraggingCardGem createDraggingGem(CardGemViewData viewData)
        {
            var gemObj = Instantiate(_draggingGemTemplate, _draggingGemRoot);
            gemObj.SetActive(true);
            var gem = gemObj.GetComponent<DraggingCardGem>();
            gem.gem.update(viewData);
            gem.onDestroy += removeDraggingGem;
            return gem;
        }
        public void resetDragging()
        {
            foreach (var gem in _draggingGems.ToArray())
            {
                gem.destroy();
            }
        }

        public (int groupIndex, int slotIndex) getHoveredGemSlotIndex(PointerEventData pointer)
        {
            List<RaycastResult> results = new List<RaycastResult>();
            EventSystem.current.RaycastAll(pointer, results);
            foreach (var result in results)
            {
                var slot = result.gameObject.GetComponentInParent<CardGemSlot>();
                if (slot)
                {
                    for (int g = 0; g < _gemSlotGroupList.Count; g++)
                    {
                        var group = _gemSlotGroupList[g];
                        var slotIndex = group.indexOf(slot);
                        if (slotIndex >= 0)
                            return (g, slotIndex);
                    }
                }
            }
            return (-1, -1);
        }
        public void updateDeck(DeckCardViewData[] viewDatas)
        {
            _deckCardList.update(viewDatas);
        }
        public void displayCardPage()
        {
            displayPage(_cardPage, _cardPageIndicator);
        }
        public void displayDeckPage()
        {
            displayPage(_deckPage, _deckPageIndicator);
        }
        #endregion

        #region 私有方法

        #region 生命周期
        private void Awake()
        {
            _draggingGemTemplate.SetActive(false);
            _cardFacePageButton.onClick.AddListener(onCardFacePageButtonClicked);
            _deckPageButton.onClick.AddListener(onDeckPageButtonClicked);
            _deckCardList.onDeckItemClicked += onDeckItemClickedCallback;
            _deckCardList.onDeckItemPointerEnter += onDeckItemPointerEnterCallback;
            _deckCardList.onDeckItemPointerExit += onDeckItemPointerExitCallback;
            _closeButton.onClick.AddListener(() => onCloseClicked?.Invoke());
        }
        private void OnEnable()
        {
            resetDragging();
        }
        #endregion
        private void displayPage(GameObject toDisplay, GameObject indicator)
        {
            _cardPage.SetActive(false);
            _deckPage.SetActive(false);

            _cardPageIndicator.SetActive(false);
            _deckPageIndicator.SetActive(false);

            toDisplay.SetActive(true);
            indicator.SetActive(true);
        }
        #region 事件回调

        #region 编辑器页面
        private void onCardFacePageButtonClicked()
        {
            displayCardPage();
        }
        private void onDeckPageButtonClicked()
        {
            displayDeckPage();
        }
        private void onDeckItemClickedCallback(int index, PointerEventData pointer)
        {
            onDeckItemClicked?.Invoke(index, pointer);
        }
        private void onDeckItemPointerEnterCallback(int index, PointerEventData pointer)
        {
            onDeckItemPointerEnter?.Invoke(index, pointer);
        }
        private void onDeckItemPointerExitCallback(int index, PointerEventData pointer)
        {
            onDeckItemPointerExit?.Invoke(index, pointer);
        }
        #endregion

        #region 背包宝石
        private void onInventoryGemBeginDragCallback(CardGem gem, PointerEventData eventData)
        {
            onInventoryGemBeginDrag?.Invoke(_gemInventoryElementList.indexOf(gem.transform as RectTransform), eventData);
        }
        private void onInventoryGemDragCallback(CardGem gem, PointerEventData eventData)
        {
            onInventoryGemDrag?.Invoke(_gemInventoryElementList.indexOf(gem.transform as RectTransform), eventData);
        }
        private void onInventoryGemEndDragCallback(CardGem gem, PointerEventData eventData)
        {
            onInventoryGemEndDrag?.Invoke(_gemInventoryElementList.indexOf(gem.transform as RectTransform), eventData);
        }
        #endregion

        #region 插槽宝石
        private void onSlotGemBeginDragCallback(CardGemSlotGroup gem, int index, PointerEventData eventData)
        {
            onSlotGemBeginDrag?.Invoke(_gemSlotGroupElementList.indexOf(gem.transform as RectTransform), index, eventData);
        }
        private void onSlotGemDragCallback(CardGemSlotGroup gem, int index, PointerEventData eventData)
        {
            onSlotGemDrag?.Invoke(_gemSlotGroupElementList.indexOf(gem.transform as RectTransform), index, eventData);
        }
        private void onSlotGemEndDragCallback(CardGemSlotGroup gem, int index, PointerEventData eventData)
        {
            onSlotGemEndDrag?.Invoke(_gemSlotGroupElementList.indexOf(gem.transform as RectTransform), index, eventData);
        }
        #endregion

        #endregion

        private void removeDraggingGem(DraggingCardGem gem)
        {
            _draggingGems.Remove(gem);
        }

        #endregion

        #region 事件
        public event Action onCloseClicked;
        public event Action<int, PointerEventData> onInventoryGemBeginDrag;
        public event Action<int, PointerEventData> onInventoryGemDrag;
        public event Action<int, PointerEventData> onInventoryGemEndDrag;
        public event Action<int, int, PointerEventData> onSlotGemBeginDrag;
        public event Action<int, int, PointerEventData> onSlotGemDrag;
        public event Action<int, int, PointerEventData> onSlotGemEndDrag;

        public event Action<int, PointerEventData> onDeckItemClicked;
        public event Action<int, PointerEventData> onDeckItemPointerEnter;
        public event Action<int, PointerEventData> onDeckItemPointerExit;
        #endregion

        #region 属性字段
        public DeckCardList deckCardList => _deckCardList;
        private List<CardGem> _gemInventoryList = new List<CardGem>();
        private List<CardGemSlotGroup> _gemSlotGroupList = new List<CardGemSlotGroup>();
        [SerializeField]
        private Card _card;
        [SerializeField]
        private GameObject _cardPageContent;
        [SerializeField]
        private GameObject _cardPageNotSelectedNote;
        [SerializeField]
        private Button _cardFacePageButton;
        [SerializeField]
        private Button _deckPageButton;
        [SerializeField]
        GameObject _cardPage;
        [SerializeField]
        GameObject _deckPage;
        [SerializeField]
        GameObject _cardPageIndicator;
        [SerializeField]
        GameObject _deckPageIndicator;
        [SerializeField]
        private Button _closeButton;
        [SerializeField]
        private ElementList _gemInventoryElementList;
        [SerializeField]
        private ElementList _gemSlotGroupElementList;
        [SerializeField]
        private DeckCardList _deckCardList;
        [SerializeField]
        private GameObject _draggingGemTemplate;
        [SerializeField]
        private Transform _draggingGemRoot;
        [SerializeField]
        private List<DraggingCardGem> _draggingGems = new List<DraggingCardGem>();
        #endregion
    }
}
