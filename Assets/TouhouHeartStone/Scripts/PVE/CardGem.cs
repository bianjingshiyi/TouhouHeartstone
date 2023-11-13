using System;
using TouhouCardEngine;

namespace Game.PVE
{
    public class CardGem
    {
        public CardGem(int id, DefineReference def)
        {
            _id = id;
            _cardRef = def;
        }
        public void setTags(string[] tags)
        {
            _tags = tags;
        }
        public DefineReference cardRef => _cardRef;
        public string[] tags => _tags;
        public int id => _id;
        private int _id;
        private DefineReference _cardRef;
        private string[] _tags;
    }
    [Serializable]
    public class SerializableCardGem
    {
        public SerializableCardGem(CardGem gem)
        {
            _id = gem.id;
            _cardRef = gem.cardRef;
            _tags = gem.tags;
        }
        public CardGem toGem()
        {
            var gem = new CardGem(_id, _cardRef);
            gem.setTags(_tags);
            return gem;
        }
        public int _id;
        public DefineReference _cardRef;
        public string[] _tags;
    }
}
