namespace TouhouHeartstone.Frontend.Model.Witness
{
    public class OnGemChange : WitnessHandler
    {
        public override string Name => "onGemChange";

        public override bool HandleWitness(EventWitness witness, DeckController deck, GenericAction callback = null)
        {
            var args = new SetGemEventArgs();
            args.MaxGem = witness.getVar<int>("value");
            args.PlayerID = witness.getVar<int>("playerIndex");

            deck.RecvEvent(args, callback);
            return false;
        }
    }
}
