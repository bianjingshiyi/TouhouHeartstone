namespace TouhouHeartstone.Backend
{
    class BurnEvent : VisibleEvent
    {
        public BurnEvent(Player player, Card card) : base("onBurn")
        {
            this.player = player;
            this.card = card;
        }
        Player player { get; }
        Card card { get; }
        public override void execute(CardEngine engine)
        {
            player["Deck"].moveTo(card, player["Grave"], player["Grave"].count);
            engine.allocateRID(card);
        }
        public override EventWitness getWitness(CardEngine engine, Player player)
        {
            EventWitness witness = new EventWitness("onBurn");
            witness.setVar("playerIndex", engine.getPlayerIndex(this.player));
            witness.setVar("cardRID", card.getRID());
            witness.setVar("cardDID", card.define.id);
            return witness;
        }
    }
}