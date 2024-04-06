using MessagePack;
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
    [MessagePackObject]
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
        [Key(0)]
        public int _id;
        [Key(1)]
        public DefineReference _cardRef;
        [Key(2)]
        public string[] _tags;
    }
}
