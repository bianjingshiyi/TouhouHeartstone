using System;

namespace Game.PVE
{
    public class CardGemSlotGroup
    {
        #region 公有方法
        public CardGemSlotGroup(int effectIndex, int count)
        {
            _effectIndex = effectIndex;
            _slots = new CardGemSlot[count];
        }
        public void setSlotAt(int index, CardGemSlot slot)
        {
            var before = _slots[index];
            if (before != null)
            {
                before.onAddGem -= onAddGemCallback;
                before.onRemoveGem -= onRemoveGemCallback;
            }
            _slots[index] = slot;
            if (slot != null)
            {
                slot.onAddGem += onAddGemCallback;
                slot.onRemoveGem += onRemoveGemCallback;
            }
        }
        public CardGemSlot getSlotAt(int index)
        {
            return _slots[index];
        }
        #endregion

        #region 私有方法

        #region 事件回调
        private void onAddGemCallback(CardGemSlot slot, CardGem gem)
        {
            onAddGem?.Invoke(this, Array.IndexOf(_slots, slot), gem);
        }
        private void onRemoveGemCallback(CardGemSlot slot)
        {
            onRemoveGem?.Invoke(this, Array.IndexOf(_slots, slot));
        }
        #endregion

        #endregion

        #region 事件
        public event Action<CardGemSlotGroup, int, CardGem> onAddGem;
        public event Action<CardGemSlotGroup, int> onRemoveGem;
        #endregion

        #region 属性字段
        public CardGemSlot[] slots => _slots;
        public int effectIndex => _effectIndex;
        private CardGemSlot[] _slots;
        private int _effectIndex;
        #endregion
    }
}
