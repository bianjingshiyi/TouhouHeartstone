using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace UI
{
    public class NodesPanelItem : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
    {
        public void setNode(string name, Sprite icon)
        {
            _name.text = name;
            //_icon.sprite = icon;
        }

        void IBeginDragHandler.OnBeginDrag(PointerEventData eventData)
        {
            onBeginDrag?.Invoke(this, eventData);
        }

        void IDragHandler.OnDrag(PointerEventData eventData)
        {
            onDrag?.Invoke(this, eventData);
        }

        void IEndDragHandler.OnEndDrag(PointerEventData eventData)
        {
            onEndDrag?.Invoke(this, eventData);
        }

        public event Action<NodesPanelItem, PointerEventData> onBeginDrag;
        public event Action<NodesPanelItem, PointerEventData> onDrag;
        public event Action<NodesPanelItem, PointerEventData> onEndDrag;


        [SerializeField]
        private Image _icon;
        [SerializeField]
        private Text _name;
    }
}
