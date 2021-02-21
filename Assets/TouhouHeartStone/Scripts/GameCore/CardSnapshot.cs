using TouhouCardEngine;
namespace TouhouHeartstone
{
    public class CardSnapshot
    {
        public Card card { get; }
        public CardDefine define { get; }
        public int cost { get; }
        public int attack { get; }
        public int life { get; }
        public CardSnapshot(THHGame game, Card card)
        {
            this.card = card;
            define = card.define;
            cost = card.getCost(game);
            attack = card.getAttack(game);
            life = card.getLife(game);
        }
    }
}