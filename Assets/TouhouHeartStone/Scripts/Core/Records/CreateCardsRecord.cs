using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TouhouHeartstone
{
    class MoveCardsRecord : Record
    {
        public int playerIndex { get; }
        public string pileName { get; }
        public CardInstance[] cards { get; }
        public int targetPlayerIndex { get; }
        public string targetPileName { get; }
        public int position { get; }
        public Visibility visibility { get; }
        public MoveCardsRecord(int playerIndex, string pileName, CardInstance[] cards, int targetPlayerIndex, string targetPileName, int position, Visibility visibility)
        {
            this.playerIndex = playerIndex;
            this.pileName = pileName;
            this.cards = cards;
            this.targetPlayerIndex = targetPlayerIndex;
            this.targetPileName = targetPileName;
            this.position = position;
            this.visibility = visibility;
        }
        public override Dictionary<int, IWitness> apply(CardEngine game)
        {
            throw new NotImplementedException();
        }
        public override Dictionary<int, IWitness> revert(CardEngine game)
        {
            throw new NotImplementedException();
        }
    }
}