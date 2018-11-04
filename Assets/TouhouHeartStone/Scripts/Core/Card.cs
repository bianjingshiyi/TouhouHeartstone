using System;

namespace TouhouHeartstone
{
    [Serializable]
    class Card
    {
        public Card(CardInstance instance)
        {
            this.instance = instance;
        }
        public CardInstance instance { get; private set; }
        public override string ToString()
        {
            return instance.ToString();
        }
    }
}