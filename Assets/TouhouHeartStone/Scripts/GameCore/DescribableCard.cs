using TouhouCardEngine;
using TouhouCardEngine.Interfaces;

namespace TouhouHeartstone
{
    public interface IDescribableEventArg : IEventArg
    {
        DescribableCard getCard(IGame game, IPlayer viewer);
        DescribableCard[] getTargets(IGame game, IPlayer viewer);
        string toString(IGame game, IPlayer viewer);

        object[] localizeStringArgs(IGame game, IPlayer viewer);
        string localizeTemplateString(IGame game, IPlayer viewer);
    }
    public class DescribableCard
    {
        public Card card;
        public CardDefine define;
        public DescribableCard(Card card)
        {
            this.card = card;
        }
        public DescribableCard(CardDefine define)
        {
            this.define = define;
        }
        public static implicit operator DescribableCard(Card card)
        {
            if (card is null)
                return null;
            return new DescribableCard(card);
        }
        public static implicit operator DescribableCard(CardDefine define)
        {
            if (define is null)
                return null;
            return new DescribableCard(define);
        }
    }
}
