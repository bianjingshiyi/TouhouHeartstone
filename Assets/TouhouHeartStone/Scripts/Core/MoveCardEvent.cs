using System.Collections.Generic;
using System.Linq;

namespace TouhouHeartstone
{
    public class MoveCardEvent : Event
    {
        public MoveCardEvent(Player player, string pileName, Card[] cards, Player targetPlayer, string targetPileName, int position) : base("onMoveCard")
        {
            this.player = player;
            this.pileName = pileName;
            this.cards = cards;
            this.targetPlayer = targetPlayer;
            this.targetPileName = targetPileName;
            this.position = position;
        }
        public Player player { get; }
        public string pileName { get; }
        public Card[] cards { get; private set; }
        public Player targetPlayer { get; }
        public string targetPileName { get; }
        public int position { get; }
        public override void execute(CardEngine engine)
        {
            List<Card> removedCards = new List<Card>();
            foreach (Card card in cards)
            {
                for (int i = 0; i < player[pileName].cardList.Count; i++)
                {
                    if (player[pileName].cardList[i] == card)
                    {
                        removedCards.Add(card);
                        player[pileName].cardList.RemoveAt(i);
                        break;
                    }
                }
            }
            if (removedCards.Count > 0)
                targetPlayer[targetPileName].cardList.InsertRange(position, removedCards);
            cards = removedCards.ToArray();
        }
        //public override EventWitness getWitness(CardEngine engine, Player player)
        //{
        //    EventWitness witness = new EventWitness("onMoveCard");
        //    witness.setVar("playerIndex", engine.getPlayerIndex(this.player));
        //    witness.setVar("pileName", pileName);
        //    witness.setVar("cards_id", cards.Select(c => { return c.id; }).ToArray());
        //    witness.setVar("targetplayerIndex", engine.getPlayerIndex(targetPlayer));
        //    witness.setVar("targetPileName", targetPileName);
        //    witness.setVar("position", position);
        //    return witness;
        //}
    }
}