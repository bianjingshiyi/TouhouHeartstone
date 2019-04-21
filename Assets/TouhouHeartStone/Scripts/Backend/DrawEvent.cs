namespace TouhouHeartstone.Backend
{
    class DrawEvent : VisibleEvent
    {
        public DrawEvent(Player player) : base("onDraw")
        {
            this.player = player;
        }
        public Player player { get; } = null;
        public override void execute(CardEngine engine)
        {
            //如果牌库里有牌
            if (player["Deck"].count > 0)
            {
                //如果手牌小于10
                if (player["Hand"].count < 10)
                {
                    //抽牌
                    card = player["Deck"].top;
                    engine.moveCard(player, "Deck", card, player, "Hand", player["Hand"].count);
                    engine.allocateRID(card);
                }
                else
                {
                    //TODO:爆牌
                }
            }
            else
            {
                //TODO:疲劳
            }
        }
        public Card card { get; private set; } = null;
        public override EventWitness getWitness(CardEngine engine, Player player)
        {
            EventWitness witness = new EventWitness("onDraw");
            witness.setVar("playerIndex", engine.getPlayerIndex(this.player));
            if (card != null)
            {
                if (player == this.player)
                    witness.setVar("cardDID", card.define.id);
                witness.setVar("cardRID", card.getRID());
            }
            return witness;
        }
    }
}