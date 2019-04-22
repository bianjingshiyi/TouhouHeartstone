namespace TouhouHeartstone.Backend
{
    class TurnStartEvent : VisibleEvent
    {
        public TurnStartEvent(Player player) : base("onTurnStart")
        {
            this.player = player;
        }
        Player player { get; }
        public override void execute(CardEngine engine)
        {
            //玩家的最大能量加1但是不超过10，充满玩家的能量。
            engine.setProp("currentPlayer", player);
            if (player.getProp<int>("maxGem") < 10)
                player.setProp("maxGem", PropertyChangeType.add, 1);
            player.setProp("gem", player.getProp<int>("maxGem"));
            //抽一张牌
            engine.draw(player);
        }
        public override EventWitness getWitness(CardEngine engine, Player player)
        {
            EventWitness witness = new EventWitness("onTurnStart");
            witness.setVar("playerIndex", engine.getPlayerIndex(this.player));
            witness.setVar("maxGem", this.player.getProp<int>("maxGem"));
            witness.setVar("gem", this.player.getProp<int>("gem"));
            return witness;
        }
    }
}