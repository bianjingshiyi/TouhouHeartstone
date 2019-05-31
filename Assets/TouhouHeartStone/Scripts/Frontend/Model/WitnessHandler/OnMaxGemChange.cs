namespace TouhouHeartstone.Frontend.Model.Witness
{
    public class OnMaxGemChange : WitnessHandler
    {
        public override string Name => "onMaxGemChange";

        protected override bool witnessSuccessHandler(EventWitness witness, DeckController deck, GenericAction callback = null)
        {
            var args = new SetGemEventArgs()
            {
                MaxGem = witness.getVar<int>("value"),
                PlayerID = witness.getVar<int>("playerIndex")
            };
            deck.RecvEvent(args, callback);
            return true;
        }
    }
}
