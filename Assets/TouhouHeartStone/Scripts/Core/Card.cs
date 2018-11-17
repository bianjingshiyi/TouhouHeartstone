using System;

namespace TouhouHeartstone
{
    [Serializable]
    abstract class Card
    {
        public static Card create(CardInstance instance)
        {
            throw new NotImplementedException();
        }
        public abstract void use(int position, Card target);
        public CardInstance instance { get; private set; }
        public override string ToString()
        {
            return instance.ToString();
        }
    }
    abstract class MonsterCard : Card
    {
    }
}