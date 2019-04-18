using System.Collections.Generic;

namespace TouhouHeartstone.Backend
{
    class DamageEvent : VisibleEvent
    {
        public DamageEvent(Card[] cards, int[] amounts) : base("onDamage")
        {
            this.cards = cards;
            this.amounts = amounts;
        }
        Card[] cards { get; }
        int[] amounts { get; }
        public override void execute(CardEngine engine)
        {
            //造成伤害
            List<Card> deathList = new List<Card>();
            for (int i = 0; i < cards.Length; i++)
            {
                if (amounts[i] > cards[i].getProp<int>("life"))
                    amounts[i] = cards[i].getProp<int>("life");
                cards[i].setProp("life", PropertyChangeType.add, -amounts[i]);
                if (cards[i].getProp<int>("life") <= 0)
                    deathList.Add(cards[i]);
            }
            engine.doEvent(new DeathEvent(deathList.ToArray()));
        }
        public override EventWitness getWitness(CardEngine engine, Player player)
        {
            EventWitness witness = new EventWitness("onDamage");
            witness.setVar("cardsRID", cards.Select(c => { return c.getRID(); }));
            witness.setVar("amounts", amounts);
            return witness;
        }
    }
}