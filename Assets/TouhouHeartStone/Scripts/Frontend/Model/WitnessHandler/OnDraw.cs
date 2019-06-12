namespace TouhouHeartstone.Frontend.Model.Witness
{
    public class OnDraw : WitnessHandler
    {
        public override string Name => "onDraw";

        protected override bool witnessSuccessHandler(EventWitness witness, DeckController deck, GenericAction callback = null)
        {
            int player = witness.getVar<int>("playerIndex");
            int cardRID = witness.getVar<int>("cardRID");
            int cardDID = witness.getVar<int>("cardDID", false);

            deck.GetUserBoard(player).DrawCard(new CardID(cardDID, cardRID), callback);
            return true;
        }
    }
}
