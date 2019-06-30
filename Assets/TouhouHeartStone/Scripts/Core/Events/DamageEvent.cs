using System.Linq;

using System.Collections.Generic;

namespace TouhouHeartstone
{
    class DamageEvent : VisibleEvent
    {
        public DamageEvent(Card[] cards, int[] amounts) : base("onDamage")
        {
            this.cards = cards;
            this.amounts = amounts;
        }
        Card[] cards { get; }
        int[] amounts { get; }
        public override void execute(CardEngine engine)
        {
            //造成伤害
            List<Card> deathList = new List<Card>();
            for (int i = 0; i < cards.Length; i++)
            {
                if (amounts[i] > cards[i].getProp<int>("life"))
                    amounts[i] = cards[i].getProp<int>("life");
                cards[i].setProp("life", PropertyChangeType.add, -amounts[i]);
                if (cards[i].getProp<int>("life") <= 0)
                    deathList.Add(cards[i]);
            }
            engine.doEvent(new DeathEvent(deathList.ToArray()));
        }
        public override EventWitness getWitness(CardEngine engine, Player player)
        {
            EventWitness witness = new DamageWitness();
            witness.setVar("cardsRID", cards.Select(c => { return c.getRID(); }).ToArray());
            witness.setVar("amounts", amounts);
            return witness;
        }
    }
    /// <summary>
    /// 爆牌事件
    /// </summary>
    public class DamageWitness : EventWitness
    {
        /// <summary>
        /// 触发事件的所有卡片RID
        /// </summary>
        public int[] cardsRID
        {
            get { return getVar<int[]>("cardsRID"); }
        }
        /// <summary>
        /// 所有卡片受到的伤害
        /// </summary>
        public int[] amounts
        {
            get { return getVar<int[]>("amounts"); }
        }
        public DamageWitness() : base("onDamage")
        {
        }
    }
}