using TouhouCardEngine;

namespace TouhouHeartstone.Frontend.Model.Witness
{
    public class OnUse : WitnessHandler
    {
        public override string Name => "onUse";

        protected override bool witnessSuccessHandler(EventWitness witness, DeckController deck, GenericAction callback = null)
        {
            int playerIndex = witness.getVar<int>("playerIndex");
            int cardRID = witness.getVar<int>("cardRID");
            int cardDID = witness.getVar<int>("cardDID");
            int targetPosition = witness.getVar<int>("targetPosition");
            int[] targetCardRID = witness.getVar<int[]>("targetCardsRID");

            UseCardEventArgs args;
            if (targetPosition == -1)
            {
                if (targetCardRID.Length == 0)
                {
                    args = new UseCardEventArgs();
                }
                else
                {
                    args = new UseCardWithTargetArgs(targetCardRID);
                }
            }
            else
            {
                if (targetCardRID.Length == 0)
                {
                    args = new UseCardWithPositionArgs(targetPosition);
                }
                else
                {
                    args = new UseCardWithTargetPositionArgs(targetPosition, targetCardRID);
                }
            }
            args.CardRID = cardRID;
            args.CardDID = cardDID;
            args.PlayerID = playerIndex;

            deck.RecvEvent(args, callback);
            return true;
        }
    }

    public class OnCountDown : WitnessHandler
    {
        public override string Name => "onCountDown";

        protected override bool witnessSuccessHandler(EventWitness witness, DeckController deck, GenericAction callback = null)
        {
            deck.CountdownStart();
            return false;
        }
    }

    public class OnTimeOut : WitnessHandler
    {
        public override string Name => "onTimeOut";

        protected override bool witnessSuccessHandler(EventWitness witness, DeckController deck, GenericAction callback = null)
        {
            return false;
        }
    }
}
