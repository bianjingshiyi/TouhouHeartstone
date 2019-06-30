using System.Linq;

namespace TouhouHeartstone
{
    class UseEvent : VisibleEvent
    {
        public UseEvent(Player player, Card card, int targetPosition, Card[] targetCards) : base("onUse")
        {
            this.player = player;
            this.card = card;
            this.targetPosition = targetPosition;
            this.targetCards = targetCards;
        }
        Player player { get; }
        Card card { get; }
        int targetPosition { get; }
        Card[] targetCards { get; }
        public override void execute(CardEngine engine)
        {
            engine.setGem(player, player.getProp<int>("gem") - card.define.getProp<int>("cost"));
            if (card.define is ServantCardDefine || (card.define is GeneratedCardDefine && (card.define as GeneratedCardDefine).type == CardDefineType.servant))
            {
                //随从卡，将卡置入战场
                engine.summon(player, card, targetPosition);
                Effect effect = card.define.effects.FirstOrDefault(e => { return card.pile.name == e.pile && e.trigger == "onUse"; });
                if (effect != null)
                    effect.execute(engine, player, card, targetCards);
            }
            else if (card.define is SpellCardDefine || (card.define is GeneratedCardDefine && (card.define as GeneratedCardDefine).type == CardDefineType.spell))
            {
                //法术卡，释放效果然后丢进墓地
                player["Hand"].moveTo(card, player["Warp"], player["Warp"].count);
                Effect effect = card.define.effects.FirstOrDefault(e => { return card.pile.name == e.pile && e.trigger == "onUse"; });
                if (effect != null)
                    effect.execute(engine, player, card, targetCards);
                player["Warp"].moveTo(card, player["Grave"], player["Grave"].count);
            }
        }
        public override EventWitness getWitness(CardEngine engine, Player player)
        {
            EventWitness witness = new UseWitness();
            witness.setVar("playerIndex", engine.getPlayerIndex(this.player));
            witness.setVar("cardRID", card.getRID());
            witness.setVar("cardDID", card.define.id);
            witness.setVar("targetPosition", targetPosition);
            witness.setVar("targetCardsRID", targetCards.Select(targetCard => { return targetCard != null ? targetCard.getRID() : -1; }).ToArray());
            return witness;
        }
    }
    /// <summary>
    /// 使用卡片事件
    /// </summary>
    public class UseWitness : EventWitness
    {
        /// <summary>
        /// 使用卡片的玩家索引
        /// </summary>
        public int playerIndex
        {
            get { return getVar<int>("playerIndex"); }
        }
        /// <summary>
        /// 使用卡片的RID
        /// </summary>
        public int cardRID
        {
            get { return getVar<int>("cardRID"); }
        }
        /// <summary>
        /// 使用卡片的DID
        /// </summary>
        public int cardDID
        {
            get { return getVar<int>("cardDID"); }
        }
        /// <summary>
        /// 卡片的使用目标的RID
        /// </summary>
        public int[] targetCardsRID
        {
            get { return getVar<int[]>("targetCardsRID"); }
        }
        /// <summary>
        /// 如果使用的随从卡，在战场上的放置位置
        /// </summary>
        public int targetPosition
        {
            get { return getVar<int>("targetPosition"); }
        }
        public UseWitness() : base("onUse")
        {
        }
    }
    public partial class CardEngine
    {
        public void use(Player player, Card card, int position, Card[] targetCards)
        {
            doEvent(new UseEvent(player, card, position, targetCards));
        }
    }
}