namespace TouhouHeartstone.Backend
{
    class MaxGemChangeEvent : VisibleEvent
    {
        public MaxGemChangeEvent(Player player, int value) : base("onMaxGemChange")
        {
            this.player = player;
            this.value = value;
        }
        Player player { get; }
        int value { get; }
        public override void execute(CardEngine engine)
        {
            player.setProp("maxGem", value);
            if (player.getProp<int>("maxGem") < 0)
                player.setProp("maxGem", 0);
            else if (player.getProp<int>("maxGem") > 10)
                player.setProp("maxGem", 10);
        }
        public override EventWitness getWitness(CardEngine engine, Player player)
        {
            EventWitness witness = new EventWitness("onMaxGemChange");
            witness.setVar("playerIndex", engine.getPlayerIndex(this.player));
            witness.setVar("value", player.getProp<int>("maxGem"));
            return witness;
        }
    }
}