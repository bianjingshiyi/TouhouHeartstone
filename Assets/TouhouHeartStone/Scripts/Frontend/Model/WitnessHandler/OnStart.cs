namespace TouhouHeartstone.Frontend.Model.Witness
{
    public class OnStart : WitnessHandler
    {
        public override string Name => "onStart";

        public override bool HandleWitness(EventWitness witness, DeckController deck, GenericAction callback = null)
        {
            callback?.Invoke(this, null);
            return false;
        }
    }
}
