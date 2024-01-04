using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BJSYGameCore;
using Game;
using TouhouCardEngine;
using TouhouCardEngine.Shared;
using TouhouHeartstone;
using UnityEngine;
using UnityEngine.EventSystems;

namespace UI
{
    public class DeckCardList : MonoBehaviour
    {
        #region 公有方法
        public void hidePreviewCard()
        {
            setPreviewCardVisible(false);
        }
        public void setPreviewCardVisible(bool value)
        {
            _previewCardTransform.gameObject.SetActive(value);
        }
        public void setPreviewCardPosition(int itemIndex)
        {
            var item = _cardElementList.getElement(itemIndex);
            var pos = _previewCardTransform.position;
            pos.y = item.position.y;
            _previewCardTransform.position = pos;
        }
        public int indexOf(CardViewDeckCardListItem item)
        {
            return _cardElementList.indexOf(item);
        }
        public void update(DeckCardViewData[] viewDatas)
        {
            _cardElementList.updateList(viewDatas.Length,
                (i, item) =>
                {
                    var listItem = item.GetComponent<CardViewDeckCardListItem>();
                    var viewData = viewDatas[i];
                    listItem.updateView(viewData.name, viewData.image, viewData.count);
                },
                rect =>
                {
                    var listItem = rect.GetComponent<CardViewDeckCardListItem>();
                    listItem.onClick += onDeckItemClickedCallback;
                    listItem.onBeginDrag += onDeckItemBeginDragCallback;
                    listItem.onDrag += onDeckItemDragCallback;
                    listItem.onEndDrag += onDeckItemEndDragCallback;
                    listItem.onPointerEnter += onDeckItemPointerEnterCallback;
                    listItem.onPointerExit += onDeckItemPointerExitCallback;
                },
                rect =>
                {
                    var listItem = rect.GetComponent<CardViewDeckCardListItem>();
                    listItem.onClick -= onDeckItemClickedCallback;
                    listItem.onBeginDrag -= onDeckItemBeginDragCallback;
                    listItem.onDrag -= onDeckItemDragCallback;
                    listItem.onEndDrag -= onDeckItemEndDragCallback;
                    listItem.onPointerEnter -= onDeckItemPointerEnterCallback;
                    listItem.onPointerExit -= onDeckItemPointerExitCallback;
                });
        }

        #endregion

        #region 私有方法

        #region 事件回调
        private void onDeckItemBeginDragCallback(CardViewDeckCardListItem item, PointerEventData pointer)
        {
            onDeckItemBeginDrag?.Invoke(_cardElementList.indexOf(item), pointer);
        }
        private void onDeckItemDragCallback(CardViewDeckCardListItem item, PointerEventData pointer)
        {
            onDeckItemDrag?.Invoke(_cardElementList.indexOf(item), pointer);
        }
        private void onDeckItemEndDragCallback(CardViewDeckCardListItem item, PointerEventData pointer)
        {
            onDeckItemEndDrag?.Invoke(_cardElementList.indexOf(item), pointer);
        }
        private void onDeckItemPointerEnterCallback(CardViewDeckCardListItem item, PointerEventData pointer)
        {
            onDeckItemPointerEnter?.Invoke(_cardElementList.indexOf(item), pointer);
        }
        private void onDeckItemPointerExitCallback(CardViewDeckCardListItem item, PointerEventData pointer)
        {
            onDeckItemPointerExit?.Invoke(_cardElementList.indexOf(item), pointer);
        }
        private void onDeckItemClickedCallback(CardViewDeckCardListItem item, PointerEventData pointer)
        {
            onDeckItemClicked?.Invoke(_cardElementList.indexOf(item), pointer);
        }
        #endregion

        #endregion

        #region 事件
        public event Action<int, PointerEventData> onDeckItemBeginDrag;
        public event Action<int, PointerEventData> onDeckItemDrag;
        public event Action<int, PointerEventData> onDeckItemEndDrag;
        public event Action<int, PointerEventData> onDeckItemPointerEnter;
        public event Action<int, PointerEventData> onDeckItemPointerExit;
        public event Action<int, PointerEventData> onDeckItemClicked;
        #endregion

        #region 属性字段
        public Card previewCard => _previewCard;
        [SerializeField]
        Transform _previewCardTransform;
        [SerializeField]
        Card _previewCard;
        [SerializeField]
        ElementList _cardElementList;
        #endregion
    }
    public static class DeckCardListHelper
    {
        public static async Task showPreviewCardByLocalPool(this DeckCardList deckView, Deck deck, int index, CardManager cardManager)
        {
            if (deck == null)
                return;
            var deckItems = await cardManager.orderDeckItemsByLocalPools(deck.cardList);
            var deckItem = deckItems[index];
            var defRef = deckItem.cardRef;
            if (defRef == null)
                return;
            var poolRef = cardManager.getLocalCardPoolReference(defRef.cardPoolId);
            var cardDef = cardManager.getCardDefine(poolRef, defRef.defineId);
            await deckView.showPreviewCard(cardDef, index, cardManager);
        }
        public static async Task showPreviewCardByPoolSet(this DeckCardList deckView, Deck deck, int index, CardManager cardManager, IEnumerable<ICardPoolReference> poolSet)
        {
            if (deck == null)
                return;
            var deckItems = await cardManager.orderDeckItemsByPoolsInSet(deck.cardList, poolSet);
            var deckItem = deckItems[index];
            var defRef = deckItem.cardRef;
            if (defRef == null)
                return;
            var poolRef = cardManager.getCardPoolReferenceInEnv(defRef.cardPoolId, poolSet);
            var cardDef = cardManager.getCardDefine(poolRef, defRef.defineId);
            await deckView.showPreviewCard(cardDef, index, cardManager);
        }
        public static async Task showPreviewCard(this DeckCardList deckView, CardDefine define, int index, CardManager cardManager)
        {
            deckView.setPreviewCardVisible(true);
            deckView.setPreviewCardPosition(index);
            if (define == null)
            {
                deckView.previewCard.setUnknownCard(true);
            }
            else
            {
                await deckView.previewCard.setCardDefine(define, cardManager, true);
            }
        }
    }
    public struct DeckCardViewData
    {
        public string name;
        public Sprite image;
        public int count;

        public static Task<DeckCardViewData[]> fromDeck(Deck deck, CardManager cardManager, ILanguageManager langManager)
        {
            if (deck == null)
                return Task.FromResult(Array.Empty<DeckCardViewData>());
            return fromDeckItems(deck.cardList, cardManager, langManager);
        }
        public static async Task<DeckCardViewData[]> fromDeckItems(IEnumerable<DeckItem> deckItems, CardManager cardManager, ILanguageManager langManager)
        {
            if (deckItems == null)
                return Array.Empty<DeckCardViewData>();
            var cards = await cardManager.orderDeckItemsByLocalPools(deckItems);
            var deckCardsViewData = new DeckCardViewData[cards.Length];
            for (int i = 0; i < cards.Length; i++)
            {
                var poolRef = cardManager.getLocalCardPoolReference(cards[i].cardRef.cardPoolId);
                CardDefine cardDefine = cardManager.getCardDefine(poolRef, cards[i].cardRef.defineId);
                CardSkinData skin = cardDefine != null ? cardManager.getCardSkin(cardDefine) : null;

                string name;
                Sprite image;
                if (skin != null)
                {
                    name = skin.name;
                    image = await cardManager.loadCardImage(cardDefine);
                }
                else
                {
                    name = langManager.getString(StringTable.BUILD_UNKNOW_CARD_NAME);
                    image = cardManager.getDefaultSprite();
                }
                deckCardsViewData[i].name = name;
                deckCardsViewData[i].image = image;
                deckCardsViewData[i].count = cards[i].count;
            }
            return deckCardsViewData;
        }
        public static async Task<DeckCardViewData[]> fromCardDefines(CardDefine[] cardDefines, CardManager cardManager, ILanguageManager langManager)
        {
            if (cardDefines == null)
                return Array.Empty<DeckCardViewData>();
            var viewData = new DeckCardViewData[cardDefines.Length];
            for (int i = 0; i < viewData.Length; i++)
            {
                CardDefine cardDefine = cardDefines[i];
                CardSkinData skin = cardDefine != null ? cardManager.getCardSkin(cardDefine) : null;

                string name;
                Sprite image;
                if (skin != null)
                {
                    name = skin.name;
                    image = await cardManager.loadCardImage(cardDefine);
                }
                else
                {
                    name = langManager.getString(StringTable.BUILD_UNKNOW_CARD_NAME);
                    image = cardManager.getDefaultSprite();
                }
                viewData[i].name = name;
                viewData[i].image = image;
                viewData[i].count = 0;
            }
            return viewData;
        }
    }
}
