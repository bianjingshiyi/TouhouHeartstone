namespace TouhouHeartstone.Frontend.Model.Witness
{
    public class OnTurnStart : WitnessHandler
    {
        public override string Name => "onTurnStart";

        protected override bool witnessSuccessHandler(EventWitness witness, DeckController deck, GenericAction callback = null)
        {
            var player = witness.getVar<int>("playerIndex");
            deck.TurnStart(player);
            return false;
        }
    }
}
