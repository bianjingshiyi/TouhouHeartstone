using System;

namespace TouhouHeartstone.Backend
{
    class AttackEvent : VisibleEvent
    {
        public AttackEvent(Player player, Card card, Card targetCard) : base("onAttack")
        {
            this.player = player;
            this.card = card;
            this.targetCard = targetCard;
        }
        Player player { get; }
        Card card { get; }
        Card targetCard { get; }
        public override void execute(CardEngine engine)
        {
            Card[] cards = new Card[2];
            int[] amounts = new int[2];
            if (card.getRID() < targetCard.getRID())
            {
                cards[0] = card;
                amounts[0] = targetCard.getProp<int>("attack");
                cards[1] = targetCard;
                amounts[1] = card.getProp<int>("attack");
            }
            else
            {
                cards[0] = targetCard;
                amounts[0] = card.getProp<int>("attack");
                cards[1] = card;
                amounts[1] = card.getProp<int>("attack");
            }
            engine.doEvent(new DamageEvent(cards, amounts));
        }
        public override EventWitness getWitness(CardEngine engine, Player player)
        {
            EventWitness witness = new EventWitness("onAttack");
            witness.setVar("playerIndex", engine.getPlayerIndex(this.player));
            witness.setVar("cardRID", card.getRID());
            witness.setVar("targetCardRID", targetCard.getRID());
            return witness;
        }
    }
}