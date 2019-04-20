namespace TouhouHeartstone.Backend
{
    class StartEvent : VisibleEvent
    {
        public StartEvent() : base("onStart")
        {
        }
        public override void execute(CardEngine engine)
        {
            foreach (Player player in engine.getProp<Player[]>("sortedPlayers"))
            {
                player["Init"].moveTo(player["Init"][0, player["Init"].count - 1], player["Hand"], 0);
            }
        }
        public override EventWitness getWitness(CardEngine engine, Player player)
        {
            EventWitness witness = new EventWitness("onStart");
            return witness;
        }
    }
}