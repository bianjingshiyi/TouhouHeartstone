using System.Linq;
using System;
using System.Collections.Generic;

namespace Game
{
    [Serializable]
    public class Deck
    {
        public string name;
        [CardDefineID]
        public int master;
        [Serializable]
        public struct DeckItem
        {
            [CardDefineID]
            public int id;
            public int count;
        }
        public List<DeckItem> deckList = new List<DeckItem>();
        public int[] toIdArray()
        {
            List<int> list = new List<int>
            {
                master
            };
            foreach (var item in deckList)
            {
                if (item.id < 1)
                    continue;
                if (item.count < 1)
                    continue;
                list.AddRange(Enumerable.Repeat(item.id, item.count));
            }
            return list.ToArray();
        }
    }
}
