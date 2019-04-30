using System.Linq;

namespace TouhouHeartstone.Backend
{
    class InitReplaceEvent : VisibleEvent
    {
        public InitReplaceEvent(Player player, Card[] cards) : base("onInitReplace")
        {
            this.player = player;
            originCards = cards;
        }
        Card[] originCards { get; set; }
        public Player player { get; }
        public override void execute(CardEngine engine)
        {
            //先把卡牌放回牌库
            int[] cardsIndex = originCards.Select(c => { return player["Init"].indexOf(c); }).ToArray();
            engine.moveCard(player, "Init", originCards, player, "Deck", 0);
            //然后洗牌
            player["Deck"].shuffle(engine);
            //最后再抽相同数量的卡并替换
            replacedCards = player["Deck"][player["Deck"].count - originCards.Length, player["Deck"].count - 1];
            for (int i = 0; i < replacedCards.Length; i++)
            {
                engine.moveCard(player, "Deck", replacedCards[i], player, "Init", cardsIndex[i]);
            }
            engine.allocateRID(replacedCards);
        }
        Card[] replacedCards { get; set; }
        public override EventWitness getWitness(CardEngine engine, Player player)
        {
            EventWitness witness = new EventWitness("onInitReplace");
            witness.setVar("playerIndex", engine.getPlayerIndex(this.player));
            witness.setVar("originCardsRID", originCards.Select(c => { return c.getRID(); }).ToArray());
            if (player == this.player)
            {
                //自己
                witness.setVar("replacedCardsDID", replacedCards.Select(c => { return c.define.id; }).ToArray());
            }
            //其他玩家
            witness.setVar("replacedCardsRID", replacedCards.Select(c => { return c.getRID(); }).ToArray());
            return witness;
        }
    }
}