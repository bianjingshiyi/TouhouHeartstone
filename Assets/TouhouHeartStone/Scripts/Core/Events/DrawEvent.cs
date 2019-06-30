namespace TouhouHeartstone
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
            //抽牌
            card = player["Deck"].top;
            engine.moveCard(player, "Deck", card, player, "Hand", player["Hand"].count);
            engine.allocateRID(card);
        }
        public Card card { get; private set; } = null;
        public override EventWitness getWitness(CardEngine engine, Player player)
        {
            EventWitness witness = new DrawWitness();
            witness.setVar("playerIndex", engine.getPlayerIndex(this.player));
            if (player == this.player)
                witness.setVar("cardDID", card.define.id);
            witness.setVar("cardRID", card.getRID());
            return witness;
        }
    }
    /// <summary>
    /// 抽卡事件
    /// </summary>
    public class DrawWitness : EventWitness
    {
        /// <summary>
        /// 抽卡玩家索引
        /// </summary>
        public int playerIndex
        {
            get { return getVar<int>("playerIndex"); }
        }
        /// <summary>
        /// 抽到的卡片的RID
        /// </summary>
        public int cardRID
        {
            get { return getVar<int>("cardRID"); }
        }
        /// <summary>
        /// 抽到的卡片DID
        /// </summary>
        public int cardDID
        {
            get { return getVar<int>("cardDID"); }
        }
        public DrawWitness() : base("onDraw")
        {
        }
    }
}