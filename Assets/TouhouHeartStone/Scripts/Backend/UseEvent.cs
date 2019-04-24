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
            //卡牌必须在手牌中才能使用，否则就不算
            cardIndex = player["Hand"].indexOf(card);
            targetCardIndex = player["Field"].indexOf(targetCard);
            if (0 <= cardIndex && cardIndex < player["Hand"].count)
            {
                engine.setGem(player, player.getProp<int>("gem") - (card.define as ICost).cost);
                if (card.define is ServantCardDefine)
                {
                    //随从卡，将卡置入战场
                    engine.doEvent(new SummonEvent(player, card, targetPosition));
                }
            }
        }
        int cardIndex { get; set; }
        int targetCardIndex { get; set; }
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