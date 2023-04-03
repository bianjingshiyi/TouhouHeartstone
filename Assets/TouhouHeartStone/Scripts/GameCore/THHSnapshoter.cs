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
            // 英雄的攻击力受武器影响，所以更新一遍。
            snapshot.setProp(DEFINE.ATTACK, card.getAttack(game));
            // 亡语不具有关键词，加上。
            snapshot.setProp(KEYWORD.DEATHRATTLE, card.hasDeathRattle(game));
            return snapshot;
        }
    }
}
