namespace TouhouHeartstone.Frontend.Model.Witness
{
    public class OnGemChange : WitnessHandler
    {
        public override string Name => "onGemChange";

        protected override bool witnessSuccessHandler(EventWitness witness, DeckController deck, GenericAction callback = null)
        {
            var args = new SetGemEventArgs();
            args.MaxGem = witness.getVar<int>("value");
            args.PlayerID = witness.getVar<int>("playerIndex");

            deck.RecvEvent(args, callback);
            return true;
        }
    }
}
