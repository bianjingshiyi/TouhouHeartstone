using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace UI
{
    public struct PortItemInfo 
    {
        public string name;
        public Color color;
        public bool isControl;
        public PortItemInfo(string name, Color color, bool isControl)
        {
            this.name = name;
            this.color = color;
            this.isControl = isControl;
        }
    }
    public abstract class ActionNodeItemKnob : MonoBehaviour
    {
        public void setName(string name)
        {
            _nameText.text = name;
        }
        /// <summary>
        /// 设置节点高亮
        /// </summary>
        /// <param name="color"></param>
        public void setHighlighted(bool highlight)
        {
            if (_highlightObj)
            {
                _highlightObj.SetActive(highlight);
            }
        }

        /// <summary>
        /// 设置节点颜色
        /// </summary>
        /// <param name="color"></param>
        public void setColor(Color color)
        {
            if (_slotImage != null)
            {
                _slotImage.color = color;
            }
        }

        // <summary>
        /// 获取节点颜色
        /// </summary>
        /// <returns></returns>
        public Color GetColor()
        {
            if (_slotImage != null)
            {
                return _slotImage.color;
            }

            return Color.white;
        }

        public Vector2 getNodeWorldPosition()
        {
            return _nodeRect.position;
        }
        public GameObject getNode()
        {
            return _nodeRect.gameObject;
        }
        public void setSelected(bool selected)
        {
            _selected = selected;
            if (!selected)
            {
                _selectedMaskTime = 0;
                _slotMaskImage.color = Color.clear;
            }
        }
        protected void Awake()
        {
            EventTrigger trigger = _interactableObj.AddComponent<EventTrigger>();

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

            entry = new EventTrigger.Entry();
            entry.eventID = EventTriggerType.PointerClick;
            entry.callback.AddListener(onClickNodeCallback);
            trigger.triggers.Add(entry);
        }
        private void Update()
        {
            if (_selected)
            {
                _selectedMaskTime = _selectedMaskTime + Time.deltaTime;
                float lerp = (-Mathf.Cos(_selectedMaskTime * Mathf.PI * 4) + 1) / 2;
                _slotMaskImage.color = Color.Lerp(color1, color2, lerp);
            }
        }
        protected abstract void onBeginDragNodeCallback(BaseEventData eventData);
        protected abstract void onDragNodeCallback(BaseEventData eventData);
        protected abstract void onEndDragNodeCallback(BaseEventData eventData);
        protected abstract void onClickNodeCallback(BaseEventData eventData);
        [SerializeField]
        Text _nameText;
        [SerializeField]
        RectTransform _nodeRect;
        [SerializeField]
        GameObject _interactableObj;
        [SerializeField]
        Image _slotImage;
        [SerializeField]
        Image _slotMaskImage;
        [SerializeField]
        GameObject _highlightObj;
        bool _selected;
        float _selectedMaskTime;

        /// <summary>
        /// 是否是动作端口。
        /// </summary>
        public bool isControl;

        static readonly Color color1 = Color.clear;
        static readonly Color color2 = new Color(0, 0, 0, 0.5f);
    }
}