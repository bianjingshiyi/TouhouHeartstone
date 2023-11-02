using System;
using UnityEngine;
using UnityEngine.EventSystems;

namespace UI
{
    public class CardGemSlotGroup : MonoBehaviour
    {
        public void update(CardGemSlotGroupViewData viewData)
        {
            var slotsData = viewData.slotsViewData;
            _slotList.updateList(slotsData.Length,
                (i, rect) =>
                {
                    var slot = rect.GetComponent<CardGemSlot>();
                    var slotData = slotsData[i];
                    slot.update(slotData);
                },
                rect =>
                {
                    var slot = rect.GetComponent<CardGemSlot>();
                    slot.onBeginDrag += onSlotBeginDragCallback;
                    slot.onDrag += onSlotDragCallback;
                    slot.onEndDrag += onSlotEndDragCallback;
                },
                rect =>
                {
                    var slot = rect.GetComponent<CardGemSlot>();
                    slot.onBeginDrag -= onSlotBeginDragCallback;
                    slot.onDrag -= onSlotDragCallback;
                    slot.onEndDrag -= onSlotEndDragCallback;
                });
        }
        #region 私有方法

        #region 事件回调
        private void onSlotBeginDragCallback(CardGemSlot slot, PointerEventData pointer) => onSlotBeginDrag?.Invoke(this, _slotList.indexOf(slot), pointer);
        private void onSlotDragCallback(CardGemSlot slot, PointerEventData pointer) => onSlotDrag?.Invoke(this, _slotList.indexOf(slot), pointer);
        private void onSlotEndDragCallback(CardGemSlot slot, PointerEventData pointer) => onSlotEndDrag?.Invoke(this, _slotList.indexOf(slot), pointer);
        #endregion

        #endregion

        public event Action<CardGemSlotGroup, int, PointerEventData> onSlotBeginDrag;
        public event Action<CardGemSlotGroup, int, PointerEventData> onSlotDrag;
        public event Action<CardGemSlotGroup, int, PointerEventData> onSlotEndDrag;

        [SerializeField]
        private ElementList _slotList;
    }
    public struct CardGemSlotGroupViewData
    {
        public CardGemSlotViewData[] slotsViewData;
    }
}
