using System.Collections;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using TouhouCardEngine;
using TouhouHeartstone;
using System.Reflection;
using Game;
namespace UI
{
    partial class BuildCardListItem : IBeginDragHandler, IDragHandler, IEndDragHandler
    {
        [SerializeField]
        Vector2 _dragStartPosition;
        [SerializeField]
        float _dragThreshold = 50f;
        public CardDefine card { get; private set; }
        public void update(CardDefine card, CardSkinData skin)
        {
            this.card = card;

            Card.update(card, skin);
        }
        void IBeginDragHandler.OnBeginDrag(PointerEventData eventData)
        {
            _dragStartPosition = eventData.position;
        }
        void IDragHandler.OnDrag(PointerEventData eventData)
        {
            Build build = GetComponentInParent<Build>();

            if (!build.DragCard.isDisplaying)
            {
                if (Vector2.Distance(_dragStartPosition, eventData.position) > _dragThreshold)
                {
                    //开始拖拽
                    build.startDrag(card, eventData.position);
                }
            }
        }
        void IEndDragHandler.OnEndDrag(PointerEventData eventData)
        {
            ////结束拖拽
            //Build build = GetComponentInParent<Build>();
            //build.stopDrag(eventData.position);
        }
    }
    partial class BuildDeckListItem : IBeginDragHandler, IDragHandler, IEndDragHandler
    {
        public void update(CardDefine card, CardSkinData skin, int count)
        {
            this.card = card;
            this.count = count;

            Image.sprite = skin.image;
            Text.text = skin.name;
        }
        [SerializeField]
        Vector2 _dragStartPosition;
        [SerializeField]
        float _dragThreshold = 50f;
        public CardDefine card { get; private set; }
        [SerializeField]
        int _count;
        public int count
        {
            get { return _count; }
            set
            {
                _count = value;
                NumText.text = _count > 1 ? _count.ToString() : null;
            }
        }
        void IBeginDragHandler.OnBeginDrag(PointerEventData eventData)
        {
            _dragStartPosition = eventData.position;
        }
        void IDragHandler.OnDrag(PointerEventData eventData)
        {
            Build build = GetComponentInParent<Build>();

            if (!build.DragCard.isDisplaying)
            {
                if (Vector2.Distance(_dragStartPosition, eventData.position) > _dragThreshold)
                {
                    //开始拖拽
                    build.startDrag(card, eventData.position);
                    count--;
                    //从列表中移除
                    if (count < 1)
                        GetComponentInParent<BuildDeckList>().removeItem(this);
                }
            }
        }
        void IEndDragHandler.OnEndDrag(PointerEventData eventData)
        {
            ////结束拖拽
            //Build build = GetComponentInParent<Build>();
            //build.stopDrag(eventData.position);
            //gameObject.SetActive(true);
        }
    }
    partial class Build
    {
        partial void onAwake()
        {
            ReturnButton.onClick.AddListener(() =>
            {
                PlayerPrefs.SetInt("DeckCount", BuildDeckList.Select(i => i.count).Sum() + 1);
                int index = 1;
                foreach (var item in BuildDeckList.getItems())
                {
                    for (int i = 0; i < item.count; i++)
                    {
                        PlayerPrefs.SetInt("Deck" + index, item.card.id);
                        parent.game.deck[index] = item.card.id;
                        index++;
                    }
                }
                PlayerPrefs.Save();
                parent.display(parent.MainMenu);
            });
        }
        CardDefine[] cards { get; set; } = null;
        protected override void onDisplay()
        {
            base.onDisplay();
            //目前只有一个卡组
            cards = parent.game.getManager<CardManager>().GetCardDefines(d => d.GetType().Assembly == typeof(THHGame).Assembly);
            int[] deck = parent.game.deck;
            CardSkinData masterSkin = parent.game.cards.GetCardSkin(deck[0]);
            MasterButton.setSprite(masterSkin.image);
            MasterNameText.text = masterSkin.name;
            BuildDeckList.clearItems();
            foreach (var cards in deck.Skip(1).Select(id => cards.First(c => c.id == id)).GroupBy(c => c.id))
            {
                var item = BuildDeckList.addItem();
                var skin = parent.game.cards.GetCardSkin(cards.First().id);
                item.update(cards.First(), skin, cards.Count());
            }
            BuildCardList.clearItems();
            foreach (CardDefine card in
                cards.Where(
                    c => c.id != 0 &&
                    c is ServantCardDefine servant &&
                    !servant.isToken
                    )
                )
            {
                var skin = parent.game.cards.GetCardSkin(card.id);
                var item = BuildCardList.addItem();
                item.update(card, skin);
            }
            BuildDeckList.sortItems((a, b) =>
            {
                if (a.card.getCost() != b.card.getCost())
                    return a.card.getCost() - b.card.getCost();
                else
                    return a.card.id - b.card.id;
            });
            DragCard.hide();
        }
        private void Update()
        {
            DeckCountText.text = BuildDeckList.getItems().Select(i => i.count).Sum() + "/30";

            if (DragCard.isDisplaying)
            {
                var input = EventSystem.current.currentInputModule.input;
                DragCard.transform.position = input.mousePosition;
                Vector2 position = input.mousePosition;
                if (DeckViewScroll.GetComponent<RectTransform>().rect.Contains(DeckViewScroll.GetComponent<RectTransform>().InverseTransformPoint(position)))
                {
                    if (!DragCard.BuildDeckListItem.isDisplaying)
                    {
                        DragCard.BuildDeckListItem.display();
                        DragCard.BuildDeckListItem.update(DragCard.BuildDeckListItem.card, parent.game.cards.GetCardSkin(DragCard.BuildDeckListItem.card.id), 1);
                    }
                    DragCard.Card.hide();
                }
                else
                {
                    if (!DragCard.Card.isDisplaying)
                    {
                        DragCard.Card.display();
                        DragCard.Card.update(DragCard.BuildDeckListItem.card, parent.game.cards.GetCardSkin(DragCard.BuildDeckListItem.card.id));
                    }
                    DragCard.BuildDeckListItem.hide();
                }
                if (input.GetMouseButtonUp(0))
                {
                    //停止拖拽
                    stopDrag(position);
                }
            }
        }
        public void startDrag(CardDefine card, Vector2 position)
        {
            DragCard.display();
            DragCard.rectTransform.position = position;
            if (DeckViewScroll.GetComponent<RectTransform>().rect.Contains(DeckViewScroll.GetComponent<RectTransform>().InverseTransformPoint(position)))
            {
                DragCard.BuildDeckListItem.display();
                DragCard.Card.hide();
            }
            else
            {
                DragCard.Card.display();
                DragCard.BuildDeckListItem.hide();
            }
            DragCard.BuildDeckListItem.update(card, parent.game.cards.GetCardSkin(card.id), 1);
            DragCard.Card.update(card, parent.game.cards.GetCardSkin(card.id));
        }
        public void stopDrag(Vector2 position)
        {
            DragCard.hide();
            Vector3 localPosition = DeckViewScroll.GetComponent<RectTransform>().InverseTransformPoint(position);
            if (!DeckViewScroll.GetComponent<RectTransform>().rect.Contains(localPosition))
                return;
            if (BuildDeckList.Select(i => i.count).Sum() >= 30)
            {
                //你不能带更多的卡牌
                return;
            }
            var item = BuildDeckList.getItems().FirstOrDefault(i => i.card == DragCard.BuildDeckListItem.card);
            if (item != null)
                item.count++;
            else
            {
                var newItem = BuildDeckList.addItem();
                newItem.update(DragCard.BuildDeckListItem.card, parent.game.cards.GetCardSkin(DragCard.BuildDeckListItem.card.id), 1);
            }
            BuildDeckList.sortItems((a, b) =>
            {
                if (a.card.getCost() != b.card.getCost())
                    return a.card.getCost() - b.card.getCost();
                else
                    return a.card.id - b.card.id;
            });
        }
    }
}