namespace TouhouHeartstone.Frontend.Model.Witness
{
    public class OnMaxGemChange : WitnessHandler
    {
        public override string Name => "onMaxGemChange";

        public override bool HandleWitness(EventWitness witness, DeckController deck, GenericAction callback = null)
        {
            var args = new SetGemEventArgs()
            {
                MaxGem = witness.getVar<int>("value"),
                PlayerID = witness.getVar<int>("playerIndex")
            };
            deck.RecvEvent(args, callback);
            return false;
        }
    }
}
