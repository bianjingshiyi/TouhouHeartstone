using System.Linq;

namespace TouhouHeartstone.Backend
{
    class InitReplaceEvent : VisibleEvent
    {
        public InitReplaceEvent(Player player, int[] cardIndex) : base("onInitReplace")
        {
            this.player = player;
            this.cardIndex = cardIndex;
        }
        Player player { get; }
        int[] cardIndex { get; }
        public override void execute(CardEngine engine)
        {
            //先把卡牌放回牌库
            Card[] card = new Card[cardIndex.Length];
            for (int i = 0; i < card.Length; i++)
            {
                card[i] = player["Init"][cardIndex[i]];
            }
            player["Init"].moveCardTo(card, player["Deck"], 0);
            //然后洗牌
            player["Deck"].shuffle(engine);
            //最后再抽相同数量的卡并替换
            card = player["Deck"][player["Deck"].count - card.Length, player["Deck"].count - 1];
            for (int i = 0; i < cardIndex.Length; i++)
            {
                player["Deck"].moveCardTo(card[i], player["Init"], cardIndex[i]);
            }
        }
        public override EventWitness getWitness(CardEngine engine, Player player)
        {
            EventWitness witness = new EventWitness("onInitReplace");
            witness.setVar("player_index", engine.getPlayerIndex(this.player));
            if (player == this.player)
            {
                //自己
                witness.setVar("replaced_define_id", cardIndex.Select(e => { return player["Init"][e].define.id; }).ToArray());
            }
            else
            {
                //其他玩家
                witness.setVar("replaced_count", cardIndex.Length);
            }
            return witness;
        }
    }
}