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
using UnityEngine.Pool;

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

            // 防止卡牌预览超出屏幕。
            avoidOutsideScreen();
        }
        public CardViewDeckCardListItem getListItem(int itemIndex)
        {
            return _cardElementList.getElement<CardViewDeckCardListItem>(itemIndex);
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
                    listItem.onDeselect += onDeckCardDeselectCallback;
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
                    listItem.onDeselect -= onDeckCardDeselectCallback;
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
        private void onDeckCardDeselectCallback(CardViewDeckCardListItem item)
        {
            onDeckItemDeselect?.Invoke(_cardElementList.indexOf(item));
        }
        #endregion

        private void avoidOutsideScreen()
        {
            var rootCanvas = getRootCanvas();
            var worldCorners = new Vector3[4];
            _previewCardTransform.GetWorldCorners(worldCorners);

            RectTransform rootCanvasRectTransform = rootCanvas.transform as RectTransform;
            Rect rootCanvasRect = rootCanvasRectTransform.rect;

            var localRectMin = rootCanvasRectTransform.InverseTransformPoint(worldCorners[0]);
            var localRectMax = rootCanvasRectTransform.InverseTransformPoint(worldCorners[2]);
            for (int axis = 0; axis < 2; axis++)
            {
                bool outside = false;
                float outsideDistance = 0;
                if (localRectMin[axis] < rootCanvasRect.min[axis] && !Mathf.Approximately(localRectMin[axis], rootCanvasRect.min[axis]))
                {
                    outside = true;
                    outsideDistance = localRectMin[axis] - rootCanvasRect.min[axis];
                }
                else if (localRectMax[axis] > rootCanvasRect.max[axis] && !Mathf.Approximately(localRectMax[axis], rootCanvasRect.max[axis]))
                {
                    outside = true;
                    outsideDistance = localRectMax[axis] - rootCanvasRect.max[axis];
                }
                if (outside)
                {
                    if (axis == 0)
                        // 水平超出屏幕。
                        RectTransformUtility.FlipLayoutOnAxis(_previewCardTransform, axis, false, false);
                    else
                    {
                        // 垂直超出屏幕。
                        var pos = _previewCardTransform.localPosition;
                        pos.y -= outsideDistance;
                        _previewCardTransform.localPosition = pos;
                    }
                }
            }
        }
        private Canvas getRootCanvas()
        {
            var canvases = gameObject.GetComponentsInParent<Canvas>(false);
            if (canvases.Length == 0)
                return null;

            // 根Canvas为GetComponentsInParent()返回的最后一个元素。
            Canvas rootCanvas = canvases[canvases.Length - 1];
            for (int i = 0; i < canvases.Length; i++)
            {
                if (canvases[i].isRootCanvas)
                {
                    rootCanvas = canvases[i];
                    break;
                }
            }
            return rootCanvas;
        }

        #endregion

        #region 事件
        public event Action<int, PointerEventData> onDeckItemBeginDrag;
        public event Action<int, PointerEventData> onDeckItemDrag;
        public event Action<int, PointerEventData> onDeckItemEndDrag;
        public event Action<int, PointerEventData> onDeckItemPointerEnter;
        public event Action<int, PointerEventData> onDeckItemPointerExit;
        public event Action<int, PointerEventData> onDeckItemClicked;
        public event Action<int> onDeckItemDeselect;
        #endregion

        #region 属性字段
        public Card previewCard => _previewCard;
        [SerializeField]
        RectTransform _previewCardTransform;
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

        public static Task<DeckCardViewData[]> fromDeck(Deck deck, MainManager main, IEnumerable<ICardPoolReference> env = null)
        {
            if (deck == null)
                return Task.FromResult(Array.Empty<DeckCardViewData>());
            return fromDeckItems(deck.cardList, main, env);
        }
        public static async Task<DeckCardViewData[]> fromDeckItems(IEnumerable<DeckItem> deckItems, MainManager main, IEnumerable<ICardPoolReference> env = null)
        {
            if (deckItems == null)
                return Array.Empty<DeckCardViewData>();
            var cardManager = main.card;
            var langManager = main.langManager;
            var cards = await cardManager.orderDeckItemsByLocalPools(deckItems);
            var deckCardsViewData = new DeckCardViewData[cards.Length];
            for (int i = 0; i < cards.Length; i++)
            {
                var cardPoolId = cards[i].cardRef.cardPoolId;
                var poolRef = env == null ? cardManager.getLocalCardPoolReference(cardPoolId) : cardManager.getCardPoolReferenceInEnv(cardPoolId, env);
                CardDefine cardDefine = cardManager.getCardDefine(poolRef, cards[i].cardRef.defineId);
                CardSkinData skin = cardDefine != null ? cardManager.getCardSkin(cardDefine) : null;

                string name;
                Sprite image;
                if (skin != null)
                {
                    name = cardManager.getCardName(poolRef, skin);
                    image = await cardManager.loadCardImage(cardDefine);
                }
                else
                {
                    name = langManager.getString(StringTable.BUILD_UNKNOWN_CARD_NAME);
                    image = cardManager.getDefaultSprite();
                }
                deckCardsViewData[i].name = name;
                deckCardsViewData[i].image = image;
                deckCardsViewData[i].count = cards[i].count;
            }
            return deckCardsViewData;
        }
        public static async Task<DeckCardViewData[]> fromCardDefines(CardDefine[] cardDefines, MainManager main)
        {
            if (cardDefines == null)
                return Array.Empty<DeckCardViewData>();
            var cardManager = main.card;
            var langManager = main.langManager;
            var viewData = new DeckCardViewData[cardDefines.Length];
            for (int i = 0; i < viewData.Length; i++)
            {
                CardDefine cardDefine = cardDefines[i];
                CardSkinData skin = cardDefine != null ? cardManager.getCardSkin(cardDefine) : null;

                string name;
                Sprite image;
                if (skin != null)
                {
                    var poolRef = cardManager.getCardPoolReferenceOfCard(cardDefine);
                    name = cardManager.getCardName(poolRef, skin);
                    image = await cardManager.loadCardImage(cardDefine);
                }
                else
                {
                    name = langManager.getString(StringTable.BUILD_UNKNOWN_CARD_NAME);
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
