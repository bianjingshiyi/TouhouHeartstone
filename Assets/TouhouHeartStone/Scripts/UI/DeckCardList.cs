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
using UIHelper = BJSYGameCore.UI.UIHelper;

namespace UI
{
    public class DeckCardList : MonoBehaviour
    {
        public void update(DeckCardViewData[] viewDatas)
        {
            UIHelper.updateList(_deckCardListRoot, _deckCardItemTemplate, _deckCardList, viewDatas.Length,
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
                });
        }
        private void onDeckItemClickedCallback(CardViewDeckCardListItem item, PointerEventData pointer)
        {
            onDeckItemClicked?.Invoke(_deckCardList.IndexOf(item.transform as RectTransform), pointer);
        }
        public event Action<int, PointerEventData> onDeckItemClicked;
        [SerializeField]
        RectTransform _deckCardListRoot;
        [SerializeField]
        RectTransform _deckCardItemTemplate;
        [SerializeField]
        List<RectTransform> _deckCardList = new List<RectTransform>();
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
