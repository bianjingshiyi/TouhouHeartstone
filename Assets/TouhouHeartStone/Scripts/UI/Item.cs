using BJSYGameCore.UI;
using TouhouHeartstone;
using UnityEngine;
using UnityEngine.EventSystems;

namespace UI
{
    partial class Item: IPointerEnterHandler, IPointerExitHandler
    {
        public ActionEvent<Item, PointerEventData> onPointerEnter { get; set; } = new ActionEvent<Item, PointerEventData>();
        public ActionEvent<Item, PointerEventData> onPointerExit { get; set; } = new ActionEvent<Item, PointerEventData>();

        public TouhouCardEngine.Card card { get; private set; } = null;
        public void update(TouhouCardEngine.Card card, CardSkinData skin)
        {
            this.card = card;

            Image.sprite = skin.image;
            if (card.getAttack(null) > 0)
            {
                //AttackText.text = card.getAttack(null).ToString();
                AttackImage.gameObject.SetActive(true);

            }
            else
            {
                AttackImage.gameObject.SetActive(false);
            }
            //DurabilityText.text = card.getCurrentLife(null).ToString();
        }
        [SerializeField]
        bool _isHovered = false;
        void IPointerEnterHandler.OnPointerEnter(PointerEventData eventData)
        {
            _isHovered = true;
            onPointerEnter.invoke(this, eventData);
        }
        void IPointerExitHandler.OnPointerExit(PointerEventData eventData)
        {
            _isHovered = false;
            onPointerExit.invoke(this, eventData);
        }
        private void Update()
        {
            if (_isHovered)
            {
                Vector2 position;
                position = EventSystem.current.currentInputModule.input.mousePosition;
                /* if (!rectTransform.contains(position))
                {
                    onPointerExit?.invoke(this, null);
                    _isHovered = false;
                } */
            }
        }
        public SimpleAnim onEquip;
        public SimpleAnim onLifeDown;
        public SimpleAnim onDestroy;
        public SimpleAnim onSetDefine;
        public SimpleAnim onItemOff;
        public SimpleAnim onItemON;
        public const string ItemOff = "ItemOff";
    }
}
