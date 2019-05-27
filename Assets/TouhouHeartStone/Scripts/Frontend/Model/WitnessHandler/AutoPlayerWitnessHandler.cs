namespace TouhouHeartstone.Frontend.Model.Witness
{
    abstract class AutoPlayerWitnessHandler : WitnessHandler
    {
        public override bool HandleWitness(EventWitness witness, DeckController deck, GenericAction callback = null)
        {
            AutoPlayerCardEventArgs args = new AutoPlayerCardEventArgs(witness);
            deck.RecvEvent(args, callback);
            return false;
        }
    }
}
