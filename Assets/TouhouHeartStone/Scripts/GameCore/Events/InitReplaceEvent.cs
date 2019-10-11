using System.Linq;

using TouhouCardEngine;

namespace TouhouHeartstone
{
    class InitReplaceEvent : VisibleEvent
    {
        public InitReplaceEvent(Player player, Card[] cards) : base("onInitReplace")
        {
            this.player = player;
            originCards = cards;
        }
        Card[] originCards { get; set; }
        public Player player
        {
            get { return getProp<Player>("player"); }
            private set { setProp("player", value); }
        }
        public override void execute(TouhouCardEngine.CardEngine engine)
        {
            replacedCards = player["Init"].replaceByRandom(engine, originCards, player["Deck"]);
            engine.registerCards(replacedCards);
        }
        Card[] replacedCards { get; set; }
        public override EventWitness getWitness(TouhouCardEngine.CardEngine engine, Player player)
        {
            EventWitness witness = new InitReplaceWitness();
            witness.setVar("playerIndex", engine.getPlayerIndex(this.player));
            witness.setVar("originCardsRID", originCards.Select(c => { return c.id; }).ToArray());
            if (player == this.player)
            {
                //自己
                witness.setVar("replacedCardsDID", replacedCards.Select(c => { return c.define.id; }).ToArray());
            }
            //其他玩家
            witness.setVar("replacedCardsRID", replacedCards.Select(c => { return c.id; }).ToArray());
            return witness;
        }
    }
    /// <summary>
    /// 替换初始手牌事件
    /// </summary>
    public class InitReplaceWitness : EventWitness
    {
        /// <summary>
        /// 替换初始手牌的玩家索引
        /// </summary>
        public int playerIndex
        {
            get { return getVar<int>("playerIndex"); }
        }
        /// <summary>
        /// 被替换的所有初始手牌RID
        /// </summary>
        public int[] originCardsRID
        {
            get { return getVar<int[]>("originCardsRID"); }
        }
        /// <summary>
        /// 替换后的所有初始手牌DID
        /// </summary>
        public int[] replacedCardsDID
        {
            get { return getVar<int[]>("replacedCardsDID"); }
        }
        /// <summary>
        /// 替换后的所有初始手牌RID
        /// </summary>
        public int[] replacedCardsRID
        {
            get { return getVar<int[]>("replacedCardsRID"); }
        }
        public InitReplaceWitness() : base("onInitReplace")
        {
        }
    }
    public static partial class CardEngineExtension
    {
        public static void initReplace(this TouhouCardEngine.CardEngine engine, Player player, Card[] cards)
        {
            engine.doEvent(new InitReplaceEvent(player, cards));
        }
    }
}