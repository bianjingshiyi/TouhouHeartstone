using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Controllers;
using TouhouCardEngine;
using TouhouCardEngine.Shared;
using UnityEditor;

namespace Game
{
    public abstract class CardPoolSelectSource
    {
        public CardPoolSelectSource(IEnumerable<ICardPoolReference> poolRef)
        {
            _allCardPools.AddRange(poolRef);
        }
        public ICardPoolReference[] getAllCardPools() => _allCardPools.ToArray();
        public ICardPoolReference[] getSelectedCardPools() => _selectedCardPools.ToArray();
        public void select(ICardPoolReference poolRef)
        {
            if (!_allCardPools.Contains(poolRef))
                return;
            if (_selectedCardPools.Contains(poolRef))
                return;
            _selectedCardPools.Add(poolRef);
        }
        public void deselect(ICardPoolReference poolRef)
        {
            if (!_allCardPools.Contains(poolRef))
                return;
            if (!_selectedCardPools.Contains(poolRef))
                return;
            _selectedCardPools.Remove(poolRef);
        }
        public abstract bool Confirm();
        public string title;
        protected List<ICardPoolReference> _allCardPools = new List<ICardPoolReference>();
        protected List<ICardPoolReference> _selectedCardPools = new List<ICardPoolReference>();
    }
}
