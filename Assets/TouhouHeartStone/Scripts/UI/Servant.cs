using UnityEngine.EventSystems;
using UnityEngine;

namespace UI
{
    partial class Servant : IPointerEnterHandler, IPointerExitHandler
    {
        public TouhouCardEngine.Card card { get; private set; }
        public void update(TouhouCardEngine.Card card, CardSkinData skin)
        {
            this.card = card;

            if (skin != null)
            {
                Image.sprite = skin.image;
            }
        }
        void IPointerEnterHandler.OnPointerEnter(PointerEventData eventData)
        {
            Table table = GetComponentInParent<Table>();
            if (eventData.position.x < Screen.width / 2)
                table.LargeCard.rectTransform.localPosition = new Vector3(250, 0);
            else
                table.LargeCard.rectTransform.localPosition = new Vector3(-250, 0);
            table.LargeCard.display();
            table.LargeCard.update(card, table.getSkin(card));
        }
        void IPointerExitHandler.OnPointerExit(PointerEventData eventData)
        {
            Table table = GetComponentInParent<Table>();
            table.LargeCard.hide();
        }
    }
}
