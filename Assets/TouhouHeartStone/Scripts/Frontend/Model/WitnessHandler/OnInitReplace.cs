using IGensoukyo.Utilities;

namespace TouhouHeartstone.Frontend.Model.Witness
{
    public class OnInitReplace : WitnessHandler
    {
        public override string Name => "onInitReplace";

        protected override bool witnessSuccessHandler(EventWitness witness, DeckController deck, GenericAction callback)
        {
            int player = witness.getVar<int>("playerIndex");
            int[] originalRID = witness.getVar<int[]>("originCardsRID");
            int[] cardsRID = witness.getVar<int[]>("replacedCardsRID");
            int[] cardsDID = witness.getVar<int[]>("replacedCardsDID", false);

            DebugUtils.NullCheck(cardsRID, "replacedCardsRID");
            DebugUtils.NullCheck(originalRID, "originCardsRID");
            if (cardsDID == null)
                cardsDID = new int[cardsRID.Length];

            var args = new ThrowCardEventArgs(player, originalRID) { NewCards = CardID.ToCardIDs(cardsDID, cardsRID) };
            deck.RecvEvent(args, callback);

            return true;
        }
    }
}
