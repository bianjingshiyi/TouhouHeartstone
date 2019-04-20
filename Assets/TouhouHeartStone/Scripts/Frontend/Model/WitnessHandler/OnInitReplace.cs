using IGensoukyo.Utilities;

namespace TouhouHeartstone.Frontend.Model.Witness
{
    public class OnInitReplace : WitnessHandler
    {
        public override string Name => "onInitReplace";

        public override bool HandleWitness(EventWitness witness, DeckController deck, GenericAction callback)
        {
            int player = witness.getVar<int>("playerIndex");
            int[] cardsRID = witness.getVar<int[]>("cardsRID");
            int[] cardsDID = witness.getVar<int[]>("cardsDID");

            DebugUtils.NullCheck(cardsRID, "cardsRID");
            if (cardsDID == null)
                cardsDID = new int[cardsRID.Length];
            
            deck.SetInitReplace(player, CardID.ToCardIDs(cardsDID, cardsRID), callback);

            return false;
        }
    }
}
