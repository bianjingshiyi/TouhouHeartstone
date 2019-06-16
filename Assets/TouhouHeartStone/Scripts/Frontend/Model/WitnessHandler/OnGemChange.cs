namespace TouhouHeartstone.Frontend.Model.Witness
{
    public class OnGemChange : WitnessHandler
    {
        public override string Name => "onGemChange";

        protected override bool witnessSuccessHandler(EventWitness witness, DeckController deck, GenericAction callback = null)
        {
            var gem = witness.getVar<int>("value");
            var PlayerID = witness.getVar<int>("playerIndex");
            deck.GetUserBoard(PlayerID).SetGem(-1, gem);
            return false;
        }
    }
}
