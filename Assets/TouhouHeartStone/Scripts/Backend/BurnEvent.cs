namespace TouhouHeartstone.Backend
{
    class BurnEvent : VisibleEvent
    {
        public BurnEvent(Player player, Card card) : base("onBurn")
        {
            this.player = player;
            this.card = card;
        }
        Player player { get; }
        Card card { get; }
        public override void execute(CardEngine engine)
        {
            player["Deck"].moveTo(card, player["Grave"], player["Grave"].count);
            engine.allocateRID(card);
        }
        public override EventWitness getWitness(CardEngine engine, Player player)
        {
            EventWitness witness = new BurnWitness();
            witness.setVar("playerIndex", engine.getPlayerIndex(this.player));
            witness.setVar("cardRID", card.getRID());
            witness.setVar("cardDID", card.define.id);
            return witness;
        }
    }
    /// <summary>
    /// 爆牌事件
    /// </summary>
    public class BurnWitness : EventWitness
    {
        /// <summary>
        /// 触发事件的玩家索引
        /// </summary>
        public int playerIndex
        {
            get { return getVar<int>("playerIndex"); }
        }
        /// <summary>
        /// 触发事件的卡片RID
        /// </summary>
        public int cardRID
        {
            get { return getVar<int>("cardRID"); }
        }
        /// <summary>
        /// 触发事件的卡片DID
        /// </summary>
        public int cardDID
        {
            get { return getVar<int>("cardDID"); }
        }
        public BurnWitness() : base("onBurn")
        {
        }
    }
}