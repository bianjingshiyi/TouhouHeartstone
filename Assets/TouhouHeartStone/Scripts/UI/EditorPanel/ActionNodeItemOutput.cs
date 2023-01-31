using System;
using UnityEngine.EventSystems;

namespace UI
{
    public class ActionNodeItemOutput : ActionNodeItemKnob
    {
        protected override void onBeginDragNodeCallback(BaseEventData eventData)
        {
            onBeginDragNode?.Invoke(this, eventData);
        }
        protected override void onDragNodeCallback(BaseEventData eventData)
        {
            onDragNode?.Invoke(this, eventData);
        }
        protected override void onEndDragNodeCallback(BaseEventData eventData)
        {
            onEndDragNode?.Invoke(this, eventData);
        }
        protected override void onClickNodeCallback(BaseEventData eventData)
        {
            onClickNode?.Invoke(this, eventData);
        }

        public event Action<ActionNodeItemOutput, BaseEventData> onBeginDragNode;
        public event Action<ActionNodeItemOutput, BaseEventData> onDragNode;
        public event Action<ActionNodeItemOutput, BaseEventData> onEndDragNode;
        public event Action<ActionNodeItemOutput, BaseEventData> onClickNode;
    }
}