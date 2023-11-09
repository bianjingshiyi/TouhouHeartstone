using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TouhouCardEngine;
using TouhouHeartstone.Builtin;

namespace Game.PVE
{
    public class CardGem
    {
        public CardGem(DefineReference def)
        {
            _cardRef = def;
        }
        public void setTags(string[] tags)
        {
            _tags = tags;
        }
        public DefineReference cardRef => _cardRef;
        public string[] tags => _tags;
        private DefineReference _cardRef;
        private string[] _tags;
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
        public void setTags(string[] tags)
        {
            _tags = tags;
        }
        public CardGemSlot[] slots => _slots;
        private CardGemSlot[] _slots;
        public string[] tags => _tags;
        private string[] _tags;
    }
}
