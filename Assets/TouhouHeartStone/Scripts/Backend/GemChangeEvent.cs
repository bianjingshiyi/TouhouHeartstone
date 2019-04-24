namespace TouhouHeartstone.Backend
{
    class GemChangeEvent : VisibleEvent
    {
        public GemChangeEvent(Player player, int value) : base("onGemChange")
        {
            this.player = player;
            this.value = value;
        }
        Player player { get; }
        int value { get; }
        public override void execute(CardEngine engine)
        {
            player.setProp("gem", value);
            if (player.getProp<int>("gem") < 0)
                player.setProp("gem", 0);
            else if (player.getProp<int>("gem") > 10)
                player.setProp("gem", 10);
        }
        public override EventWitness getWitness(CardEngine engine, Player player)
        {
            EventWitness witness = new EventWitness("onGemChange");
            witness.setVar("playerIndex", engine.getPlayerIndex(this.player));
            witness.setVar("value", player.getProp<int>("gem"));
            return witness;
        }
    }
}