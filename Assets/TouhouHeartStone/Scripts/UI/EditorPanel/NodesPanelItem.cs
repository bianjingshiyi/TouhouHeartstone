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
            _editButton.onClick.AddListener(onEdit);
            if (_doubleClickButton)
                _doubleClickButton.onDoubleClick.AddListener(onDoubleClickedCallback);
        }
        private void onEdit()
        {
            onEditClicked?.Invoke(this);
        }
        private void onDoubleClickedCallback()
        {
            onDoubleClicked?.Invoke(this);
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
        public event Action<NodesPanelItem> onDoubleClicked;


        [SerializeField]
        private Image _icon;
        [SerializeField]
        private Text _name;
        [SerializeField]
        private DoubleClickButtonExt _doubleClickButton;
        [SerializeField]
        private GameObject _editButtonObj;
        [SerializeField]
        private Button _editButton;
    }
}
