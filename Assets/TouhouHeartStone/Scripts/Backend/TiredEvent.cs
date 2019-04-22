namespace TouhouHeartstone.Backend
{
    class TiredEvent : VisibleEvent
    {
        public TiredEvent(Player player) : base("onTired")
        {
            this.player = player;
        }
        Player player { get; }
        public override void execute(CardEngine engine)
        {
            player.setProp("tired", PropertyChangeType.add, 1);
            engine.damage(player["Master"][0], player.getProp<int>("tired"));
        }
        public override EventWitness getWitness(CardEngine engine, Player player)
        {
            EventWitness witness = new EventWitness("onTired");
            witness.setVar("playerIndex", engine.getPlayerIndex(this.player));
            return witness;
        }
    }
}