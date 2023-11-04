using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace UI
{
    public class CardGemSlot : MonoBehaviour
    {
        public void update(CardGemSlotViewData viewData)
        {
            //image.sprite = viewData.frameSprite;
            gem.update(viewData.gemViewData);
        }
        public CardGem getGem()
        {
            return gem;
        }
        private void Awake()
        {
            gem.onBeginDrag += (g, p) => onBeginDrag?.Invoke(this, p);
            gem.onDrag += (g, p) => onDrag?.Invoke(this, p);
            gem.onEndDrag += (g, p) => onEndDrag?.Invoke(this, p);
        }

        public event Action<CardGemSlot, PointerEventData> onBeginDrag;
        public event Action<CardGemSlot, PointerEventData> onDrag;
        public event Action<CardGemSlot, PointerEventData> onEndDrag;

        [SerializeField]
        private Image image;
        [SerializeField]
        private CardGem gem;
    }
    public struct CardGemSlotViewData
    {
        public Sprite frameSprite;
        public CardGemViewData gemViewData;
    }
}
