using System;
using System.Collections.Generic;

namespace TouhouHeartstone
{
    [Serializable]
    public abstract class Card
    {
        public static Card create(CardInstance instance, Player owner)
        {
            return new LuckyCoin(instance, owner);
        }
        public Card(CardInstance instance, Player owner)
        {
            this.instance = instance;
            this.owner = owner;
        }
        public Player owner { get; protected set; }
        public abstract void use(Game game, int position, Card target);
        public CardInstance instance { get; protected set; }
        public override string ToString()
        {
            return instance.ToString();
        }
        public static implicit operator Card[] (Card card)
        {
            return new Card[] { card };
        }
    }
    class LuckyCoin : Card
    {
        public LuckyCoin(CardInstance instance, Player owner) : base(instance, owner)
        {
        }
        public override void use(Game game, int position, Card target)
        {
            game.records.addRecord(new AddCrystalRecord(owner.id, 1, CrystalState.temp));
        }
    }
}