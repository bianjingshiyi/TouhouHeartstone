using TouhouCardEngine;
using TouhouCardEngine.Interfaces;
using TouhouHeartstone;

namespace TouhouHeartStone
{
    public class THHSnapshoter : ISnapshoter
    {
        public CardSnapshot snapshot(IGame game, Card card)
        {
            if (card == null)
                return null;
            var snapshot = new CardSnapshot(game, card);
            return snapshot;
        }
    }
}
