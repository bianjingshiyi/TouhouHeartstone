using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Game.PVE
{
    public class CardGem
    {
    }
    public class CardGemSlot
    {
        public void setGem(CardGem gem)
        {
            _gem = gem;
        }
        public CardGem gem => _gem;
        private CardGem _gem;
    }
    public class CardGemSlotGroup
    {
        public CardGemSlotGroup(int count)
        {
            _slots = new CardGemSlot[count];
            for (int i = 0; i < count; i++)
            {
                _slots[i] = new CardGemSlot();
            }
        }
        public CardGemSlot getSlotAt(int index)
        {
            return _slots[index];
        }
        public CardGemSlot[] slots => _slots;
        private CardGemSlot[] _slots;
    }
}
