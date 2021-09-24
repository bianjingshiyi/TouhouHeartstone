using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace UI
{
    public class ActionNodeItemInput : MonoBehaviour
    {
        public void setName(string name)
        {
            _nameText.text = name;
        }
        public Vector2 getNodeWorldPosition()
        {
            return _nodeRect.position;
        }
        protected void Awake()
        {
            EventTrigger trigger = _nodeRect.gameObject.AddComponent<EventTrigger>();
            EventTrigger.Entry entry = new EventTrigger.Entry();
            entry.eventID = EventTriggerType.BeginDrag;
            entry.callback.AddListener(onBeginDragNodeCallback);
            trigger.triggers.Add(entry);
            entry = new EventTrigger.Entry();
            entry.eventID = EventTriggerType.Drag;
            entry.callback.AddListener(onDragNodeCallback);
            trigger.triggers.Add(entry);
            entry = new EventTrigger.Entry();
            entry.eventID = EventTriggerType.EndDrag;
            entry.callback.AddListener(onEndDragNodeCallback);
            trigger.triggers.Add(entry);
        }
        private void onBeginDragNodeCallback(BaseEventData eventData)
        {
            onBeginDragNode?.Invoke(this, eventData);
        }
        private void onDragNodeCallback(BaseEventData eventData)
        {
            onDragNode?.Invoke(this, eventData);
        }
        private void onEndDragNodeCallback(BaseEventData eventData)
        {
            onEndDragNode?.Invoke(this, eventData);
        }
        public event Action<ActionNodeItemInput, BaseEventData> onBeginDragNode;
        public event Action<ActionNodeItemInput, BaseEventData> onDragNode;
        public event Action<ActionNodeItemInput, BaseEventData> onEndDragNode;
        [SerializeField]
        Text _nameText;
        [SerializeField]
        RectTransform _nodeRect;
    }
}