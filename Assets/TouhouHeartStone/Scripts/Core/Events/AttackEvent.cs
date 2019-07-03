using System;

namespace TouhouHeartstone
{
    class AttackEvent : VisibleEvent
    {
        public AttackEvent(Player player, Card card, Card targetCard) : base("onAttack")
        {
            this.player = player;
            this.card = card;
            this.targetCard = targetCard;
        }
        Player player { get; }
        Card card { get; }
        Card targetCard { get; }
        public override void execute(CardEngine engine)
        {
            card.setProp("attackTimes", PropertyChangeType.add, 1);
            if (card.getProp<int>("attack") > 0 && targetCard.getProp<int>("attack") > 0)
            {
                Card[] cards = new Card[2];
                int[] amounts = new int[2];
                if (card.id < targetCard.id)
                {
                    cards[0] = card;
                    amounts[0] = targetCard.getProp<int>("attack");
                    cards[1] = targetCard;
                    amounts[1] = card.getProp<int>("attack");
                }
                else
                {
                    cards[0] = targetCard;
                    amounts[0] = card.getProp<int>("attack");
                    cards[1] = card;
                    amounts[1] = card.getProp<int>("attack");
                }
                engine.damage(cards, amounts);
            }
            else if (card.getProp<int>("attack") > 0)
                engine.damage(targetCard, card.getProp<int>("attack"));
            else if (targetCard.getProp<int>("attack") > 0)
                engine.damage(card, targetCard.getProp<int>("attack"));
        }
        public override EventWitness getWitness(CardEngine engine, Player player)
        {
            EventWitness witness = new AttackWitness();
            witness.setVar("playerIndex", engine.getPlayerIndex(this.player));
            witness.setVar("cardRID", card.id);
            witness.setVar("targetCardRID", targetCard.id);
            return witness;
        }
    }
    /// <summary>
    /// 攻击事件
    /// </summary>
    public class AttackWitness : EventWitness
    {
        /// <summary>
        /// 发布攻击命令的玩家
        /// </summary>
        public int playerIndex
        {
            get { return getVar<int>("playerIndex"); }
        }
        /// <summary>
        /// 进行攻击的卡片RID
        /// </summary>
        public int cardRID
        {
            get { return getVar<int>("cardRID"); }
        }
        /// <summary>
        /// 被攻击的卡片RID
        /// </summary>
        public int targetCardRID
        {
            get { return getVar<int>("targetCardRID"); }
        }
        public AttackWitness() : base("onAttack")
        {
        }
    }
    public partial class CardEngine
    {
        public void attack(Player player, Card card, Card targetCard)
        {
            doEvent(new AttackEvent(player, card, targetCard));
        }
    }
}