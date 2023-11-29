using TouhouCardEngine;
using TouhouCardEngine.Interfaces;

namespace TouhouHeartstone
{
    public class AutocastEventArg : EventArg
    {
        public AutocastEventArg()
        {

        }
        public override void Record(IGame game, EventRecord record)
        {
            record.setVar(EVENTVAR.PLAYER, player);
            record.setCardState(EVENTVAR.CARD, card);
            record.setCardStates(EVENTVAR.TARGETS, targets);
        }
        public Player player
        {
            get => getVar<Player>(EVENTVAR.PLAYER);
            set => setVar(EVENTVAR.PLAYER, value);
        }
        public Card card
        {
            get => getVar<Card>(EVENTVAR.CARD);
            set => setVar(EVENTVAR.CARD, value);
        }
        public Card[] targets
        {
            get => getVar<Card[]>(EVENTVAR.TARGETS);
            set => setVar(EVENTVAR.TARGETS, value);
        }
    }
}
