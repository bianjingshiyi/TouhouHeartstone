using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace UI
{
    public class NodesPanelItem : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
    {
        public void setNode(string name, Sprite icon, bool editable)
        {
            _name.text = name;
            _editButtonObj.SetActive(editable);
            //_icon.sprite = icon;
        }
        private void Awake()
        {
            _editButtonObj.SetActive(false);
            _editButton.onClick.AddListener(onEdit);
        }
        private void onEdit()
        {
            onEditClicked?.Invoke(this);
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
        public event Action<NodesPanelItem> onEditClicked;


        [SerializeField]
        private Image _icon;
        [SerializeField]
        private Text _name;
        [SerializeField]
        private GameObject _editButtonObj;
        [SerializeField]
        private Button _editButton;
    }
}
