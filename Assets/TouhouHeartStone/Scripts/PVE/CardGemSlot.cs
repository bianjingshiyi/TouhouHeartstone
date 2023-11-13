using System;
using TouhouCardEngine;

namespace Game.PVE
{
    public class CardGemSlot
    {
        #region 公有方法
        public CardGemSlot(int index, string[] tags)
        {
            _index = index;
            _tags = tags;
        }
        public void setGem(CardGem gem)
        {
            if (_gem == gem)
                return;

            if (_gem != null)
            {
                _gem = null;
                removeGem();
            }
            if (gem != null)
            {
                _gem = gem;
                addGem(_gem);
            }
        }
        #endregion

        #region 私有方法
        private void addGem(CardGem gem)
        {
            onAddGem?.Invoke(this, gem);
        }
        private void removeGem()
        {
            onRemoveGem?.Invoke(this);
        }
        #endregion

        #region 事件
        public event Action<CardGemSlot, CardGem> onAddGem;
        public event Action<CardGemSlot> onRemoveGem;
        #endregion

        #region 属性字段
        public CardGem gem => _gem;
        /// <summary>
        /// 该插槽在卡牌属性中的位置。
        /// </summary>
        public int index => _index;
        public string[] tags => _tags;
        private CardGem _gem;
        private int _index;
        private string[] _tags;
        #endregion
    }
}
