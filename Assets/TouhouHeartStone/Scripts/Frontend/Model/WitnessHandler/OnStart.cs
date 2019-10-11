using TouhouCardEngine;

namespace TouhouHeartstone.Frontend.Model.Witness
{
    public class OnStart : WitnessHandler
    {
        public override string Name => "onStart";

        protected override bool witnessSuccessHandler(EventWitness witness, DeckController deck, GenericAction callback = null)
        {
            return false;
        }
    }
}
