using TouhouHeartstone;
using UnityEngine;
using UnityEngine.EventSystems;
namespace UI
{
    partial class Deck : IPointerEnterHandler, IPointerExitHandler
    {
        public SimpleAnim onFatigue;
        [SerializeField]
        bool _isHovered = false;
        public ActionEvent<Deck, PointerEventData> onPointerEnter { get; set; } = new ActionEvent<Deck, PointerEventData>();
        public ActionEvent<Deck, PointerEventData> onPointerExit { get; set; } = new ActionEvent<Deck, PointerEventData>();
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
                if (!rectTransform.contains(position))
                {
                    _isHovered = false;
                    onPointerExit.invoke(this, null);
                }
            }
        }
    }
}