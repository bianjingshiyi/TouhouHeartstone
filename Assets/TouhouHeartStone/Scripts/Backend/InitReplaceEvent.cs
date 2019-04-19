using System.Linq;

namespace TouhouHeartstone.Backend
{
    class InitReplaceEvent : VisibleEvent
    {
        public InitReplaceEvent(Player player, Card[] cards) : base("onInitReplace")
        {
            this.player = player;
            this.cards = cards;
        }
        public Player player { get; }
        Card[] cards { get; set; }
        public override void execute(CardEngine engine)
        {
            //先把卡牌放回牌库
            int[] cardsIndex = cards.Select(c => { return player["Init"].indexOf(c); }).ToArray();
            engine.moveCard(player, "Init", cards, player, "Deck", 0);
            //然后洗牌
            player["Deck"].shuffle(engine);
            //最后再抽相同数量的卡并替换
            cards = player["Deck"][player["Deck"].count - cards.Length, player["Deck"].count - 1];
            for (int i = 0; i < cards.Length; i++)
            {
                engine.moveCard(player, "Deck", cards[i], player, "Init", cardsIndex[i]);
            }
        }
        public override EventWitness getWitness(CardEngine engine, Player player)
        {
            EventWitness witness = new EventWitness("onInitReplace");
            witness.setVar("playerIndex", engine.getPlayerIndex(this.player));
            if (player == this.player)
            {
                //自己
                witness.setVar("replacedCardsDID", cards.Select(c => { return c.define.id; }).ToArray());
            }
            //其他玩家
            witness.setVar("replacedCardsRID", cards.Select(c => { return c.getRID(); }).ToArray());
            return witness;
        }
    }
}