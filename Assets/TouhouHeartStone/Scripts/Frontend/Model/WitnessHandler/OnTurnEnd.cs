namespace TouhouHeartstone.Frontend.Model.Witness
{
    public class OnTurnEnd : WitnessHandler
    {
        public override string Name => "onTurnEnd";

        protected override bool witnessSuccessHandler(EventWitness witness, DeckController deck, GenericAction callback = null)
        {
            var player = witness.getVar<int>("playerIndex");

            deck.onTurnEnd(player);
            return false;
        }
    }
}
