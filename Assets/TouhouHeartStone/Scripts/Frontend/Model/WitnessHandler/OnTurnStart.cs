namespace TouhouHeartstone.Frontend.Model.Witness
{
    public class OnTurnStart : WitnessHandler
    {
        public override string Name => "onTurnStart";

        public override bool HandleWitness(EventWitness witness, DeckController deck, GenericAction callback = null)
        {
            var player = witness.getVar<int>("playerIndex");

            var gem = witness.getVar<int>("gem");

            deck.TurnStart(player);
            callback?.Invoke(this, null);

            return false;
        }
    }
}
