namespace TouhouHeartstone.Backend
{
    class UseEvent : VisibleEvent
    {
        public UseEvent(Player player, Card card, int targetPosition, Card targetCard) : base("onUse")
        {
            this.player = player;
            this.card = card;
            this.targetPosition = targetPosition;
            this.targetCard = targetCard;
        }
        Player player { get; }
        Card card { get; }
        int targetPosition { get; }
        Card targetCard { get; }
        public override void execute(CardEngine engine)
        {
            engine.setGem(player, player.getProp<int>("gem") - (card.define as ICost).cost);
            if (card.define is ServantCardDefine)
            {
                //随从卡，将卡置入战场
                engine.doEvent(new SummonEvent(player, card, targetPosition));
            }
            else if (card.define is SpellCardDefine)
            {
                //法术卡，释放效果然后丢进墓地
                player["Hand"].moveTo(card, player["Grave"], player["Grave"].count);
            }
        }
        public override EventWitness getWitness(CardEngine engine, Player player)
        {
            EventWitness witness = new EventWitness("onUse");
            witness.setVar("playerIndex", engine.getPlayerIndex(this.player));
            witness.setVar("cardRID", card.getRID());
            witness.setVar("cardDID", card.define.id);
            witness.setVar("targetPosition", targetPosition);
            witness.setVar("targetCardRID", targetCard != null ? targetCard.getRID() : -1);
            return witness;
        }
    }
}