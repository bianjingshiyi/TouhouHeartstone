using System.Linq;
using System.Collections.Generic;

using TouhouCardEngine;

namespace TouhouHeartstone
{
    class DeathEvent : VisibleEvent
    {
        public DeathEvent(Card[] cards) : base("onDeath")
        {
            this.cards = cards;
        }
        Card[] cards { get; }
        public override void execute(TouhouCardEngine.CardEngine engine)
        {
            List<Player> remainPlayerList = new List<Player>(engine.getPlayers());
            for (int i = 0; i < cards.Length; i++)
            {
                Pile pile = cards[i].pile;
                Player player = pile.owner;
                if (cards[i] != player["Master"][0])
                    pile.moveTo(cards[i], player["Grave"], player["Grave"].count);
                else
                    remainPlayerList.Remove(player);
            }
            if (remainPlayerList.Count < engine.getPlayers().Length)
            {
                if (remainPlayerList.Count > 0)
                    engine.doEvent(new GameEndEvent(remainPlayerList.ToArray()));
                else
                    engine.doEvent(new GameEndEvent(new Player[0]));
            }
        }
        public override EventWitness getWitness(TouhouCardEngine.CardEngine engine, Player player)
        {
            EventWitness witness = new DeathWitness();
            witness.setVar("cardsRID", cards.Select(c => { return c.id; }).ToArray());
            return witness;
        }
    }
    /// <summary>
    /// 死亡事件
    /// </summary>
    public class DeathWitness : EventWitness
    {
        /// <summary>
        /// 触发事件的所有卡片
        /// </summary>
        public int[] cardsRID
        {
            get { return getVar<int[]>("cardsRID"); }
        }
        public DeathWitness() : base("onDeath")
        {
        }
    }
}