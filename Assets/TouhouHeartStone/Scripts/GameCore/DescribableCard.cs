using TouhouCardEngine;
using TouhouCardEngine.Interfaces;

namespace TouhouHeartstone
{
    public interface IDescribableEventDefine
    {
        DescribableCard getCard(IGame game, EventArg eventArg, IPlayer viewer);
        DescribableCard[] getTargets(IGame game, EventArg eventArg, IPlayer viewer);
        Player getPlayer(IGame game, EventArg eventArg);
        string toString(IGame game, EventArg eventArg, IPlayer viewer);
        object[] localizeStringArgs(IGame game, EventArg eventArg, IPlayer viewer);
        string localizeTemplateString(IGame game, EventArg eventArg, IPlayer viewer);
        object[] localizeLogItemNameArgs(IGame game, EventArg eventArg);
        string localizeLogItemNameTemplate(IGame game, EventArg eventArg);
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
