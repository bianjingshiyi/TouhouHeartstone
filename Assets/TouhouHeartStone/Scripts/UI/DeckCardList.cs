using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BJSYGameCore;
using Game;
using TouhouCardEngine;
using TouhouHeartstone;
using UnityEngine;
using UnityEngine.EventSystems;

namespace UI
{
    public class DeckCardList : MonoBehaviour
    {
        #region 公有方法

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
                    listItem.onBeginDrag += onDeckItemBeginDragCallback;
                    listItem.onDrag += onDeckItemDragCallback;
                    listItem.onEndDrag += onDeckItemEndDragCallback;
                    listItem.onPointerEnter += onDeckItemPointerEnterCallback;
                    listItem.onPointerExit += onDeckItemPointerExitCallback;
                },
                rect =>
                {
                    var listItem = rect.GetComponent<CardViewDeckCardListItem>();
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
        #endregion

        #endregion

        #region 事件
        public event Action<int, PointerEventData> onDeckItemBeginDrag;
        public event Action<int, PointerEventData> onDeckItemDrag;
        public event Action<int, PointerEventData> onDeckItemEndDrag;
        public event Action<int, PointerEventData> onDeckItemPointerEnter;
        public event Action<int, PointerEventData> onDeckItemPointerExit;
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
            var cards = deckItems.OrderBy(e => cardManager.getLocalCardDefine(e.cardRef) is CardDefine cardDefine ? cardDefine.getCost() : 0).ToArray();
            var deckCardsViewData = new DeckCardViewData[cards.Length];
            for (int i = 0; i < cards.Length; i++)
            {
                var poolRef = cardManager.getLocalCardPoolReference(cards[i].cardRef.cardPoolId);
                await cardManager.getOrLoadCardPool(poolRef);
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
