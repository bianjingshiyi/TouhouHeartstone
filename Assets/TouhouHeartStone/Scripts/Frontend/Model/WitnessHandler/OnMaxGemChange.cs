namespace TouhouHeartstone.Frontend.Model.Witness
{
    public class OnMaxGemChange : WitnessHandler
    {
        public override string Name => "onMaxGemChange";

        protected override bool witnessSuccessHandler(EventWitness witness, DeckController deck, GenericAction callback = null)
        {
            var MaxGem = witness.getVar<int>("value");
            var PlayerID = witness.getVar<int>("playerIndex");

            deck.GetUserBoard(PlayerID).SetGem(MaxGem, -1);
            return false;
        }
    }
}
