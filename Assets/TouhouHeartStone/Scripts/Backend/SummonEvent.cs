namespace TouhouHeartstone.Backend
{
    class SummonEvent : VisibleEvent
    {
        public SummonEvent(Player player, Card card, int position) : base("onSummon")
        {
            this.player = player;
            this.card = card;
            this.position = position;
        }
        Player player { get; }
        Card card { get; }
        int position { get; }
        public override void execute(CardEngine engine)
        {
            engine.doEvent(new MoveCardEvent(player, "Hand", card, player, "Field", position));
            card.setProp("life", (card.define as ServantCardDefine).life);
            card.setProp("attack", (card.define as ServantCardDefine).attack);
        }
        public override EventWitness getWitness(CardEngine engine, Player player)
        {
            EventWitness witness = new EventWitness("onSummon");
            witness.setVar("playerIndex", engine.getPlayerIndex(this.player));
            witness.setVar("cardRID", card.getRID());
            witness.setVar("cardDID", card.define.id);
            witness.setVar("position", position);
            return witness;
        }
    }
}