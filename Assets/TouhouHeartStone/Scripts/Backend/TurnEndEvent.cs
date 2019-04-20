namespace TouhouHeartstone.Backend
{
    class TurnEndEvent : VisibleEvent
    {
        public TurnEndEvent(Player player) : base("onTurnEnd")
        {
            this.player = player;
        }
        Player player { get; }
        public override void execute(CardEngine engine)
        {
        }
        public override EventWitness getWitness(CardEngine engine, Player player)
        {
            EventWitness witness = new EventWitness("onTurnEnd");
            witness.setVar("playerIndex", engine.getPlayerIndex(this.player));
            return witness;
        }
    }
}